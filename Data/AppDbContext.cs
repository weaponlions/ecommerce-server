using eShopServer.Models;
using Microsoft.EntityFrameworkCore;

namespace eShopServer.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // ── Dashboard ──
    public DbSet<DashboardSection> DashboardSections => Set<DashboardSection>();
    public DbSet<NavbarLink> NavbarLinks => Set<NavbarLink>();
    public DbSet<CarouselSlide> CarouselSlides => Set<CarouselSlide>();
    public DbSet<Collection> Collections => Set<Collection>();
    public DbSet<FooterLink> FooterLinks => Set<FooterLink>();
    public DbSet<SocialIcon> SocialIcons => Set<SocialIcon>();

    // ── Products ──
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<CategoryAttribute> CategoryAttributes => Set<CategoryAttribute>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductAttributeValue> ProductAttributeValues => Set<ProductAttributeValue>();
    public DbSet<RecentlyVisitedProduct> RecentlyVisitedProducts => Set<RecentlyVisitedProduct>();

    // ── Media ──
    public DbSet<MediaAsset> MediaAssets => Set<MediaAsset>();
    public DbSet<MediaUsage> MediaUsages => Set<MediaUsage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ── Dashboard Sections ──
        modelBuilder.Entity<DashboardSection>()
            .HasIndex(s => s.SectionKey)
            .IsUnique();

        // ── Recently Visited ──
        modelBuilder.Entity<RecentlyVisitedProduct>()
            .HasIndex(r => new { r.UserId, r.ProductId });

        modelBuilder.Entity<RecentlyVisitedProduct>()
            .HasOne(r => r.Product)
            .WithMany()
            .HasForeignKey(r => r.ProductId);

        // ── Product pricing precision ──
        modelBuilder.Entity<Product>()
            .Property(p => p.Price)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Product>()
            .Property(p => p.OriginalPrice)
            .HasPrecision(18, 2);

        // ── Category ──
        modelBuilder.Entity<Category>()
            .HasIndex(c => c.Slug)
            .IsUnique();

        modelBuilder.Entity<Category>()
            .HasMany(c => c.Products)
            .WithOne(p => p.Category)
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Category>()
            .HasMany(c => c.Attributes)
            .WithOne(a => a.Category)
            .HasForeignKey(a => a.CategoryId)
            .OnDelete(DeleteBehavior.Cascade);

        // ── Category Attributes ──
        modelBuilder.Entity<CategoryAttribute>()
            .HasIndex(a => new { a.CategoryId, a.Name })
            .IsUnique();

        // ── Product Attribute Values ──
        modelBuilder.Entity<ProductAttributeValue>()
            .HasIndex(v => new { v.ProductId, v.CategoryAttributeId })
            .IsUnique();

        modelBuilder.Entity<ProductAttributeValue>()
            .HasOne(v => v.Product)
            .WithMany(p => p.AttributeValues)
            .HasForeignKey(v => v.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ProductAttributeValue>()
            .HasOne(v => v.CategoryAttribute)
            .WithMany(a => a.Values)
            .HasForeignKey(v => v.CategoryAttributeId)
            .OnDelete(DeleteBehavior.Cascade);

        // ── Media Asset ──
        modelBuilder.Entity<MediaAsset>()
            .HasIndex(m => m.Category);

        modelBuilder.Entity<MediaAsset>()
            .HasIndex(m => m.FileName)
            .IsUnique();

        // ── Media Usage ──
        modelBuilder.Entity<MediaUsage>()
            .HasIndex(u => new { u.EntityType, u.EntityId });

        modelBuilder.Entity<MediaUsage>()
            .HasIndex(u => new { u.MediaAssetId, u.EntityType, u.EntityId, u.FieldName })
            .IsUnique();

        modelBuilder.Entity<MediaUsage>()
            .HasOne(u => u.MediaAsset)
            .WithMany(a => a.Usages)
            .HasForeignKey(u => u.MediaAssetId)
            .OnDelete(DeleteBehavior.Cascade);

        // ── Entity → MediaAsset FKs (SetNull on delete) ──
        modelBuilder.Entity<CarouselSlide>()
            .HasOne(e => e.MediaAsset)
            .WithMany()
            .HasForeignKey(e => e.MediaAssetId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Collection>()
            .HasOne(e => e.MediaAsset)
            .WithMany()
            .HasForeignKey(e => e.MediaAssetId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Product>()
            .HasOne(e => e.MediaAsset)
            .WithMany()
            .HasForeignKey(e => e.MediaAssetId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Category>()
            .HasOne(e => e.MediaAsset)
            .WithMany()
            .HasForeignKey(e => e.MediaAssetId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<SocialIcon>()
            .HasOne(e => e.MediaAsset)
            .WithMany()
            .HasForeignKey(e => e.MediaAssetId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
