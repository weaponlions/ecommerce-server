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
            new CarouselSlide("Summer Collection 2026", 1)
            {
                Subtitle = "Up to 50% off on all summer essentials",
                LinkUrl = "/shop/summer",
                ButtonText = "Shop Now"
            },
            new CarouselSlide("New Arrivals", 2)
            {
                Subtitle = "Check out the latest trends",
                LinkUrl = "/shop/new",
                ButtonText = "Explore"
            },
            new CarouselSlide("Premium Electronics", 3)
            {
                Subtitle = "Tech that redefines your life",
                LinkUrl = "/shop/electronics",
                ButtonText = "Discover"
            }
        );

        // ── Products ──
        db.Products.AddRange(
            new Product("Classic White Sneakers",    89.99m)
                { OriginalPrice = 129.99m, CategoryLabel = "Footwear",    Badge = "Hot",        Rating = 4.5, ReviewCount = 342,  TrendingScore = 95 },
            new Product("Wireless Noise-Cancelling", 249.99m)
                { OriginalPrice = 349.99m, CategoryLabel = "Electronics", Badge = "Bestseller", Rating = 4.8, ReviewCount = 1205, TrendingScore = 98 },
            new Product("Minimalist Leather Watch",  159.99m)
                { CategoryLabel = "Accessories", Badge = "New",        Rating = 4.6, ReviewCount = 89,  TrendingScore = 82 },
            new Product("Organic Cotton T-Shirt",    34.99m)
                { CategoryLabel = "Clothing",    Rating = 4.3, ReviewCount = 567, TrendingScore = 75 },
            new Product("Smart Fitness Tracker",     79.99m)
                { OriginalPrice = 99.99m,  CategoryLabel = "Electronics", Badge = "Sale",       Rating = 4.4, ReviewCount = 892, TrendingScore = 88 },
            new Product("Designer Sunglasses",       199.99m)
                { CategoryLabel = "Accessories", Badge = "Trending",   Rating = 4.7, ReviewCount = 234, TrendingScore = 91 },
            new Product("Premium Yoga Mat",          49.99m)
                { OriginalPrice = 69.99m,  CategoryLabel = "Fitness",     Rating = 4.2, ReviewCount = 156, TrendingScore = 60 },
            new Product("Silk Blend Scarf",          44.99m)
                { CategoryLabel = "Accessories", Badge = "New",        Rating = 4.1, ReviewCount = 78,  TrendingScore = 55 },
            new Product("Heritage Leather Backpack", 179.99m)
                { OriginalPrice = 219.99m, CategoryLabel = "Bags",        Badge = "Popular",    Rating = 4.6, ReviewCount = 445, TrendingScore = 85 },
            new Product("Portable Bluetooth Speaker", 59.99m)
                { OriginalPrice = 79.99m,  CategoryLabel = "Electronics", Badge = "Deal",       Rating = 4.3, ReviewCount = 678, TrendingScore = 78 }
        );

        // ── Collections ──
        db.Collections.AddRange(
            new Collection("Summer Essentials",  1) { Description = "Beat the heat in style",             LinkUrl = "/collections/summer",    VisitCount = 12500 },
            new Collection("Work From Home",     2) { Description = "Comfort meets productivity",         LinkUrl = "/collections/wfh",       VisitCount = 9800 },
            new Collection("Fitness & Wellness", 3) { Description = "Gear up for your best self",         LinkUrl = "/collections/fitness",   VisitCount = 8700 },
            new Collection("Tech Gadgets",       4) { Description = "Innovation at your fingertips",      LinkUrl = "/collections/tech",      VisitCount = 11200 },
            new Collection("Minimalist Living",  5) { Description = "Less is more — curated simplicity",  LinkUrl = "/collections/minimalist", VisitCount = 6300 },
            new Collection("Luxury Picks",       6) { Description = "Premium products for refined taste", LinkUrl = "/collections/luxury",    VisitCount = 7500 }
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
