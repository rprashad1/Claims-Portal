using ClaimsPortal.Components;
using ClaimsPortal.Services;
using Microsoft.EntityFrameworkCore;
using ClaimsPortal.Data;
using System.IO;

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

// Register HttpClientFactory for components that need to make HTTP calls
builder.Services.AddHttpClient();

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

app.MapPost("/api/letters/form/flatten", async (HttpRequest req, ClaimsPortal.Data.ClaimsPortalDbContext db, ClaimsPortal.Services.LetterService letterService, ClaimsPortal.Services.ITextPdfFormService iTextService, IWebHostEnvironment env) =>
{
    // Editing/flattening disabled
    return Results.NotFound("Editing and PDF flattening functionality has been disabled.");
});

// Preview: render current placeholders to a temporary PDF for in-browser preview
app.MapPost("/api/letters/form/preview", async (HttpRequest req, ClaimsPortal.Services.LetterService letterService, IWebHostEnvironment env) =>
{
    // Preview generation disabled
    return Results.NotFound("Preview functionality has been disabled.");
});

// Render template output as HTML (substitute placeholders) for in-browser editing
app.MapPost("/api/letters/form/render", async (HttpRequest req, IWebHostEnvironment env) =>
{
    // Rendering for inline edit disabled
    return Results.NotFound("Rendering for inline edit has been disabled.");
});

// GET helper for render removed: serving template HTML for inline edit disabled
app.MapGet("/api/letters/form/render/{*templateName}", () => Results.NotFound("GET render route disabled."));

// Save edited HTML for a claim/document (used as source for final flatten)
app.MapPost("/api/letters/form/saveHtml", async (HttpRequest req, IWebHostEnvironment env) =>
{
    // Saving edited HTML disabled
    return Results.NotFound("Saving edited HTML has been disabled.");
});

// Upload filled PDF: extract AcroForm values and persist them to LetterGen_FormData
app.MapPost("/api/letters/form/upload", async (HttpRequest req, ClaimsPortal.Data.ClaimsPortalDbContext db, ClaimsPortal.Services.ITextPdfFormService iTextService, IWebHostEnvironment env) =>
{
    // Upload/extract functionality disabled
    return Results.NotFound("Upload and field extraction has been disabled.");
});

app.Run();