using ClaimsPortal.Components;
using ClaimsPortal.Services;
using Microsoft.EntityFrameworkCore;
using ClaimsPortal.Data;
using System.IO;
using ClaimsPortal.Models;
using ClaimsPortal.Models.Dto;
using ClaimsPortal.Utils;

var builder = WebApplication.CreateBuilder(args);

// Ensure legacy code page encodings (e.g. windows-1252) are available when reading legacy Word-exported templates
System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Show detailed Blazor circuit errors in development to aid debugging
if (builder.Environment.IsDevelopment())
{
    builder.Services.Configure<Microsoft.AspNetCore.Components.Server.CircuitOptions>(opts =>
    {
        opts.DetailedErrors = true;
    });
}

// Register a DbContext factory for SQL Server (preferred for per-call contexts)
builder.Services.AddDbContextFactory<ClaimsPortalDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("ClaimsConnection")));

// Provide a scoped ClaimsPortalDbContext resolved from the factory so existing scoped
// constructor injections continue to work without creating a direct scoped options dependency
builder.Services.AddScoped(sp => sp.GetRequiredService<Microsoft.EntityFrameworkCore.IDbContextFactory<ClaimsPortalDbContext>>().CreateDbContext());

// Register Database Services (Real implementations)
builder.Services.AddScoped<DatabaseLookupService>();
builder.Services.AddScoped<IDatabasePolicyService, DatabasePolicyService>();
builder.Services.AddScoped<IDatabaseEntityService, DatabaseEntityService>();
builder.Services.AddScoped<IDatabaseClaimService, DatabaseClaimService>();

// Register Vendor Services - Database Connected Implementations
builder.Services.AddScoped<IVendorService, DatabaseVendorService>();
builder.Services.AddScoped<IVendorSearchService, VendorSearchService>();

// Register Mock Services (for features not yet using database)
builder.Services.AddScoped<IPolicyService, MockPolicyService>();
builder.Services.AddScoped<IClaimService, MockClaimService>();
builder.Services.AddScoped<IAdjusterService, MockAdjusterService>();
builder.Services.AddScoped<ILookupService, MockLookupService>();

// Register Letter config store so components can read/write configured letter rules
builder.Services.AddSingleton<LetterConfigStore>();

// Register HttpClientFactory for components that need to make HTTP calls
builder.Services.AddHttpClient();

// Provide a default HttpClient for components with a BaseAddress set to the app's
// current base URI so components can call relative API endpoints like "/api/...".
builder.Services.AddScoped(sp =>
{
    var nav = sp.GetRequiredService<Microsoft.AspNetCore.Components.NavigationManager>();
    return new System.Net.Http.HttpClient { BaseAddress = new Uri(nav.BaseUri) };
});

// Register letter service (used for generating PDFs and rendering templates)
builder.Services.AddScoped<LetterService>();

// Register background worker to process letter generation queue
builder.Services.AddHostedService<LetterGenerationWorker>();

// Letter generation services removed: PDF generation and inline editing disabled per request
// (LetterService, PdfFormService, ITextPdfFormService, LetterGenerationWorker unregistered)

// Register Address Service (using Mock for development, switch to GeocodioAddressService for production)
builder.Services.AddScoped<IAddressService, MockAddressService>();
// For production with Geocodio API:
// builder.Services.AddHttpClient<IAddressService, GeocodioAddressService>();

// Coordinator for shared hospital search modal callbacks
builder.Services.AddSingleton<HospitalSearchCoordinator>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Serve generated files folder for quick access in the UI
var generatedDir = Path.Combine(app.Environment.ContentRootPath, "GeneratedLetters");
Directory.CreateDirectory(generatedDir);
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(generatedDir),
    RequestPath = "/generated"
});

// Minimal endpoint to receive client-side logs from the browser for debugging
app.MapPost("/api/clientlogs", async (HttpRequest req) =>
{
    try
    {
        using var reader = new StreamReader(req.Body);
        var body = await reader.ReadToEndAsync();
        var logsDir = Path.Combine(app.Environment.ContentRootPath, "logs");
        Directory.CreateDirectory(logsDir);
        var filePath = Path.Combine(logsDir, "clientlogs.txt");
        var entry = DateTime.UtcNow.ToString("o") + " " + body + Environment.NewLine;

        // Append robustly using FileStream with FileShare.ReadWrite to avoid crashes
        // when another process has the file open. Retry once on IOException.
        try
        {
            await using var fs = new FileStream(filePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
            using var sw = new StreamWriter(fs) { AutoFlush = true };
            await sw.WriteAsync(entry);
        }
        catch (IOException)
        {
            // Retry once after small delay
            await Task.Delay(100);
            try
            {
                await using var fs2 = new FileStream(filePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
                using var sw2 = new StreamWriter(fs2) { AutoFlush = true };
                await sw2.WriteAsync(entry);
            }
            catch (Exception ex2)
            {
                app.Logger.LogWarning(ex2, "Failed to write client log after retry");
            }
        }

        return Results.Ok();
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "ClientLogs error");
        return Results.StatusCode(500);
    }
});

// API: admin endpoints for letter generation queue
app.MapGet("/api/letterqueue", async (ClaimsPortalDbContext db) =>
{
    var items = await db.LetterGenQueue
        .OrderByDescending(q => q.CreatedAt)
        .Select(q => new {
            q.QueueId,
            q.ClaimNumber,
            q.SelectedRuleIds,
            q.Status,
            q.Tries,
            q.CreatedAt,
            q.LastAttemptAt,
            q.LastError
        }).ToListAsync();
    return Results.Ok(items);
});

app.MapPost("/api/letterqueue/{id:int}/requeue", async (int id, ClaimsPortalDbContext db) =>
{
    var q = await db.LetterGenQueue.FirstOrDefaultAsync(x => x.QueueId == id);
    if (q == null) return Results.NotFound();
    q.Status = "Pending";
    q.Tries = 0;
    q.LastError = null;
    q.LastAttemptAt = null;
    q.CreatedAt = DateTimeOffset.UtcNow;
    await db.SaveChangesAsync();
    return Results.Ok();
});

// List generated letters (files in GeneratedLetters folder)
app.MapGet("/api/generated", (IWebHostEnvironment env) =>
{
    try
    {
        var dir = Path.Combine(env.ContentRootPath, "GeneratedLetters");
        if (!Directory.Exists(dir)) return Results.Ok(Array.Empty<object>());

        var files = Directory.GetFiles(dir)
            .Select(f => new {
                Name = Path.GetFileName(f),
                Url = "/generated/" + System.Net.WebUtility.UrlEncode(Path.GetFileName(f)),
                Size = new FileInfo(f).Length,
                Modified = File.GetLastWriteTimeUtc(f)
            })
            .OrderByDescending(f => f.Modified)
            .ToList();

        return Results.Ok(files);
    }
    catch (Exception ex)
    {
        app.Logger.LogWarning(ex, "Failed to list generated letters");
        return Results.StatusCode(500);
    }
});

// --- Letter form endpoints: extract fields, save/load form-data, flatten to final PDF ---

app.MapPost("/api/letters/fields", async (HttpRequest req, ClaimsPortal.Data.ClaimsPortalDbContext db, IWebHostEnvironment env) =>
{
    // Fields extraction disabled
    return Results.NotFound("Template field extraction has been disabled.");
});

app.MapPost("/api/letters/form/save", async (HttpRequest req, ClaimsPortal.Data.ClaimsPortalDbContext db) =>
{
    // Saving form data disabled
    return Results.NotFound("Form saving has been disabled.");
});

app.MapGet("/api/letters/form/load/{claimNumber}/{documentNumber}", async (string claimNumber, string documentNumber, ClaimsPortal.Data.ClaimsPortalDbContext db) =>
{
    // Loading saved form data disabled
    return Results.NotFound("Form load has been disabled.");
});

app.MapPost("/api/letters/form/flatten", async (HttpRequest req, ClaimsPortal.Data.ClaimsPortalDbContext db, IWebHostEnvironment env) =>
{
    // Editing/flattening disabled
    return Results.NotFound("Editing and PDF flattening functionality has been disabled.");
});

// Preview: render current placeholders to a temporary PDF for in-browser preview
app.MapPost("/api/letters/form/preview", async (HttpRequest req, IWebHostEnvironment env) =>
{
    // Preview generation disabled
    return Results.NotFound("Preview functionality has been disabled.");
});

// Render template output as HTML (substitute placeholders) for in-browser editing
app.MapPost("/api/letters/form/render", async (HttpRequest req, ClaimsPortal.Data.ClaimsPortalDbContext db, IWebHostEnvironment env) =>
{
    try
    {
        using var doc = await System.Text.Json.JsonDocument.ParseAsync(req.Body);
        var root = doc.RootElement;
        var templateName = root.TryGetProperty("templateName", out var t) ? t.GetString() ?? string.Empty : (root.TryGetProperty("TemplateName", out var t2) ? t2.GetString() ?? string.Empty : string.Empty);

        // Load template file
        var templatesDir = Path.Combine(env.ContentRootPath, "wwwroot", "templates");
        var templatePath = Path.Combine(templatesDir, templateName ?? string.Empty);
        if (string.IsNullOrWhiteSpace(templateName) || !System.IO.File.Exists(templatePath))
        {
            return Results.BadRequest("Template not found");
        }

        var html = await System.IO.File.ReadAllTextAsync(templatePath, System.Text.Encoding.GetEncoding(1252));

        // Build placeholder dictionary: support explicit 'fields' object or fall back to small set from claimNumber
        var placeholders = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (root.TryGetProperty("fields", out var fieldsEl) && fieldsEl.ValueKind == System.Text.Json.JsonValueKind.Object)
        {
            foreach (var p in fieldsEl.EnumerateObject())
            {
                placeholders[p.Name] = p.Value.GetString() ?? string.Empty;
            }
        }
        else if (root.TryGetProperty("claimNumber", out var claimEl))
        {
            var claimNumber = claimEl.GetString() ?? string.Empty;
            placeholders["ClaimNumber"] = claimNumber;
            // No DB lookup here - keep placeholder population minimal to avoid coupling to specific DbSet names.
        }

        // perform simple replacements (support formats used in templates)
        foreach (var kv in placeholders)
        {
            var val = kv.Value ?? string.Empty;
            html = html.Replace("{{" + kv.Key + "}}", val);
            html = html.Replace("{{ " + kv.Key + " }}", val);
            html = html.Replace("{" + kv.Key + "}", val);
            html = html.Replace("{ " + kv.Key + " }", val);
        }

        return Results.Content(html, "text/html");
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "Render endpoint error");
        return Results.StatusCode(500);
    }
});

// GET helper for render removed: serving template HTML for inline edit disabled
app.MapGet("/api/letters/form/render/{*templateName}", () => Results.NotFound("GET render route disabled."));

// Save edited HTML for a claim/document (used as source for final flatten)
app.MapPost("/api/letters/form/saveHtml", async (HttpRequest req, ClaimsPortal.Data.ClaimsPortalDbContext db, IWebHostEnvironment env) =>
{
    try
    {
        using var reader = new StreamReader(req.Body);
        var bodyStr = await reader.ReadToEndAsync();
        app.Logger.LogInformation("saveHtml called. body length={len}", bodyStr?.Length ?? 0);
        using var doc = System.Text.Json.JsonDocument.Parse(bodyStr ?? "{}");
        var root = doc.RootElement;
        var claimNumber = root.TryGetProperty("claimNumber", out var c) ? c.GetString() ?? string.Empty : string.Empty;
        var documentNumber = root.TryGetProperty("documentNumber", out var d) ? d.GetString() ?? string.Empty : string.Empty;
        var templateName = root.TryGetProperty("templateName", out var t) ? t.GetString() ?? string.Empty : string.Empty;
        var html = root.TryGetProperty("html", out var h) ? h.GetString() ?? string.Empty : string.Empty;
        var ruleIdStr = root.TryGetProperty("ruleId", out var r) ? r.GetString() ?? string.Empty : string.Empty;

        // Find rule if provided
        long? ruleId = null;
        ClaimsPortal.Data.LetterGenAdminRule? rule = null;
        try
        {
            if (!string.IsNullOrEmpty(ruleIdStr) && long.TryParse(ruleIdStr, out var rid))
            {
                ruleId = rid;
                rule = await db.LetterGenAdminRules.FirstOrDefaultAsync(x => x.Id == rid);
            }
            else if (!string.IsNullOrEmpty(templateName))
            {
                rule = await db.LetterGenAdminRules.FirstOrDefaultAsync(x => x.TemplateFile == templateName || (x.DocumentName != null && (x.DocumentName + ".html") == templateName));
                if (rule != null) ruleId = rule.Id;
            }
        }
        catch (Exception ex)
        {
            app.Logger.LogWarning(ex, "Failed to lookup rule for saveHtml (ruleIdStr={ruleIdStr}, template={templateName})", ruleIdStr, templateName);
        }

        app.Logger.LogInformation("saveHtml payload: claim={claim}, document={docNum}, template={template}, ruleId={ruleId}, htmlLength={htmlLen}", claimNumber, documentNumber, templateName, ruleId, html?.Length ?? 0);

        var outDir = Path.Combine(env.ContentRootPath, "GeneratedLetters");
        if (rule != null && !string.IsNullOrWhiteSpace(rule.Location))
        {
            try { outDir = rule.Location; System.IO.Directory.CreateDirectory(outDir); } catch { outDir = Path.Combine(env.ContentRootPath, "GeneratedLetters"); }
        }
        else
        {
            System.IO.Directory.CreateDirectory(outDir);
        }

        var safeDocName = "ManualLetter";
        if (rule != null && !string.IsNullOrWhiteSpace(rule.DocumentName))
            safeDocName = string.Join("_", rule.DocumentName.Split(System.IO.Path.GetInvalidFileNameChars(), StringSplitOptions.RemoveEmptyEntries)).Replace(' ', '_');
        else if (!string.IsNullOrWhiteSpace(templateName))
            safeDocName = System.IO.Path.GetFileNameWithoutExtension(templateName);

        // derive feature number from documentNumber if present (expecting format Claim_feature)
        var featureSuffix = "";
        int featureNumber = 0;
        if (!string.IsNullOrEmpty(documentNumber) && documentNumber.Contains("_"))
        {
            var parts = documentNumber.Split('_');
            if (int.TryParse(parts.Last(), out var fn))
            {
                featureNumber = fn;
                featureSuffix = $"_F{fn}";
            }
        }

        var filename = $"{safeDocName}_{claimNumber}{featureSuffix}_{DateTime.UtcNow:yyyyMMddHHmmss}.pdf";
        var outPath = Path.Combine(outDir, filename);

        // Generate PDF from HTML
        var letterService = req.HttpContext.RequestServices.GetRequiredService<ClaimsPortal.Services.LetterService>();
        try
        {
            app.Logger.LogInformation("Generating PDF to {outPath}", outPath);
            await letterService.GeneratePdfFromHtmlAsync(html ?? string.Empty, outPath);
            app.Logger.LogInformation("PDF generation completed: {outPath}", outPath);
        }
        catch (Exception ex)
        {
            app.Logger.LogError(ex, "PDF generation failed for claim={claim}, outPath={outPath}", claimNumber, outPath);
            return Results.Problem(detail: "PDF generation failed: " + ex.Message, statusCode: 500);
        }

        // Persist metadata
        try
        {
            var fi = new System.IO.FileInfo(outPath);
            // compute sha256
            string? shaHex = null;
            try
            {
                using var stream = System.IO.File.OpenRead(outPath);
                using var sha256 = System.Security.Cryptography.SHA256.Create();
                var hash = sha256.ComputeHash(stream);
                shaHex = BitConverter.ToString(hash).Replace("-", string.Empty).ToLowerInvariant();
            }
            catch (Exception ex2)
            {
                app.Logger.LogWarning(ex2, "Failed to compute sha256 for file {outPath}", outPath);
            }

            // try to resolve SubClaimId by ClaimNumber + feature number if available
            long? resolvedSubClaimId = null;
            try
            {
                if (featureNumber > 0)
                {
                    var sub = await db.SubClaims.FirstOrDefaultAsync(s => s.ClaimNumber == claimNumber && s.FeatureNumber == featureNumber);
                    if (sub != null) resolvedSubClaimId = sub.SubClaimId;
                }
            }
            catch (Exception ex3)
            {
                app.Logger.LogWarning(ex3, "Failed to lookup SubClaim for claim={claim} feature={feature}", claimNumber, featureNumber);
            }

            var docRec = new ClaimsPortal.Data.LetterGenGeneratedDocument
            {
                Id = Guid.NewGuid(),
                RuleId = ruleId,
                ClaimNumber = claimNumber,
                SubClaimId = resolvedSubClaimId,
                SubClaimFeatureNumber = featureNumber == 0 ? null : (int?)featureNumber,
                DocumentNumber = documentNumber,
                FileName = filename,
                StorageProvider = "filesystem",
                StoragePath = outPath,
                Content = null,
                ContentType = "application/pdf",
                FileSize = fi.Exists ? fi.Length : (long?)null,
                Sha256Hash = shaHex,
                MailTo = rule?.MailTo,
                CreatedBy = null,
                CreatedAt = DateTimeOffset.UtcNow,
                GenerationType = "manual"
            };
            db.LetterGenGeneratedDocuments.Add(docRec);
            await db.SaveChangesAsync();
            app.Logger.LogInformation("Saved LetterGenGeneratedDocument id={id} file={file}", docRec.Id, docRec.StoragePath);
        }
        catch (Exception ex)
        {
            app.Logger.LogWarning(ex, "Failed to save letter metadata for claim={claim}, file={file}", claimNumber, outPath);
        }

        // copy to generated folder served by UI
        try
        {
            var generatedDir = Path.Combine(env.ContentRootPath, "GeneratedLetters");
            System.IO.Directory.CreateDirectory(generatedDir);
            var copyTo = Path.Combine(generatedDir, filename);
            try { System.IO.File.Copy(outPath, copyTo, overwrite: true); } catch { }
        }
        catch { }

        var webUrl = $"/generated/{Uri.EscapeDataString(filename)}";
        return Results.Json(new Dictionary<string, string> { ["url"] = webUrl });
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "saveHtml error");
        return Results.StatusCode(500);
    }
});

// Upload filled PDF: extract AcroForm values and persist them to LetterGen_FormData
app.MapPost("/api/letters/form/upload", async (HttpRequest req, ClaimsPortal.Data.ClaimsPortalDbContext db, IWebHostEnvironment env) =>
{
    // Upload/extract functionality disabled
    return Results.NotFound("Upload and field extraction has been disabled.");
});

// --- Admin: User management endpoints (create user, list roles/groups) ---
app.MapGet("/api/admin/roles", async (ClaimsPortalDbContext db) =>
{
    var roles = await db.UserRoles
        .Select(r => new { r.RoleId, r.RoleName, r.Description })
        .ToListAsync();
    return Results.Ok(roles);
});

app.MapGet("/api/admin/groups", async (ClaimsPortalDbContext db) =>
{
    var groups = await db.AssignmentGroups
        .Select(g => new { g.GroupId, g.GroupCode, g.GroupName, g.Description })
        .ToListAsync();
    return Results.Ok(groups);
});

app.MapPost("/api/admin/users", async (ClaimsPortal.Models.Dto.CreateUserDto dto, ClaimsPortalDbContext db) =>
{
    if (string.IsNullOrWhiteSpace(dto.Username) || string.IsNullOrWhiteSpace(dto.Password))
        return Results.BadRequest("Username and password are required.");

    // check uniqueness
    var exists = await db.Users.AnyAsync(u => u.Username == dto.Username);
    if (exists) return Results.Conflict("Username already exists");

    var hash = PasswordHasher.Hash(dto.Password);

    var user = new ClaimsPortal.Models.User
    {
        Username = dto.Username,
        PasswordHash = hash,
        FullName = dto.FullName,
        Email = dto.Email,
        Telephone = dto.Telephone,
        Extension = dto.Extension,
        Status = dto.Status ?? "Active",
        StartDate = dto.StartDate,
        EndDate = dto.EndDate,
        AssignmentGroupId = dto.AssignmentGroupId,
        RoleId = dto.RoleId,
        SupervisorUserId = dto.SupervisorUserId,
        ExpenseReserve = dto.ExpenseReserve,
        IndemnityReserve = dto.IndemnityReserve,
        ExpensePayment = dto.ExpensePayment,
        IndemnityPayment = dto.IndemnityPayment,
        CreatedBy = dto.CreatedBy,
        CreatedOn = DateTime.UtcNow
    };

    db.Users.Add(user);
    await db.SaveChangesAsync();

    return Results.Created($"/api/admin/users/{user.UserId}", new { user.UserId, user.Username });
});

// List users with optional search/status
app.MapGet("/api/admin/users", async (string? search, string? status, ClaimsPortalDbContext db) =>
{
    var q = db.Users.AsQueryable();
    if (!string.IsNullOrWhiteSpace(search))
    {
        q = q.Where(u => u.Username.Contains(search) || (u.FullName != null && u.FullName.Contains(search)) || (u.Email != null && u.Email.Contains(search)));
    }
    if (!string.IsNullOrWhiteSpace(status))
    {
        q = q.Where(u => u.Status == status);
    }

    var list = await q.OrderBy(u => u.Username)
        .Select(u => new {
            u.UserId,
            u.Username,
            u.FullName,
            u.Email,
            u.Telephone,
            u.Extension,
            u.Status,
            u.StartDate,
            u.EndDate,
            AssignmentGroup = u.AssignmentGroup != null ? new { u.AssignmentGroup.GroupId, u.AssignmentGroup.GroupName } : null,
            Role = u.Role != null ? new { u.Role.RoleId, u.Role.RoleName } : null,
            u.SupervisorUserId,
            u.ExpenseReserve,
            u.IndemnityReserve,
            u.ExpensePayment,
            u.IndemnityPayment
        }).ToListAsync();

    return Results.Ok(list);
});

// Get single user
app.MapGet("/api/admin/users/{id:long}", async (long id, ClaimsPortalDbContext db) =>
{
    var u = await db.Users
        .Include(x => x.AssignmentGroup)
        .Include(x => x.Role)
        .FirstOrDefaultAsync(x => x.UserId == id);
    if (u == null) return Results.NotFound();

    return Results.Ok(new {
        u.UserId,
        u.Username,
        u.FullName,
        u.Email,
        u.Telephone,
        u.Extension,
        u.Status,
        u.StartDate,
        u.EndDate,
        AssignmentGroup = u.AssignmentGroup != null ? new { u.AssignmentGroup.GroupId, u.AssignmentGroup.GroupName } : null,
        Role = u.Role != null ? new { u.Role.RoleId, u.Role.RoleName } : null,
        u.SupervisorUserId,
        u.ExpenseReserve,
        u.IndemnityReserve,
        u.ExpensePayment,
        u.IndemnityPayment,
        u.CreatedBy,
        u.CreatedOn
    });
});

// Update user
app.MapPut("/api/admin/users/{id:long}", async (long id, ClaimsPortal.Models.Dto.UpdateUserDto dto, ClaimsPortalDbContext db) =>
{
    var u = await db.Users.FirstOrDefaultAsync(x => x.UserId == id);
    if (u == null) return Results.NotFound();

    if (!string.IsNullOrWhiteSpace(dto.Password))
    {
        u.PasswordHash = PasswordHasher.Hash(dto.Password);
    }
    if (dto.FullName != null) u.FullName = dto.FullName;
    if (dto.Email != null) u.Email = dto.Email;
    if (dto.Telephone != null) u.Telephone = dto.Telephone;
    if (dto.Extension != null) u.Extension = dto.Extension;
    if (dto.Status != null) u.Status = dto.Status;
    if (dto.StartDate.HasValue) u.StartDate = dto.StartDate.Value;
    if (dto.EndDate.HasValue) u.EndDate = dto.EndDate;
    if (dto.AssignmentGroupId.HasValue) u.AssignmentGroupId = dto.AssignmentGroupId;
    if (dto.RoleId.HasValue) u.RoleId = dto.RoleId;
    if (dto.SupervisorUserId.HasValue) u.SupervisorUserId = dto.SupervisorUserId;
    if (dto.ExpenseReserve.HasValue) u.ExpenseReserve = dto.ExpenseReserve;
    if (dto.IndemnityReserve.HasValue) u.IndemnityReserve = dto.IndemnityReserve;
    if (dto.ExpensePayment.HasValue) u.ExpensePayment = dto.ExpensePayment;
    if (dto.IndemnityPayment.HasValue) u.IndemnityPayment = dto.IndemnityPayment;

    await db.SaveChangesAsync();
    return Results.Ok();
});

app.Run();