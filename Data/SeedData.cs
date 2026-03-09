using eShopServer.Models;
using System.IO;
using System.Net.Http;

namespace eShopServer.Data;

public static class SeedData
{
    public static void Initialize(AppDbContext db)
    {
        if (db.DashboardSections.Any()) return; // Already seeded

        var webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        var uploadsDir = Path.Combine(webRootPath, "uploads");
        if (!Directory.Exists(uploadsDir)) Directory.CreateDirectory(uploadsDir);

        using var client = new HttpClient();
        
        // Helper to download image
        MediaAsset EnsureMedia(string filename, string defaultUrl, string category, string alt, string title)
        {
            var path = Path.Combine(uploadsDir, filename);
            if (!File.Exists(path))
            {
                try {
                    var bytes = client.GetByteArrayAsync(defaultUrl).GetAwaiter().GetResult();
                    File.WriteAllBytes(path, bytes);
                } catch {
                    var dummy = Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mNkYAAAAAYAAjCB0C8AAAAASUVORK5CYII=");
                    File.WriteAllBytes(path, dummy);
                }
            }
            var fi = new FileInfo(path);
            var asset = db.MediaAssets.FirstOrDefault(m => m.FileName == filename);
            if (asset == null) {
                asset = new MediaAsset(filename, filename, "image/jpeg", fi.Length, $"/uploads/{filename}")
                {
                    Category = category, AltText = alt, Title = title
                };
                db.MediaAssets.Add(asset);
                db.SaveChanges(); // Persist to assign Id
            }
            return asset;
        }

        void TrackUsage(int assetId, string entityType, int entityId, string propName) {
            db.MediaUsages.Add(new MediaUsage { MediaAssetId = assetId, EntityType = entityType, EntityId = entityId, FieldName = propName });
        }

        // ── Generate Media Assets ──
        var c1Asset = EnsureMedia("carousel_1.jpg", "https://picsum.photos/seed/surf/1920/600", "carousel", "Summer Collection", "Summer Collection");
        var c2Asset = EnsureMedia("carousel_2.jpg", "https://picsum.photos/seed/tech/1920/600", "carousel", "New Arrivals", "New Arrivals");
        var c3Asset = EnsureMedia("carousel_3.jpg", "https://picsum.photos/seed/watch/1920/600", "carousel", "Electronics", "Electronics");

        var col1Asset = EnsureMedia("col_summer.jpg", "https://picsum.photos/seed/col1/800/800", "collection", "Summer", "Summer");
        var col2Asset = EnsureMedia("col_wfh.jpg", "https://picsum.photos/seed/col2/800/800", "collection", "WFH", "WFH");

        var p1Listing = EnsureMedia("p1_listing.jpg", "https://picsum.photos/seed/p1/800/800", "product", "White Sneakers", "Sneakers Listing");
        var p1Gallery = EnsureMedia("p1_gallery.jpg", "https://picsum.photos/seed/p1a/800/800", "product", "White Sneakers Side", "Sneakers Side");
        var p1Hover = EnsureMedia("p1_hover.jpg", "https://picsum.photos/seed/p1b/800/800", "product", "White Sneakers Top", "Sneakers Top");

        var p2Listing = EnsureMedia("p2_listing.jpg", "https://picsum.photos/seed/p2/800/800", "product", "Headphones", "Headphones Listing");
        var p3Listing = EnsureMedia("p3_listing.jpg", "https://picsum.photos/seed/p3/800/800", "product", "Watch", "Watch Listing");

        var socialFb = EnsureMedia("icon_fb.png", "https://picsum.photos/seed/fb/64/64", "social-icon", "Facebook", "Facebook");

        // ── Dashboard Sections ──
        db.DashboardSections.AddRange(
            new DashboardSection("navbar",           "Navigation",               1),
            new DashboardSection("carousel",         "Hero Carousel",            2),
            new DashboardSection("trending",         "Trending Now",             3) { LayoutHint = "grid-4" },
            new DashboardSection("recently_visited", "Recently Visited",         4) { LayoutHint = "scroll-horizontal" },
            new DashboardSection("collections",      "Most Visited Collections", 5) { LayoutHint = "grid-3" },
            new DashboardSection("footer",           "Footer",                   6)
        );

        // ── Navbar Links ──
        db.NavbarLinks.AddRange(
            new NavbarLink("Home",        "/",            1) { Icon = "home" },
            new NavbarLink("Shop",        "/shop",        2) { Icon = "storefront" },
            new NavbarLink("About",       "/about",       8) { Icon = "info" }
        );

        // ── Carousel Slides ──
        var s1 = new CarouselSlide("Summer Collection 2026", 1) { Subtitle = "Up to 50% off", LinkUrl = "/shop/summer", ButtonText = "Shop Now", MediaAssetId = c1Asset.Id };
        var s2 = new CarouselSlide("New Arrivals", 2) { Subtitle = "Check out the latest", LinkUrl = "/shop/new", ButtonText = "Explore", MediaAssetId = c2Asset.Id };
        var s3 = new CarouselSlide("Premium Electronics", 3) { Subtitle = "Tech that redefines", LinkUrl = "/shop/electronics", ButtonText = "Discover", MediaAssetId = c3Asset.Id };
        db.CarouselSlides.AddRange(s1, s2, s3);
        db.SaveChanges();
        TrackUsage(c1Asset.Id, "CarouselSlide", s1.Id, "MediaAssetId");
        TrackUsage(c2Asset.Id, "CarouselSlide", s2.Id, "MediaAssetId");
        TrackUsage(c3Asset.Id, "CarouselSlide", s3.Id, "MediaAssetId");

        // ── Products ──
        var prod1 = new Product("Classic White Sneakers", 89.99m) { OriginalPrice = 129.99m, CategoryLabel = "Footwear", Badge = "Hot", Rating = 4.5, ReviewCount = 342, TrendingScore = 95 };
        var prod2 = new Product("Wireless Noise-Cancelling", 249.99m) { OriginalPrice = 349.99m, CategoryLabel = "Electronics", Badge = "Bestseller", Rating = 4.8, ReviewCount = 1205, TrendingScore = 98 };
        var prod3 = new Product("Minimalist Leather Watch", 159.99m) { CategoryLabel = "Accessories", Badge = "New", Rating = 4.6, ReviewCount = 89, TrendingScore = 82 };
        
        db.Products.AddRange(prod1, prod2, prod3);
        db.SaveChanges(); // Need IDs for Images

        // ── Product Images ──
        var pi1 = new ProductImage(prod1.Id, p1Listing.Id, "listing", 1) { IsActive = true, AltText = "Listing Image" };
        var pi2 = new ProductImage(prod1.Id, p1Gallery.Id, "gallery", 2) { IsActive = true, AltText = "Side Angle" };
        var pi3 = new ProductImage(prod1.Id, p1Hover.Id, "hover", 3) { IsActive = true, AltText = "On Hover" };
        
        var pi4 = new ProductImage(prod2.Id, p2Listing.Id, "listing", 1) { IsActive = true, AltText = "Listing Image" };
        var pi5 = new ProductImage(prod3.Id, p3Listing.Id, "listing", 1) { IsActive = true, AltText = "Listing Image" };

        db.ProductImages.AddRange(pi1, pi2, pi3, pi4, pi5);
        db.SaveChanges();

        TrackUsage(p1Listing.Id, "ProductImage", pi1.Id, "MediaAssetId");
        TrackUsage(p1Gallery.Id, "ProductImage", pi2.Id, "MediaAssetId");
        TrackUsage(p1Hover.Id, "ProductImage", pi3.Id, "MediaAssetId");
        TrackUsage(p2Listing.Id, "ProductImage", pi4.Id, "MediaAssetId");
        TrackUsage(p3Listing.Id, "ProductImage", pi5.Id, "MediaAssetId");

        // ── Collections ──
        var coll1 = new Collection("Summer Essentials", 1) { Description = "Beat the heat", LinkUrl = "/collections/summer", VisitCount = 12500, MediaAssetId = col1Asset.Id };
        var coll2 = new Collection("Work From Home", 2) { Description = "Comfort comes first", LinkUrl = "/collections/wfh", VisitCount = 9800, MediaAssetId = col2Asset.Id };
        db.Collections.AddRange(coll1, coll2);
        db.SaveChanges();
        TrackUsage(col1Asset.Id, "Collection", coll1.Id, "MediaAssetId");
        TrackUsage(col2Asset.Id, "Collection", coll2.Id, "MediaAssetId");

        // ── Footer Links ──
        db.FooterLinks.AddRange(new FooterLink("Company", "About Us", "/about", 1));

        // ── Social Icons ──
        var soc1 = new SocialIcon("facebook", "fab fa-facebook-f", "https://facebook.com/eshop", 1) { MediaAssetId = socialFb.Id };
        db.SocialIcons.Add(soc1);
        db.SaveChanges();
        TrackUsage(socialFb.Id, "SocialIcon", soc1.Id, "MediaAssetId");

        db.SaveChanges();
    }
}
