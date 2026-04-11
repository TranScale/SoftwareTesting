using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Net.Http.Headers;
using MySqlConnector;
using System.Linq;
using OnlineSalesManagementSystem.Data;
using OnlineSalesManagementSystem.Domain.Entities;
using OnlineSalesManagementSystem.Helpers;

// ✅ Resolve ambiguous references if OSMS.PermissionPatch is also in the solution
using PermissionPolicyProvider = OnlineSalesManagementSystem.Services.Security.PermissionPolicyProvider;
using PermissionAuthorizationHandler = OnlineSalesManagementSystem.Services.Security.PermissionAuthorizationHandler;
using IPermissionService = OnlineSalesManagementSystem.Services.Security.IPermissionService;
using PermissionService = OnlineSalesManagementSystem.Services.Security.PermissionService;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
});
builder.Services.AddMemoryCache();
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
});
builder.Services.Configure<BrotliCompressionProviderOptions>(options =>
{
    options.Level = System.IO.Compression.CompressionLevel.Fastest;
});
builder.Services.Configure<GzipCompressionProviderOptions>(options =>
{
    options.Level = System.IO.Compression.CompressionLevel.Fastest;
});

// EF Core
var connectionString = builder.Configuration.GetConnectionString("MySqlConnection")
    ?? builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "Connection string 'MySqlConnection' or 'DefaultConnection' was not found.");

var tidbPassword = builder.Configuration["TiDB:Password"];
if (!string.IsNullOrWhiteSpace(tidbPassword))
{
    connectionString = connectionString.Replace("<TIDB_PASSWORD>", tidbPassword, StringComparison.Ordinal);
}

if (connectionString.Contains("<TIDB_PASSWORD>", StringComparison.Ordinal))
{
    throw new InvalidOperationException(
        "TiDB password is missing. Set 'TiDB:Password' (User Secrets / Environment / appsettings.Development.json).");
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseMySql(
        connectionString,
        new MySqlServerVersion(new Version(8, 0, 11)),
        mysql => mysql.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null));
});

// Identity (cookie auth)
builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;

        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequiredLength = 8;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Admin/Auth/Login";
    options.LogoutPath = "/Admin/Auth/Logout";
    options.AccessDeniedPath = "/Admin/Auth/AccessDenied";

    options.Cookie.Name = "OSMS.Auth";
    options.SlidingExpiration = true;
});

// Authorization: permission-based policies
builder.Services.AddAuthorization();

// ✅ Use ONLY ONE policy provider (avoid conflicts/ambiguity)
builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
builder.Services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
builder.Services.AddScoped<IPermissionService, PermissionService>();

builder.Services.AddScoped<OnlineSalesManagementSystem.Services.Inventory.IStockService, OnlineSalesManagementSystem.Services.Inventory.StockService>();
builder.Services.AddScoped<OnlineSalesManagementSystem.Services.Sales.IInvoiceTotalsService, OnlineSalesManagementSystem.Services.Sales.InvoiceTotalsService>();

var app = builder.Build();

// Auto-initialize database + seed (FAIL FAST)
await using (var scope = app.Services.CreateAsyncScope())
{
    var sp = scope.ServiceProvider;
    var logger = sp.GetRequiredService<ILogger<Program>>();

    try
    {
        var db = sp.GetRequiredService<ApplicationDbContext>();
        var userManager = sp.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = sp.GetRequiredService<RoleManager<IdentityRole>>();
        var initMode = builder.Configuration.GetValue<string>("Database:InitializationMode") ?? "EnsureCreatedOnce";
        var autoResetOnMissingTables = builder.Configuration.GetValue("Database:AutoResetOnMissingTables", true);
        var forceSeed = builder.Configuration.GetValue("Seed:Force", false);
        var resetDemoData = builder.Configuration.GetValue("Seed:ResetDemoData", false);
        var shouldSeed = false;
        var dbTarget = DatabaseConnectionDisplay.FromConnectionString(connectionString);

        logger.LogInformation(
            "Current database target => {Engine} | {Database} | {Host}",
            dbTarget.Engine,
            dbTarget.Database,
            dbTarget.Host);

        if (initMode.Equals("Migrate", StringComparison.OrdinalIgnoreCase))
        {
            logger.LogInformation("Applying EF Core migrations...");
            await db.Database.MigrateAsync();
            shouldSeed = true;
        }
        else if (initMode.Equals("EnsureCreated", StringComparison.OrdinalIgnoreCase))
        {
            logger.LogInformation("Ensuring database is created from current model (Code First)...");
            await db.Database.EnsureCreatedAsync();
            shouldSeed = true;
        }
        else if (initMode.Equals("EnsureCreatedOnce", StringComparison.OrdinalIgnoreCase))
        {
            logger.LogInformation("Ensuring database is created once from current model (Code First)...");
            var createdNow = await db.Database.EnsureCreatedAsync();
            shouldSeed = createdNow;

            // If a previous EnsureCreated failed halfway, EF sees "database has tables"
            // and won't create missing tables on next runs. Recover automatically once.
            try
            {
                _ = await db.Users.AsNoTracking().AnyAsync();
            }
            catch (MySqlException ex) when (ex.Number == 1146 && autoResetOnMissingTables)
            {
                logger.LogWarning(ex,
                    "Detected partially created TiDB schema (missing table). Recreating schema once...");

                await db.Database.EnsureDeletedAsync();
                await db.Database.EnsureCreatedAsync();
                shouldSeed = true;
            }
            catch (MySqlException ex) when (ex.Number == 1146 && !autoResetOnMissingTables)
            {
                throw new InvalidOperationException(
                    "Database schema is partial (missing AspNetUsers). " +
                    "Enable Database:AutoResetOnMissingTables or reset DB manually.", ex);
            }

            if (!shouldSeed)
            {
                if (forceSeed || resetDemoData)
                {
                    shouldSeed = true;
                    logger.LogInformation(
                        "Schema already exists. Running seeding because Seed:Force={ForceSeed} or Seed:ResetDemoData={ResetDemoData}.",
                        forceSeed,
                        resetDemoData);
                }
                else
                {
                    logger.LogInformation("Schema already exists. Skipping Code First initialization + seeding.");
                }
            }
        }
        else if (initMode.Equals("None", StringComparison.OrdinalIgnoreCase))
        {
            logger.LogInformation("Database initialization is disabled (InitializationMode=None).");
            shouldSeed = false;
        }
        else
        {
            throw new InvalidOperationException(
                $"Unsupported Database:InitializationMode '{initMode}'. Use 'EnsureCreatedOnce', 'EnsureCreated', 'Migrate', or 'None'.");
        }

        if (shouldSeed)
        {
            // Sanity check: nếu bảng Identity thiếu, nổ ngay tại đây cho dễ debug
            _ = await db.Users.AsNoTracking().AnyAsync();

            logger.LogInformation("Seeding database...");

            // Optional: reseed dữ liệu demo mà không cần drop DB
            // Bật bằng appsettings.Development.json:
            //   "Seed": { "ResetDemoData": true }
            await DbSeeder.SeedAsync(db, userManager, roleManager, resetDemoData);

            logger.LogInformation("Database initialization + seeding done.");
        }
    }
    catch (Exception ex)
    {
        // QUAN TRỌNG: Đừng nuốt lỗi, vì app chạy tiếp sẽ gây lỗi lắt nhắt như AspNetUsers not found
        logger.LogCritical(ex, "Migration/Seeding FAILED. Application will stop.");
        throw;
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseResponseCompression();
app.UseHttpsRedirection();
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = context =>
    {
        var ext = Path.GetExtension(context.File.Name);
        if (string.IsNullOrWhiteSpace(ext))
        {
            return;
        }

        var longCacheExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".css", ".js", ".jpg", ".jpeg", ".png", ".webp", ".svg", ".ico", ".woff", ".woff2", ".ttf"
        };

        if (longCacheExtensions.Contains(ext))
        {
            context.Context.Response.Headers[HeaderNames.CacheControl] = "public,max-age=2592000,immutable";
        }
    }
});

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapGet("/", () => Results.Redirect("/Admin/Auth/Login", permanent: false))
    .AllowAnonymous();

// Areas first (Admin)
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

// Default (public site)
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
