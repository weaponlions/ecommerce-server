# E-Commerce Server Implementation Plan & Progress Map

This document outlines the architecture, current progress, and implementation details for the core e-commerce features: **Dashboard**, **Product Page**, **Search**, **Collection**, **Variants**, and **Filtration**.

It maps existing code to requirements and identifies gaps that need to be addressed to fully achieve the desired outcomes.

---

## 1. Dashboard
**Outcome**: A dynamic, server-driven homepage featuring promotional banners, trending items, and curated sections.

### ✅ Current Status: Implemented
The dashboard is fully operational with a server-driven architecture. The client fetches the structure, and the server dictates the content and order.

### 🗺️ Code Mapping
| Component | Code Location | Details |
|-----------|---------------|---------|
| **Endpoint** | `Controllers/DashboardController.cs` | `GET /api/dashboard` (Aggregates all sections) |
| **Service** | `Services/DashboardService.cs` | Logic to fetch Sections, Navbar, Carousel, Trending. |
| **Models** | `Models/DashboardSection.cs` | Defines sections like "Trending", "New Arrivals". |
| **Models** | `Models/CarouselSlide.cs` | Hero banner images/links. |
| **Models** | `Models/NavbarLink.cs` | Navigation menu structure. |

### 🔍 Key Features Achieved
- **Dynamic Sections**: Admin can rearrange sections via `DisplayOrder`.
- **Hero Carousel**: Fully manageable via Admin API.
- **Trending Products**: Algorithm matches products with high `TrendingScore`.

---

## 2. Product Page
**Outcome**: A detailed product view showing images, price, description, and dynamic attributes (like Specs, Material).

### ✅ Current Status: Implemented
The core product details are ready, supporting dynamic attributes driven by the product's category.

### 🗺️ Code Mapping
| Component | Code Location | Details |
|-----------|---------------|---------|
| **Endpoint** | `Controllers/ProductsController.cs` | `GET /api/products/{id}` |
| **Service** | `Services/ProductService.cs` | `GetProductByIdAsync` (Fetches product + matches attributes). |
| **Model** | `Models/Product.cs` | Core product data (Name, Price, Stock). |
| **Attributes** | `Models/ProductAttributeValue.cs` | Links Product -> Attribute (e.g. "Material": "Cotton"). |

### 🔍 Key Features Achieved
- **Dynamic Content**: Attributes change based on Category (e.g., "Screen Size" for Laptops, "Fabric" for Shirts).
- **SEO Ready**: Slugs and Descriptions are included.
- **Media**: Integrated with `MediaAsset` system for product images.

---

## 3. Product Features (Variants & Filtration)
**Outcome**: 
1. **Filtration**: Users can filter by Price, Category, and *Contextual Attributes* (e.g., Filter by "Ram > 16GB" only when inside "Laptops" category).
2. **Product Variants**: Handling different versions of a product (Size/Color).

### ✅ Filtration Status: Implemented & Working
The system supports advanced, dynamic filtering where attributes change based on the selected category (e.g., Laptops have "RAM", Shirts have "Size"). This is fully implemented across the stack.

#### 🏗️ Architecture: Entity-Attribute-Value (EAV)
instead of hardcoding columns like `Color` or `Size` on the Product table, we use a flexible EAV pattern:
1. **CategoryAttribute**: Defines the "Keys" allowed for a category (e.g., "Screen Size" for Laptops).
2. **ProductAttributeValue**: Stores the "Values" for a specific product (e.g., "15.6 inch").

#### 🔌 API Contract (Client ↔ Server)
The client can filter by any attribute defined in the category using `attr_` prefixed query parameters.

**Request Example:**
`GET /api/products?categoryId=5&minPrice=1000&attr_processor=Intel&attr_ram=16GB`

**How it works:**
1. **Client**: Dynamically generates inputs based on the Category's attributes.
2. **Controller** (`ProductsController.cs`): 
   - Iterates through all query parameters.
   - Extracts keys starting with `attr_` (e.g., `attr_processor` → `processor`).
   - Builds a `Dictionary<string, string>` of filters.
3. **Service** (`ProductService.cs`):
   - Fetches products matching standard filters (Price, Category).
   - **In-Memory Filtering**: Iterates through candidate products.
   - Fetches attribute values for each product (`GetByProductIdAsync`).
   - Checks if the product has attributes matching the requested values.
   - Returns the filtered list.

#### ✅ Verification
- **Scenario 1 (Exact Match)**: `attr_color=Red` matches products having "Red" in the `Color` attribute.
- **Scenario 2 (Multi-Select)**: If a product has `Color=["Red", "Blue"]` (JSON), searching `attr_color=Red` correctly finds it via `AttributeValueMatches`.
- **Scenario 3 (Booleans)**: `attr_wireless=true` correctly parses boolean values.

### ⚠️ Performance Note (Optimization Gap)
While fully functional, the current implementation uses an **N+1 Query pattern** in `ProductService.cs`:
- It fetches attribute values for *every* candidate product individually inside the loop.
- **Impact**: Filtering 100 products results in 101 database queries.
- **Recommendation**: Optimize to fetch all relevant attributes in a single `Join` query or pre-fetch batch.

### ⚠️ Product Variants Status: Simplistic (Attribute-Based)
**Current Architecture**: "Flat" Products with Attributes.
- The system uses **Attributes** (e.g., Color=Red, Size=M) attached to a Product.
- **Limitation**: It does **not** yet utilize a Parent-Child relationship (e.g., One generic "T-Shirt" container with multiple SKU children).
- **Current Workaround**: Each variant (Red T-Shirt, Blue T-Shirt) exists as a separate Product, or one Product represents all variants (mixed stock).

**To Achieve Full Variants:**
1. **Option A (Quick)**: Continue using current structure. Create separate products for "Shirt Red" and "Shirt Blue" if you need distinct stock. Group them on frontend by similar names.
2. **Option B (Robust)**: Add a `GroupId` or `ParentId` to `Product` model to link variants together.

---

## 4. Search Page
**Outcome**: A dedicated search results page allowing users to find products by text.

### ✅ Current Status: Implemented (Basic)
Search functionality exists within the main product listing API.

### 🗺️ Code Mapping
| Component | Code Location | Details |
|-----------|---------------|---------|
| **Endpoint** | `Controllers/ProductsController.cs` | `GET /api/products?search=...` |
| **Logic** | `Services/ProductService.cs` | Uses `string.Contains` on Name, Description, and CategoryLabel. |

### 🚀 Recommendation
The current simplistic text matching (`Contains`) is sufficient for small catalogs. For scale, consider adding full-text search indices or simple token matching to improve relevance.

---

## 5. Collection Page (Gaps Identified)
**Outcome**: Users click a "Summer Sale" banner and see a list of products in that collection.

### ⚠️ Current Status: Partial
- **Collections Logic**: The `Collection` entity exists, but it is currently just a "Banner/Link" container. There is **no database link** between a `Product` and a `Collection`.
- **Problem**: You cannot currently query "All products in Collection X".

### 🛠️ Implementation Plan (How to fix)
To achieve a "Collection Page" where products are dynamically listed:

#### Step 1: Update Database Schema
Modify the `Product` model to include a relationship to `Collection`.

**Option A: Simple (One Collection per Product)**
Add `public int? CollectionId { get; set; }` to `Product.cs`.

**Option B: Advanced (Many-to-Many)**
Create a join table `ProductCollections` (ProductId, CollectionId) so a product can be in "Summer Sale" AND "Menswear".

#### Step 2: Update Filtering Service
Update `ProductService.GetProductsAsync` to accept `CollectionId` in the filter request.

```csharp
// Inside GetProductsAsync
if (filter.CollectionId.HasValue) 
{
    allProducts = allProducts.Where(p => p.CollectionId == filter.CollectionId.Value);
}
```

#### Step 3: Frontend Usage
When a user clicks a Collection on the Dashboard:
1. Frontend reads `Collection.LinkUrl` OR `Collection.Id`.
2. Frontend calls `GET /api/products?collectionId={id}`.

---

## Summary of Next Steps

| Required Outcome | Action Required | Complexity |
|------------------|-----------------|------------|
| **Dynamic Collections** | Add `CollectionId` to Product model & Update Migration. | Medium |
| **Strict Variants** | Decide between "Separate Products" or add `ParentId` grouping. | High |
| **Search Improvements** | Optimize string matching for better results (e.g. handle typos). | Low |



dotnet ef migrations add AddCollectionsVariantsSearch
dotnet ef database update