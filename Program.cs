using eShopServer.Data;
using eShopServer.Interfaces.Repositories;
using eShopServer.Interfaces.Services;
using eShopServer.Middleware;
using eShopServer.Repositories;
using eShopServer.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders(); // optional: remove default providers
builder.Logging.AddConsole();
builder.Logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Warning);
builder.Logging.AddFilter("Microsoft.EntityFrameworkCore", LogLevel.Warning); // optional: all EF logs

// ── Rate Limiting & Compression (Production) ──
builder.Services.AddResponseCompression(options => { options.EnableForHttps = true; });
builder.Services.AddRateLimiter(options => {
    options.GlobalLimiter = System.Threading.RateLimiting.PartitionedRateLimiter.Create<HttpContext, string>(context =>
        System.Threading.RateLimiting.RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: partition => new System.Threading.RateLimiting.FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 100,
                QueueLimit = 2,
                Window = TimeSpan.FromMinutes(1)
            }));
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});


// ── Database (MySQL via Pomelo) ──
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

if (!builder.Environment.IsEnvironment("Testing"))
{
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString), 
            mySqlOptions => mySqlOptions.EnableRetryOnFailure(
                maxRetryCount: 3, 
                maxRetryDelay: TimeSpan.FromSeconds(5), 
                errorNumbersToAdd: null))
            .EnableSensitiveDataLogging(builder.Environment.IsDevelopment()));
}

// ── Repositories (scoped) ──
builder.Services.AddScoped<IDashboardSectionRepository, DashboardSectionRepository>();
builder.Services.AddScoped<INavbarLinkRepository, NavbarLinkRepository>();
builder.Services.AddScoped<ICarouselSlideRepository, CarouselSlideRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IRecentlyVisitedProductRepository, RecentlyVisitedProductRepository>();
builder.Services.AddScoped<ICollectionRepository, CollectionRepository>();
builder.Services.AddScoped<IFooterLinkRepository, FooterLinkRepository>();
builder.Services.AddScoped<ISocialIconRepository, SocialIconRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<ICategoryAttributeRepository, CategoryAttributeRepository>();
builder.Services.AddScoped<IProductAttributeValueRepository, ProductAttributeValueRepository>();
builder.Services.AddScoped<IProductCollectionRepository, ProductCollectionRepository>();
builder.Services.AddScoped<IMediaAssetRepository, MediaAssetRepository>();
builder.Services.AddScoped<IMediaUsageRepository, MediaUsageRepository>();
builder.Services.AddScoped<IProductImageRepository, ProductImageRepository>();

// ── Services (scoped) ──
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IMediaService, MediaService>();

// ── Controllers + OpenAPI ──
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// ── CORS ──
var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        if (builder.Environment.IsDevelopment() || allowedOrigins.Length == 0)
        {
            policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
        }
        else
        {
            policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod().AllowCredentials();
        }
    });
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    if (db.Database.IsRelational())
    {
        db.Database.Migrate();
    }
}

// ── Seed database ──
using (var scope = app.Services.CreateScope())
{
    // var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    // db.Database.EnsureCreated();
    // SeedData.Initialize(db);
}

// ── Middleware ──
app.UseGlobalExceptionHandler(); // Must be first — catches all downstream exceptions
app.UseResponseCompression();
app.UseRateLimiter();

if (app.Environment.IsDevelopment())
{
    // app.MapOpenApi();
}

app.MapOpenApi();
app.UseSwagger();
app.UseSwaggerUI();


app.UseHttpsRedirection();
app.UseStaticFiles(); // Serve uploaded media from wwwroot/uploads
app.UseCors();

// ── Map controllers ──
app.MapControllers();

app.MapGet("/", () =>
    Results.Content(
        "Server is running move to <a href=\"/swagger\">/swagger</a>",
        "text/html"));

app.Run();

public partial class Program { }
