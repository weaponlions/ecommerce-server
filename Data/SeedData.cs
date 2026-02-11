using eShopServer.Models;

namespace eShopServer.Data;

public static class SeedData
{
    public static void Initialize(AppDbContext db)
    {
        if (db.DashboardSections.Any()) return; // Already seeded

        // ── Dashboard Sections (controls order & visibility) ──
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
            new NavbarLink("Men",         "/shop/men",    3) { ParentId = 2 },
            new NavbarLink("Women",       "/shop/women",  4) { ParentId = 2 },
            new NavbarLink("Kids",        "/shop/kids",   5) { ParentId = 2 },
            new NavbarLink("Collections", "/collections", 6) { Icon = "category" },
            new NavbarLink("Deals",       "/deals",       7) { Icon = "local_offer" },
            new NavbarLink("About",       "/about",       8) { Icon = "info" }
        );

        // ── Carousel Slides ──
        db.CarouselSlides.AddRange(
            new CarouselSlide("Summer Collection 2026", "https://images.unsplash.com/photo-1441986300917-64674bd600d8?w=1920", 1)
            {
                Subtitle = "Up to 50% off on all summer essentials",
                LinkUrl = "/shop/summer",
                ButtonText = "Shop Now"
            },
            new CarouselSlide("New Arrivals", "https://images.unsplash.com/photo-1472851294608-062f824d29cc?w=1920", 2)
            {
                Subtitle = "Check out the latest trends",
                LinkUrl = "/shop/new",
                ButtonText = "Explore"
            },
            new CarouselSlide("Premium Electronics", "https://images.unsplash.com/photo-1498049794561-7780e7231661?w=1920", 3)
            {
                Subtitle = "Tech that redefines your life",
                LinkUrl = "/shop/electronics",
                ButtonText = "Discover"
            }
        );

        // ── Products ──
        db.Products.AddRange(
            new Product("Classic White Sneakers",    89.99m, "https://images.unsplash.com/photo-1549298916-b41d501d3772?w=400")
                { OriginalPrice = 129.99m, CategoryLabel = "Footwear",    Badge = "Hot",        Rating = 4.5, ReviewCount = 342,  TrendingScore = 95 },
            new Product("Wireless Noise-Cancelling", 249.99m, "https://images.unsplash.com/photo-1505740420928-5e560c06d30e?w=400")
                { OriginalPrice = 349.99m, CategoryLabel = "Electronics", Badge = "Bestseller", Rating = 4.8, ReviewCount = 1205, TrendingScore = 98 },
            new Product("Minimalist Leather Watch",  159.99m, "https://images.unsplash.com/photo-1524592094714-0f0654e20314?w=400")
                { CategoryLabel = "Accessories", Badge = "New",        Rating = 4.6, ReviewCount = 89,  TrendingScore = 82 },
            new Product("Organic Cotton T-Shirt",    34.99m, "https://images.unsplash.com/photo-1521572163474-6864f9cf17ab?w=400")
                { CategoryLabel = "Clothing",    Rating = 4.3, ReviewCount = 567, TrendingScore = 75 },
            new Product("Smart Fitness Tracker",     79.99m, "https://images.unsplash.com/photo-1575311373937-040b8e1fd5b6?w=400")
                { OriginalPrice = 99.99m,  CategoryLabel = "Electronics", Badge = "Sale",       Rating = 4.4, ReviewCount = 892, TrendingScore = 88 },
            new Product("Designer Sunglasses",       199.99m, "https://images.unsplash.com/photo-1572635196237-14b3f281503f?w=400")
                { CategoryLabel = "Accessories", Badge = "Trending",   Rating = 4.7, ReviewCount = 234, TrendingScore = 91 },
            new Product("Premium Yoga Mat",          49.99m, "https://images.unsplash.com/photo-1601925260368-ae2f83cf8b7f?w=400")
                { OriginalPrice = 69.99m,  CategoryLabel = "Fitness",     Rating = 4.2, ReviewCount = 156, TrendingScore = 60 },
            new Product("Silk Blend Scarf",          44.99m, "https://images.unsplash.com/photo-1601924994987-69e26d50dc26?w=400")
                { CategoryLabel = "Accessories", Badge = "New",        Rating = 4.1, ReviewCount = 78,  TrendingScore = 55 },
            new Product("Heritage Leather Backpack", 179.99m, "https://images.unsplash.com/photo-1553062407-98eeb64c6a62?w=400")
                { OriginalPrice = 219.99m, CategoryLabel = "Bags",        Badge = "Popular",    Rating = 4.6, ReviewCount = 445, TrendingScore = 85 },
            new Product("Portable Bluetooth Speaker", 59.99m, "https://images.unsplash.com/photo-1608043152269-423dbba4e7e1?w=400")
                { OriginalPrice = 79.99m,  CategoryLabel = "Electronics", Badge = "Deal",       Rating = 4.3, ReviewCount = 678, TrendingScore = 78 }
        );

        // ── Collections ──
        db.Collections.AddRange(
            new Collection("Summer Essentials",  "https://images.unsplash.com/photo-1507525428034-b723cf961d3e?w=600", 1) { Description = "Beat the heat in style",             LinkUrl = "/collections/summer",    VisitCount = 12500 },
            new Collection("Work From Home",     "https://images.unsplash.com/photo-1593642632559-0c6d3fc62b89?w=600", 2) { Description = "Comfort meets productivity",         LinkUrl = "/collections/wfh",       VisitCount = 9800 },
            new Collection("Fitness & Wellness", "https://images.unsplash.com/photo-1571019613454-1cb2f99b2d8b?w=600", 3) { Description = "Gear up for your best self",         LinkUrl = "/collections/fitness",   VisitCount = 8700 },
            new Collection("Tech Gadgets",       "https://images.unsplash.com/photo-1519389950473-47ba0277781c?w=600", 4) { Description = "Innovation at your fingertips",      LinkUrl = "/collections/tech",      VisitCount = 11200 },
            new Collection("Minimalist Living",  "https://images.unsplash.com/photo-1494438639946-1ebd1d20bf85?w=600", 5) { Description = "Less is more — curated simplicity",  LinkUrl = "/collections/minimalist", VisitCount = 6300 },
            new Collection("Luxury Picks",       "https://images.unsplash.com/photo-1490367532201-b9bc1dc483f6?w=600", 6) { Description = "Premium products for refined taste", LinkUrl = "/collections/luxury",    VisitCount = 7500 }
        );

        // ── Footer Links ──
        db.FooterLinks.AddRange(
            // Company
            new FooterLink("Company", "About Us",       "/about",    1),
            new FooterLink("Company", "Careers",        "/careers",  2),
            new FooterLink("Company", "Press",          "/press",    3),
            new FooterLink("Company", "Blog",           "/blog",     4),
            // Help
            new FooterLink("Help",    "Contact Us",     "/contact",  1),
            new FooterLink("Help",    "FAQs",           "/faq",      2),
            new FooterLink("Help",    "Shipping Info",  "/shipping", 3),
            new FooterLink("Help",    "Returns",        "/returns",  4),
            // Legal
            new FooterLink("Legal",   "Privacy Policy", "/privacy",  1),
            new FooterLink("Legal",   "Terms of Use",   "/terms",    2),
            new FooterLink("Legal",   "Cookie Policy",  "/cookies",  3)
        );

        // ── Social Icons ──
        db.SocialIcons.AddRange(
            new SocialIcon("facebook",  "fab fa-facebook-f",  "https://facebook.com/eshop",  1),
            new SocialIcon("instagram", "fab fa-instagram",   "https://instagram.com/eshop", 2),
            new SocialIcon("twitter",   "fab fa-twitter",     "https://twitter.com/eshop",   3),
            new SocialIcon("youtube",   "fab fa-youtube",     "https://youtube.com/eshop",   4),
            new SocialIcon("pinterest", "fab fa-pinterest-p", "https://pinterest.com/eshop", 5)
        );

        db.SaveChanges();
    }
}
