using ClaimsPortal.Components;
using ClaimsPortal.Services;
using Microsoft.EntityFrameworkCore;
using ClaimsPortal.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Register DbContext for SQL Server
// For Blazor Server, use AddDbContext with proper scoping
builder.Services.AddDbContext<ClaimsPortalDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("ClaimsConnection")));

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

// Register Address Service (using Mock for development, switch to GeocodioAddressService for production)
builder.Services.AddScoped<IAddressService, MockAddressService>();
// For production with Geocodio API:
// builder.Services.AddHttpClient<IAddressService, GeocodioAddressService>();

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

app.Run();