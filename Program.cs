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


// ── Database (MySQL via Pomelo) ──
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)).EnableSensitiveDataLogging(false));

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

// ── Services (scoped) ──
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IMediaService, MediaService>();

// ── Controllers + OpenAPI ──
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// ── CORS ──
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod());
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// ── Seed database ──
// using (var scope = app.Services.CreateScope())
// {
//     var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
//     db.Database.EnsureCreated();
//     SeedData.Initialize(db);
// }

// ── Middleware ──
app.UseGlobalExceptionHandler(); // Must be first — catches all downstream exceptions

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

app.Run();
