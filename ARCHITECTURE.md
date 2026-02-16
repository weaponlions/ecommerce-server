# eShopServer — Architecture & Documentation

> **Framework:** ASP.NET Core 10 (`.NET 10`)  
> **ORM:** Entity Framework Core 9 (Pomelo MySQL provider)  
> **Database:** MySQL 8  
> **API Docs:** Swagger / OpenAPI  
> **Architecture:** Layered — Controller → Service → Repository → EF Core  

---

## Table of Contents

1. [Project Structure](#1-project-structure)
2. [Architecture Overview](#2-architecture-overview)
3. [Database Models](#3-database-models)
   - [Dashboard Module](#31-dashboard-module)
   - [Product Module](#32-product-module)
   - [Media Module](#33-media-module)
4. [Entity Relationships (ER Diagram)](#4-entity-relationships)
5. [Repositories](#5-repositories)
6. [Services (Business Logic)](#6-services)
7. [Controllers (API Layer)](#7-controllers)
8. [DTOs (Data Transfer Objects)](#8-dtos)
9. [Middleware](#9-middleware)
10. [Configuration & Startup](#10-configuration--startup)
11. [API Route Map](#11-api-route-map)

---

## 1. Project Structure

```
eShopServer/
├── Controllers/                    # API endpoints (5 controllers)
│   ├── AdminController.cs          # Dashboard content CRUD
│   ├── AdminProductsController.cs  # Categories, attributes, products CRUD
│   ├── DashboardController.cs      # Public read-only dashboard API
│   ├── MediaController.cs          # Media upload, browse, link, delete
│   └── ProductsController.cs       # Public product browsing & filtering
│
├── Data/
│   ├── AppDbContext.cs              # EF Core DbContext with all DbSets & configs
│   ├── SeedData.cs                 # Initial data seeding (demo content)
│   └── Migrations/                 # EF Core migration history
│
├── DTOs/                           # Request/Response data transfer objects
│   ├── DashboardDtos.cs            # Dashboard section, navbar, carousel, etc.
│   ├── MediaDtos.cs                # Media asset responses & requests
│   └── ProductModuleDtos.cs        # Category, attribute, product DTOs
│
├── Interfaces/
│   ├── Repositories/               # Repository contracts (14 interfaces)
│   │   ├── IRepository.cs          # Generic CRUD base interface
│   │   ├── ICarouselSlideRepository.cs
│   │   ├── ICategoryRepository.cs
│   │   ├── ICategoryAttributeRepository.cs
│   │   ├── ICollectionRepository.cs
│   │   ├── IDashboardSectionRepository.cs
│   │   ├── IFooterLinkRepository.cs
│   │   ├── IMediaAssetRepository.cs
│   │   ├── IMediaUsageRepository.cs
│   │   ├── INavbarLinkRepository.cs
│   │   ├── IProductRepository.cs
│   │   ├── IProductAttributeValueRepository.cs
│   │   ├── IRecentlyVisitedProductRepository.cs
│   │   └── ISocialIconRepository.cs
│   └── Services/                   # Service contracts (4 interfaces)
│       ├── IAdminService.cs
│       ├── IDashboardService.cs
│       ├── IMediaService.cs
│       └── IProductService.cs
│
├── Middleware/
│   └── GlobalExceptionHandler.cs   # Catches all exceptions & returns clean JSON
│
├── Models/                         # EF Core entity models (13 models)
│   ├── CarouselSlide.cs
│   ├── Category.cs
│   ├── CategoryAttribute.cs
│   ├── Collection.cs
│   ├── DashboardSection.cs
│   ├── FooterLink.cs
│   ├── MediaAsset.cs
│   ├── MediaUsage.cs
│   ├── NavbarLink.cs
│   ├── Product.cs
│   ├── ProductAttributeValue.cs
│   ├── RecentlyVisitedProduct.cs
│   └── SocialIcon.cs
│
├── Repositories/                   # Repository implementations (14 files)
│   ├── Repository.cs               # Generic base with CRUD
│   └── ... (one per entity)
│
├── Services/                       # Business logic layer (4 services)
│   ├── AdminService.cs             # Dashboard content management
│   ├── DashboardService.cs         # Public dashboard aggregation
│   ├── MediaService.cs             # File upload, CRUD, usage tracking
│   └── ProductService.cs           # Categories, attributes, products
│
├── Program.cs                      # App startup, DI, middleware pipeline
├── appsettings.json                # Connection string & logging config
└── eShopServer.csproj              # NuGet dependencies
```

---

## 2. Architecture Overview

The project follows a **clean layered architecture** with dependency injection:

```
┌──────────────────────────────────────────────────────────┐
│                    HTTP Request                           │
└──────────────────┬───────────────────────────────────────┘
                   │
┌──────────────────▼───────────────────────────────────────┐
│              Middleware Layer                             │
│  • GlobalExceptionHandler (catches all exceptions)       │
│  • CORS, HTTPS Redirect, Static Files                    │
└──────────────────┬───────────────────────────────────────┘
                   │
┌──────────────────▼───────────────────────────────────────┐
│              Controller Layer                            │
│  • Receives HTTP requests                                │
│  • Validates input, routes to services                   │
│  • Returns HTTP responses (200, 201, 404, 400)           │
│                                                          │
│  AdminController          → IAdminService                │
│  AdminProductsController  → IProductService              │
│  DashboardController      → IDashboardService            │
│  MediaController          → IMediaService                │
│  ProductsController       → IProductService              │
└──────────────────┬───────────────────────────────────────┘
                   │
┌──────────────────▼───────────────────────────────────────┐
│              Service Layer (Business Logic)               │
│  • Orchestrates repository calls                         │
│  • Validates business rules                              │
│  • Maps entities ↔ DTOs                                  │
│  • Manages media usage tracking                          │
│                                                          │
│  AdminService       → 7 repos + media repos              │
│  DashboardService   → 7 repos (read-only)                │
│  MediaService       → media repos + file system           │
│  ProductService     → 5 repos + media repos              │
└──────────────────┬───────────────────────────────────────┘
                   │
┌──────────────────▼───────────────────────────────────────┐
│              Repository Layer                            │
│  • Generic Repository<T> base class                      │
│  • Entity-specific repos with custom queries             │
│  • All DB access goes through repositories               │
└──────────────────┬───────────────────────────────────────┘
                   │
┌──────────────────▼───────────────────────────────────────┐
│              Data Layer                                   │
│  • AppDbContext (EF Core)                                │
│  • MySQL database via Pomelo provider                    │
│  • Code-first migrations                                 │
└──────────────────────────────────────────────────────────┘
```

### Key Design Principles

- **Dependency Injection** — All services and repositories are registered as scoped in `Program.cs`
- **Interface Segregation** — Every service and repository has an interface
- **Generic Repository** — `Repository<T>` provides standard CRUD; entity-specific repos add custom queries
- **Rich Domain Models** — Constructors enforce required fields with validation; private parameterless constructors for EF Core
- **Immutable DTOs** — All DTOs are C# `record` types for clean data transfer

---

## 3. Database Models

### 3.1 Dashboard Module

These models power the configurable storefront dashboard.

#### `DashboardSection`

| Column | Type | Description |
|--------|------|-------------|
| `Id` | `int` PK | Auto-increment |
| `SectionKey` | `string(50)` | Unique key: `"navbar"`, `"carousel"`, `"trending"`, `"recently_visited"`, `"collections"`, `"footer"` |
| `Title` | `string(100)` | Display title for the section |
| `DisplayOrder` | `int` | Controls rendering order (lower = first) |
| `IsVisible` | `bool` | Toggle section visibility |
| `LayoutHint` | `string(50)?` | Optional CSS/layout hint (e.g., `"grid-4"`, `"scroll-horizontal"`) |

**Use Case:** The admin can reorder, rename, show/hide, and style each section of the dashboard without code changes. The frontend reads the sections in order and renders the appropriate component for each `SectionKey`.

---

#### `NavbarLink`

| Column | Type | Description |
|--------|------|-------------|
| `Id` | `int` PK | Auto-increment |
| `Label` | `string(100)` | Display text |
| `Url` | `string(500)` | Navigation URL |
| `Icon` | `string(100)?` | Optional material icon name |
| `DisplayOrder` | `int` | Sort order |
| `IsVisible` | `bool` | Show/hide toggle |
| `ParentId` | `int?` | Self-referencing FK for dropdown menus |

**Use Case:** Supports multi-level navigation. Top-level links have `ParentId = null`; dropdown children reference their parent's `Id`. The admin can create nested menus dynamically.

---

#### `CarouselSlide`

| Column | Type | Description |
|--------|------|-------------|
| `Id` | `int` PK | Auto-increment |
| `Title` | `string(200)` | Slide heading |
| `Subtitle` | `string(500)?` | Optional subtext |
| `LinkUrl` | `string(2000)?` | CTA destination URL |
| `ButtonText` | `string(100)?` | CTA button label |
| `DisplayOrder` | `int` | Sort order |
| `MediaAssetId` | `int?` | FK → `MediaAsset` (slide image) |
| `IsVisible` | `bool` | Show/hide toggle |
| `StartDate` | `DateTime?` | Optional scheduling: show after this date |
| `EndDate` | `DateTime?` | Optional scheduling: hide after this date |

**Use Case:** Hero carousel on the homepage. Supports time-based scheduling (e.g., flash sale banners that auto-appear/disappear). Images are managed through the media asset system.

---

#### `Collection`

| Column | Type | Description |
|--------|------|-------------|
| `Id` | `int` PK | Auto-increment |
| `Name` | `string(200)` | Collection name |
| `Description` | `string(1000)?` | Optional description |
| `LinkUrl` | `string(2000)?` | URL to the collection page |
| `VisitCount` | `int` | Tracks popularity |
| `DisplayOrder` | `int` | Sort order |
| `MediaAssetId` | `int?` | FK → `MediaAsset` (collection image) |
| `IsVisible` | `bool` | Show/hide toggle |

**Use Case:** Curated product groupings like "Summer Essentials" or "Tech Gadgets". Displayed on the dashboard sorted by `VisitCount` to show most popular collections.

---

#### `FooterLink`

| Column | Type | Description |
|--------|------|-------------|
| `Id` | `int` PK | Auto-increment |
| `GroupName` | `string(100)` | Groups links into columns: `"Company"`, `"Help"`, `"Legal"` |
| `Label` | `string(100)` | Link text |
| `Url` | `string(500)` | Link destination |
| `DisplayOrder` | `int` | Sort order within group |
| `IsVisible` | `bool` | Show/hide toggle |

**Use Case:** Footer navigation organized into named groups. The frontend renders each group as a column.

---

#### `SocialIcon`

| Column | Type | Description |
|--------|------|-------------|
| `Id` | `int` PK | Auto-increment |
| `Platform` | `string(50)` | e.g., `"facebook"`, `"instagram"` |
| `IconRef` | `string(200)` | CSS class (e.g., `"fab fa-facebook"`) or media URL |
| `Url` | `string(500)` | Social profile URL |
| `DisplayOrder` | `int` | Sort order |
| `MediaAssetId` | `int?` | FK → `MediaAsset` (optional uploaded icon) |
| `IsVisible` | `bool` | Show/hide toggle |

**Use Case:** Social media links in the footer. Supports both CSS icon classes (Font Awesome) and uploaded image icons via the media system.

---

#### `RecentlyVisitedProduct`

| Column | Type | Description |
|--------|------|-------------|
| `Id` | `int` PK | Auto-increment |
| `UserId` | `string(100)` | User or session ID |
| `ProductId` | `int` | FK → `Product` |
| `VisitedAt` | `DateTime` | Timestamp of visit |

**Indexes:** Composite on `(UserId, ProductId)` for fast lookups.

**Use Case:** Tracks which products a user has viewed recently. Limited to 20 per user (older visits are pruned). Powers the "Recently Visited" dashboard section.

---

### 3.2 Product Module

A flexible, category-driven product system with dynamic attributes.

#### `Category`

| Column | Type | Description |
|--------|------|-------------|
| `Id` | `int` PK | Auto-increment |
| `Name` | `string(100)` | Display name: `"Shoes"`, `"Electronics"` |
| `Slug` | `string(100)` | URL-friendly identifier (unique): `"shoes"`, `"electronics"` |
| `Description` | `string(500)?` | Optional description |
| `MediaAssetId` | `int?` | FK → `MediaAsset` (category image) |
| `IsActive` | `bool` | Active toggle |
| `CreatedAt` | `DateTime` | Creation timestamp |

**Relationships:**
- Has many `CategoryAttribute` (cascade delete)
- Has many `Product` (set null on delete)

**Use Case:** Categories define what attributes their products can have. Adding a new product type (e.g., "Furniture") requires only creating a new category and its attributes — no schema changes needed.

---

#### `CategoryAttribute`

| Column | Type | Description |
|--------|------|-------------|
| `Id` | `int` PK | Auto-increment |
| `CategoryId` | `int` | FK → `Category` |
| `Name` | `string(50)` | Machine key: `"size"`, `"color"` (lowercase, unique per category) |
| `DisplayName` | `string(100)` | Human label: `"Size"`, `"Color"` |
| `DataType` | `enum` | `String`, `Number`, `Select`, `MultiSelect`, `Boolean` |
| `IsRequired` | `bool` | Whether products must provide a value |
| `IsFilterable` | `bool` | Whether this attribute appears as a filter on the frontend |
| `Options` | `string(4000)?` | JSON array of allowed values for Select/MultiSelect: `["S","M","L","XL"]` |
| `DisplayOrder` | `int` | Sort order |

**Unique Index:** `(CategoryId, Name)` — no duplicate attribute names per category.

**Use Case:** Defines the schema for a category's products. Example:
- Category "Shoes" → Attributes: `size (Select: [38,39,40,...])`, `color (Select: [Red,Blue,...])`, `waterproof (Boolean)`
- Category "Clothes" → Attributes: `size (Select: [S,M,L,XL])`, `fabric (String)`, `color (MultiSelect: [Red,Blue,...])`

---

#### `Product`

| Column | Type | Description |
|--------|------|-------------|
| `Id` | `int` PK | Auto-increment |
| `Name` | `string(200)` | Product name |
| `Description` | `string(2000)?` | Optional description |
| `Price` | `decimal(18,2)` | Current selling price |
| `OriginalPrice` | `decimal(18,2)?` | Original price (to show discounts) |
| `CategoryLabel` | `string(100)?` | Legacy display label (e.g., "Footwear") |
| `Badge` | `string(50)?` | Promotional badge: `"Hot"`, `"Bestseller"`, `"Sale"` |
| `Rating` | `double` | Average rating (0-5) |
| `ReviewCount` | `int` | Number of reviews |
| `TrendingScore` | `int` | Higher = more trending (used for sorting) |
| `CategoryId` | `int?` | FK → `Category` |
| `Stock` | `int` | Quantity available (-1 = unlimited) |
| `MediaAssetId` | `int?` | FK → `MediaAsset` (product image) |
| `IsVisible` | `bool` | Show/hide toggle |
| `CreatedAt` | `DateTime` | Creation timestamp |

**Relationships:**
- Belongs to `Category?` (optional)
- Belongs to `MediaAsset?` (optional)
- Has many `ProductAttributeValue` (cascade delete)

**Use Case:** Core product entity. Products can optionally belong to a category, which determines their available attributes. The `TrendingScore` powers the trending section on the dashboard.

---

#### `ProductAttributeValue`

| Column | Type | Description |
|--------|------|-------------|
| `Id` | `int` PK | Auto-increment |
| `ProductId` | `int` | FK → `Product` |
| `CategoryAttributeId` | `int` | FK → `CategoryAttribute` |
| `Value` | `string(2000)` | Stored as string. Format depends on `DataType`: plain text, numeric string, JSON array, or `"true"`/`"false"` |

**Unique Index:** `(ProductId, CategoryAttributeId)` — one value per attribute per product.

**Use Case:** Stores the actual attribute values for each product. Example: Product "Nike Air Max" → `{ size: "42", color: "White", waterproof: "true" }`. Values are validated against the attribute's `DataType` and `Options` at the service layer.

---

### 3.3 Media Module

Centralized media asset management with usage tracking.

#### `MediaAsset`

| Column | Type | Description |
|--------|------|-------------|
| `Id` | `int` PK | Auto-increment |
| `FileName` | `string(500)` | GUID-based filename on disk (unique) |
| `OriginalFileName` | `string(500)` | User's original filename |
| `ContentType` | `string(100)` | MIME type: `"image/jpeg"`, `"image/png"`, etc. |
| `FileSizeBytes` | `long` | File size in bytes |
| `Width` | `int?` | Image width in pixels (null for SVG/non-raster) |
| `Height` | `int?` | Image height in pixels |
| `Url` | `string(2000)` | Public URL: `/uploads/{category}/{filename}` |
| `AltText` | `string(500)?` | Accessibility alt text |
| `Title` | `string(200)?` | Display title |
| `Category` | `string(50)` | Organizational: `"carousel"`, `"product"`, `"collection"`, `"category"`, `"social-icon"`, `"general"` |
| `CreatedAt` | `DateTime` | Upload timestamp |
| `UpdatedAt` | `DateTime?` | Last metadata update |

**Unique Index:** `FileName`

**Allowed types:** JPEG, PNG, GIF, WebP, SVG, BMP, ICO  
**Max file size:** 10 MB  
**Storage:** `wwwroot/uploads/{category}/{guid-filename}`

**Use Case:** All images in the system are uploaded here first, then linked to entities via `MediaAssetId` foreign keys. This decouples image storage from business entities and enables:
- **Reuse** — same image on multiple entities
- **Usage tracking** — know where every image is used
- **Browsing** — admin media library with search and category filter
- **Metadata** — alt text, titles for SEO/accessibility

---

#### `MediaUsage`

| Column | Type | Description |
|--------|------|-------------|
| `Id` | `int` PK | Auto-increment |
| `MediaAssetId` | `int` | FK → `MediaAsset` (cascade delete) |
| `EntityType` | `string(100)` | e.g., `"CarouselSlide"`, `"Product"`, `"Collection"` |
| `EntityId` | `int` | The entity's PK |
| `FieldName` | `string(100)` | Which field uses the asset: `"MediaAssetId"` |
| `CreatedAt` | `DateTime` | Timestamp |

**Indexes:**
- `(EntityType, EntityId)` — find all media for an entity
- `(MediaAssetId, EntityType, EntityId, FieldName)` — unique, prevents duplicate links

**Use Case:** Tracks where each media asset is used. When an admin views a media asset, they can see all entities using it. When an entity is deleted, its usage records are cleaned up.

---

## 4. Entity Relationships

```
┌─────────────────┐         ┌──────────────────────┐
│  DashboardSection│         │     MediaAsset        │
│─────────────────│         │──────────────────────│
│ SectionKey (UK) │         │ FileName (UK)         │
│ Title           │         │ Url                   │
│ DisplayOrder    │         │ Category              │
│ IsVisible       │    ┌───►│ AltText, Title        │
│ LayoutHint      │    │    │                       │
└─────────────────┘    │    │    ┌──────────┐       │
                       │    │    │ Usages[] ├──────┐ │
┌─────────────────┐    │    └────┴──────────┴─────┘ │
│   NavbarLink     │    │                      │     │
│─────────────────│    │    ┌──────────────┐   │     │
│ Label, Url      │    │    │  MediaUsage   │◄──┘     │
│ ParentId (self) │    │    │──────────────│         │
└─────────────────┘    │    │ EntityType   │         │
                       │    │ EntityId     │         │
┌─────────────────┐    │    │ FieldName    │         │
│  CarouselSlide   │    │    └──────────────┘         │
│─────────────────│    │                              │
│ Title, Subtitle │    │    ┌──────────────┐         │
│ MediaAssetId ───┼────┤    │   Category    │         │
│ StartDate/End   │    │    │──────────────│         │
└─────────────────┘    │    │ Name, Slug   │         │
                       │    │ MediaAssetId ┼─────────┘
┌─────────────────┐    │    │              │          
│   Collection     │    │    │  ┌────────┐  │          
│─────────────────│    │    │  │Attrs[] ├──┼─► CategoryAttribute
│ Name, Desc      │    │    │  └────────┘  │    │ Name, DataType
│ MediaAssetId ───┼────┤    │  ┌────────┐  │    │ Options (JSON)
│ VisitCount      │    │    │  │Products├──┼─┐  │ IsFilterable
└─────────────────┘    │    │  └────────┘  │ │  └───────────────
                       │    └──────────────┘ │          │
┌─────────────────┐    │                     │          │
│   SocialIcon     │    │    ┌──────────────┐ │  ┌──────▼────────┐
│─────────────────│    │    │   Product     │◄┘  │ProductAttrValue│
│ Platform        │    │    │──────────────│    │───────────────│
│ IconRef         │    │    │ Name, Price  │    │ ProductId     │
│ MediaAssetId ───┼────┘    │ CategoryId   │    │ CatAttrId     │
│ Url             │         │ MediaAssetId ┼──┐ │ Value         │
└─────────────────┘         │ TrendingScore│  │ └───────────────┘
                            │ Stock        │  │
┌─────────────────┐         │ AttrValues[] │  └──► MediaAsset
│   FooterLink     │         └──────┬───────┘
│─────────────────│                │
│ GroupName       │    ┌───────────▼──────────┐
│ Label, Url      │    │RecentlyVisitedProduct│
└─────────────────┘    │─────────────────────│
                       │ UserId              │
                       │ ProductId → Product │
                       │ VisitedAt           │
                       └─────────────────────┘
```

### FK Behaviors

| FK Relationship | On Delete |
|---|---|
| `CarouselSlide.MediaAssetId → MediaAsset` | `SetNull` |
| `Collection.MediaAssetId → MediaAsset` | `SetNull` |
| `Product.MediaAssetId → MediaAsset` | `SetNull` |
| `Category.MediaAssetId → MediaAsset` | `SetNull` |
| `SocialIcon.MediaAssetId → MediaAsset` | `SetNull` |
| `MediaUsage.MediaAssetId → MediaAsset` | `Cascade` |
| `Product.CategoryId → Category` | `SetNull` |
| `CategoryAttribute.CategoryId → Category` | `Cascade` |
| `ProductAttributeValue.ProductId → Product` | `Cascade` |
| `ProductAttributeValue.CategoryAttributeId → CategoryAttribute` | `Cascade` |

---

## 5. Repositories

### Generic Base: `Repository<T>`

Provides standard CRUD operations that all entity repositories inherit:

| Method | Description |
|--------|-------------|
| `GetAllAsync()` | Returns all entities |
| `FindAsync(predicate)` | Filter by expression |
| `GetByIdAsync(id)` | Find by primary key |
| `AddAsync(entity)` | Insert + SaveChanges |
| `UpdateAsync(entity)` | Update + SaveChanges |
| `DeleteAsync(id)` | Remove + SaveChanges |
| `SaveChangesAsync()` | Manual save |

### Entity-Specific Repositories

| Repository | Custom Methods |
|---|---|
| `DashboardSectionRepository` | `GetVisibleOrderedAsync()` |
| `NavbarLinkRepository` | `GetVisibleOrderedAsync()` |
| `CarouselSlideRepository` | `GetActiveSlidesAsync(currentTime)` — respects scheduling dates |
| `ProductRepository` | `GetTrendingAsync(count)` — top products by `TrendingScore` |
| `RecentlyVisitedProductRepository` | `GetByUserAsync(userId, limit)`, `FindByUserAndProductAsync()`, `RemoveOldestVisitsAsync()` |
| `CollectionRepository` | `GetMostVisitedAsync(count)` — sorted by `VisitCount` desc |
| `FooterLinkRepository` | `GetVisibleOrderedAsync()` |
| `SocialIconRepository` | `GetVisibleOrderedAsync()` |
| `CategoryRepository` | `GetActiveCategoriesAsync()`, `GetBySlugAsync(slug)`, `GetWithAttributesAsync(id)` |
| `CategoryAttributeRepository` | `GetByCategoryIdAsync(categoryId)` |
| `ProductAttributeValueRepository` | `GetByProductIdAsync(productId)`, `DeleteByProductIdAsync(productId)` |
| `MediaAssetRepository` | `GetByCategoryAsync()`, `GetByFileNameAsync()`, `GetWithUsagesAsync()`, `SearchAsync()`, `GetTotalCountAsync()` |
| `MediaUsageRepository` | `GetByMediaAssetIdAsync()`, `GetByEntityAsync()`, `FindExactAsync()`, `DeleteByMediaAssetIdAsync()`, `DeleteByEntityAsync()` |

---

## 6. Services

### `AdminService`

Manages all dashboard content CRUD. For entities with images, it:
1. **Validates** that the referenced `MediaAssetId` exists
2. **Assigns** `MediaAssetId` on the entity
3. **Tracks** usage via `MediaUsage` records
4. **Cleans up** usage records when entities are deleted

| Manages | Operations |
|---|---|
| Dashboard Sections | Get all, Update |
| Navbar Links | CRUD |
| Carousel Slides | CRUD (with media validation) |
| Products (dashboard) | CRUD (with media validation) |
| Collections | CRUD (with media validation) |
| Footer Links | CRUD |
| Social Icons | CRUD (with optional media — resolves icon from media URL) |

---

### `DashboardService`

**Read-only** service that aggregates all dashboard data for the frontend. Each section builder method queries its repository and maps entities to DTOs.

| Method | Description |
|---|---|
| `GetFullDashboardAsync(userId?)` | Returns all visible sections with their data in order |
| `GetNavbarAsync()` | Navbar links with nested children |
| `GetCarouselAsync()` | Active slides (respects scheduling) |
| `GetTrendingAsync()` | Top 12 products by `TrendingScore` |
| `GetRecentlyVisitedAsync(userId)` | Last 20 visited products for a user |
| `TrackVisitAsync(request)` | Records a product visit (upserts, prunes old) |
| `GetCollectionsAsync()` | Top 10 collections by `VisitCount` |
| `GetFooterAsync()` | Grouped footer links + social icons |

---

### `ProductService`

Handles the complete product catalog with dynamic attributes.

| Area | Operations |
|---|---|
| **Categories** | List active, Get by slug/id, CRUD (with media) |
| **Category Attributes** | List by category, CRUD (with validation) |
| **Products** | Get by id, Paginated listing with filters, CRUD (with media + attributes) |

**Key Features:**
- **Dynamic filtering** — filter products by any category attribute using `attr_color=Red&attr_size=M` query params
- **Attribute validation** — values are validated against their `DataType` and `Options`
- **Multi-sort** — sort by price, name, rating, newest, or trending (default)
- **Pagination** — configurable page/pageSize (max 100)

---

### `MediaService`

Handles file upload to disk and CRUD on media records.

| Method | Description |
|--------|-------------|
| `UploadAsync(file, altText, title, category)` | Validates file (type, size), saves to `wwwroot/uploads/{category}/`, creates DB record |
| `GetByIdAsync(id)` | Returns asset with usage details |
| `GetAllAsync(search, category, page, pageSize)` | Paginated browse with search and category filter |
| `UpdateMetadataAsync(id, request)` | Update alt text, title, category (moves file if category changes) |
| `DeleteAsync(id)` | Deletes file from disk + DB record + all usage records |
| `LinkAsync(request)` | Creates a usage tracking record |
| `UnlinkAsync(usageId)` | Removes a usage record |
| `GetUsagesForAssetAsync(assetId)` | Lists where an asset is used |
| `GetMediaForEntityAsync(type, id)` | Lists all media linked to an entity |

**Auto-detected dimensions:** Reads image headers to extract width/height for JPEG, PNG, GIF, BMP, WebP.

---

## 7. Controllers

### Route Prefix Summary

| Controller | Route | Auth | Purpose |
|---|---|---|---|
| `DashboardController` | `api/dashboard` | Public | Read dashboard data |
| `ProductsController` | `api/products` | Public | Browse/search products |
| `AdminController` | `api/admin` | Admin* | Manage dashboard content |
| `AdminProductsController` | `api/admin/products` | Admin* | Manage categories & products |
| `MediaController` | `api/admin/media` | Admin* | Upload & manage images |

*\*Note: Auth is not yet implemented — in production, admin routes should be protected.*

---

## 8. DTOs

All DTOs are immutable C# `record` types.

### Dashboard DTOs (`DashboardDtos.cs`)

| DTO | Type | Purpose |
|---|---|---|
| `DashboardResponse` | Response | Full dashboard with all sections |
| `DashboardSectionDto` | Response | Single section with polymorphic `Data` |
| `NavbarDto` / `NavbarLinkDto` | Response | Nested navbar tree |
| `CarouselSlideDto` | Response | Slide with `MediaAssetId` |
| `ProductDto` | Response | Lightweight product for lists |
| `CollectionDto` | Response | Collection with `MediaAssetId` |
| `FooterDto` / `FooterGroupDto` / `FooterLinkDto` | Response | Grouped footer links |
| `SocialIconDto` | Response | Social link with icon |
| `TrackVisitRequest` | Request | Track a product visit |
| `Upsert*Request` | Request | Create/update requests for each entity |

### Product DTOs (`ProductModuleDtos.cs`)

| DTO | Type | Purpose |
|---|---|---|
| `CategoryResponse` / `CategoryListResponse` | Response | Category with/without attributes |
| `CategoryAttributeResponse` | Response | Attribute definition with parsed options |
| `ProductDetailResponse` | Response | Full product with attributes dictionary |
| `ProductListItemResponse` | Response | Lightweight product for listings |
| `ProductFilterRequest` | Request | Filtering, search, sort, pagination |
| `PagedResponse<T>` | Response | Generic paginated result wrapper |

### Media DTOs (`MediaDtos.cs`)

| DTO | Type | Purpose |
|---|---|---|
| `MediaAssetResponse` | Response | Asset summary with usage count |
| `MediaAssetDetailResponse` | Response | Asset with full usage list |
| `MediaUsageResponse` | Response | Single usage record |
| `UpdateMediaMetadataRequest` | Request | Update alt text, title, category |
| `LinkMediaRequest` | Request | Link asset to an entity |

---

## 9. Middleware

### `GlobalExceptionHandler`

A first-in-pipeline middleware that catches all unhandled exceptions and returns clean JSON error responses. Key features:

- **EF Core / MySQL errors** — translates raw MySQL error codes into user-friendly messages:
  - Duplicate key → `"A record with this value already exists"`
  - FK constraint violation → `"Cannot delete: this record is referenced by other data"`
  - Data too long → `"One or more field values exceed the maximum allowed length"`
- **Validation errors** → `400 Bad Request`
- **ArgumentExceptions** → `400 Bad Request`
- **KeyNotFoundException** → `404 Not Found`
- **All other exceptions** → `500 Internal Server Error` with safe message in production

---

## 10. Configuration & Startup

### NuGet Dependencies

| Package | Version | Purpose |
|---|---|---|
| `Microsoft.AspNetCore.OpenApi` | 10.0.2 | OpenAPI/Swagger spec generation |
| `Microsoft.EntityFrameworkCore` | 9.0.0 | ORM |
| `Microsoft.EntityFrameworkCore.Design` | 9.0.0 | Migration tooling |
| `Pomelo.EntityFrameworkCore.MySql` | 9.0.0 | MySQL database provider |
| `Swashbuckle.AspNetCore` | 10.1.2 | Swagger UI |

### DI Registration Order (`Program.cs`)

1. **DbContext** — MySQL via Pomelo with auto-detected server version
2. **Repositories (Scoped)** — 12 entity repos + 2 media repos
3. **Services (Scoped)** — Dashboard, Admin, Product, Media
4. **Controllers** — auto-discovered via `AddControllers()`
5. **OpenAPI + Swagger**
6. **CORS** — Allow any origin/header/method (dev config)

### Middleware Pipeline Order

1. `GlobalExceptionHandler` (must be first)
2. `MapOpenApi()` + `SwaggerUI`
3. `UseHttpsRedirection()`
4. `UseStaticFiles()` — serves uploaded media from `wwwroot/uploads/`
5. `UseCors()`
6. `MapControllers()`

---

## 11. API Route Map

### Public Endpoints

```
GET    /api/dashboard                          Full dashboard (all sections)
GET    /api/dashboard?userId={id}              Full dashboard with recently visited
GET    /api/dashboard/navbar                   Navbar links
GET    /api/dashboard/carousel                 Active carousel slides
GET    /api/dashboard/trending                 Trending products
GET    /api/dashboard/recently-visited/{userId} Recently visited products
POST   /api/dashboard/recently-visited         Track a product visit
GET    /api/dashboard/collections              Most visited collections
GET    /api/dashboard/footer                   Footer links + social icons

GET    /api/products                           Browse products (with filters)
GET    /api/products/{id}                      Single product detail
GET    /api/products/categories                All active categories
GET    /api/products/categories/{slug}         Single category with attributes
```

### Admin — Dashboard Content

```
GET    /api/admin/sections                     All dashboard sections
PUT    /api/admin/sections/{id}                Update a section

GET    /api/admin/navbar                       All navbar links
POST   /api/admin/navbar                       Create navbar link
PUT    /api/admin/navbar/{id}                  Update navbar link
DELETE /api/admin/navbar/{id}                  Delete navbar link

GET    /api/admin/carousel                     All carousel slides
POST   /api/admin/carousel                     Create slide
PUT    /api/admin/carousel/{id}                Update slide
DELETE /api/admin/carousel/{id}                Delete slide

GET    /api/admin/collections                  All collections
POST   /api/admin/collections                  Create collection
PUT    /api/admin/collections/{id}             Update collection
DELETE /api/admin/collections/{id}             Delete collection

GET    /api/admin/footer                       All footer links
POST   /api/admin/footer                       Create footer link
PUT    /api/admin/footer/{id}                  Update footer link
DELETE /api/admin/footer/{id}                  Delete footer link

GET    /api/admin/social                       All social icons
POST   /api/admin/social                       Create social icon
PUT    /api/admin/social/{id}                  Update social icon
DELETE /api/admin/social/{id}                  Delete social icon
```

### Admin — Products

```
GET    /api/admin/products/categories          All categories
GET    /api/admin/products/categories/{id}     Single category
POST   /api/admin/products/categories          Create category
PUT    /api/admin/products/categories/{id}     Update category
DELETE /api/admin/products/categories/{id}     Delete category

GET    /api/admin/products/categories/{id}/attributes   Category attributes
POST   /api/admin/products/categories/{id}/attributes   Create attribute
PUT    /api/admin/products/attributes/{id}              Update attribute
DELETE /api/admin/products/attributes/{id}              Delete attribute

GET    /api/admin/products                     All products (paginated)
GET    /api/admin/products/{id}                Single product
POST   /api/admin/products                     Create product
PUT    /api/admin/products/{id}                Update product
DELETE /api/admin/products/{id}                Delete product
```

### Admin — Media

```
POST   /api/admin/media/upload                 Upload image (multipart/form-data)
GET    /api/admin/media                        Browse all assets (search, filter, paginate)
GET    /api/admin/media/{id}                   Single asset with usages
PUT    /api/admin/media/{id}                   Update metadata (alt text, title, category)
DELETE /api/admin/media/{id}                   Delete asset (file + DB + usages)
POST   /api/admin/media/link                   Link asset to entity
DELETE /api/admin/media/link/{usageId}         Unlink asset
GET    /api/admin/media/{id}/usages            Where is this asset used?
GET    /api/admin/media/entity/{type}/{id}     What media does this entity use?
```

---

## Typical Workflow: Adding a Product with an Image

1. **Upload** the image:
   ```
   POST /api/admin/media/upload
   → Returns: { id: 42, url: "/uploads/product/abc123.jpg", ... }
   ```

2. **Create** the product referencing the media:
   ```
   POST /api/admin/products
   Body: { name: "Sneakers", price: 89.99, mediaAssetId: 42, categoryId: 1, ... }
   ```
   The service automatically validates the media asset exists and creates a `MediaUsage` tracking record.

3. **Frontend** reads the product, gets `mediaAssetId: 42`, and can:
   - Fetch the full media asset details via `GET /api/admin/media/42` to get the URL
   - Or resolve the URL from the dashboard response directly

4. **Deleting** the product automatically cleans up the `MediaUsage` record (but keeps the image in the media library for potential reuse).
