# eShopServer — Frontend API Reference

> **Purpose:** This document is a complete reference for building an e-commerce frontend against the eShopServer public API. It contains every public endpoint, every response shape, every query parameter, and example JSON responses. Use this as the single source of truth for API integration.
>
> **Base URL:** `http://localhost:5142`
> **Content-Type:** All responses are `application/json`
> **CORS:** Open (AllowAnyOrigin) during development
> **Media Assets:** Images are referenced by `mediaAssetId` (integer). Resolve to URL via `/api/admin/media/{id}` or serve from `{baseUrl}/uploads/{filename}`.

---

## Table of Contents

1. [Overview & Architecture](#overview--architecture)
2. [Dashboard API](#dashboard-api)
   - [Full Dashboard](#1-get-full-dashboard)
   - [Navbar](#2-get-navbar)
   - [Carousel](#3-get-carousel-slides)
   - [Trending Products](#4-get-trending-products)
   - [Recently Visited](#5-get-recently-visited-products)
   - [Track Visit](#6-post-track-product-visit)
   - [Collections](#7-get-collections)
   - [Footer](#8-get-footer)
3. [Product API](#product-api)
   - [List Categories](#9-get-product-categories)
   - [Category Detail](#10-get-category-by-slug)
   - [Browse Products (with filters)](#11-get-browse--filter-products)
   - [Product Detail](#12-get-product-detail)
4. [TypeScript Interfaces](#typescript-interfaces)
5. [Frontend Page Mapping](#frontend-page-mapping)
6. [Media Asset Resolution](#media-asset-resolution)

---

## Overview & Architecture

The API is split into two modules:

| Module | Purpose | Base Route |
|--------|---------|------------|
| **Dashboard** | Homepage data — navbar, carousel, trending, collections, footer | `/api/dashboard` |
| **Products** | Product catalog — categories, search, filter, detail | `/api/products` |

**Key concepts:**
- **Server-driven UI:** The dashboard endpoint returns sections in an admin-configured order. The frontend should render sections in the order received.
- **Media Assets:** All images use `mediaAssetId` (nullable integer). The frontend resolves these to actual image URLs.
- **Dynamic Attributes:** Product categories define custom attributes (like "Color", "Size", "RAM"). These are used for filtering and display.

---

## Dashboard API

### 1. `GET` Full Dashboard

```
GET /api/dashboard?userId={userId}
```

Returns the **entire homepage** in a single call. Sections are sorted by server-defined order. Each section has a `sectionKey` identifying what it is and a `data` field with the actual content.

| Param | Type | Required | Description |
|-------|------|----------|-------------|
| `userId` | `string` | ❌ | Pass to include "recently visited" section. Omit to skip it. |

**Response:**

```json
{
  "sections": [
    {
      "sectionKey": "navbar",
      "title": "Navigation",
      "displayOrder": 1,
      "layoutHint": null,
      "data": {
        "links": [
          {
            "id": 1,
            "label": "Home",
            "url": "/",
            "icon": "home",
            "displayOrder": 1,
            "children": null
          },
          {
            "id": 2,
            "label": "Shop",
            "url": "/shop",
            "icon": "storefront",
            "displayOrder": 2,
            "children": [
              {
                "id": 3,
                "label": "Men",
                "url": "/shop/men",
                "icon": null,
                "displayOrder": 3,
                "children": null
              },
              {
                "id": 4,
                "label": "Women",
                "url": "/shop/women",
                "icon": null,
                "displayOrder": 4,
                "children": null
              }
            ]
          }
        ]
      }
    },
    {
      "sectionKey": "carousel",
      "title": "Hero Carousel",
      "displayOrder": 2,
      "layoutHint": null,
      "data": [
        {
          "id": 1,
          "title": "Summer Collection 2026",
          "subtitle": "Up to 50% off on all summer essentials",
          "mediaAssetId": 5,
          "linkUrl": "/shop/summer",
          "buttonText": "Shop Now",
          "displayOrder": 1
        }
      ]
    },
    {
      "sectionKey": "trending",
      "title": "Trending Now",
      "displayOrder": 3,
      "layoutHint": "grid-4",
      "data": [
        {
          "id": 1,
          "name": "Classic White Sneakers",
          "description": null,
          "price": 89.99,
          "originalPrice": 129.99,
          "mediaAssetId": 12,
          "categoryLabel": "Footwear",
          "badge": "Hot",
          "rating": 4.5,
          "reviewCount": 342
        }
      ]
    },
    {
      "sectionKey": "recently_visited",
      "title": "Recently Visited",
      "displayOrder": 4,
      "layoutHint": "scroll-horizontal",
      "data": []
    },
    {
      "sectionKey": "collections",
      "title": "Most Visited Collections",
      "displayOrder": 5,
      "layoutHint": "grid-3",
      "data": [
        {
          "id": 1,
          "name": "Summer Essentials",
          "description": "Beat the heat in style",
          "mediaAssetId": 20,
          "linkUrl": "/collections/summer",
          "visitCount": 12500
        }
      ]
    },
    {
      "sectionKey": "footer",
      "title": "Footer",
      "displayOrder": 6,
      "layoutHint": null,
      "data": {
        "linkGroups": [
          {
            "groupName": "Company",
            "links": [
              { "id": 1, "label": "About Us", "url": "/about" },
              { "id": 2, "label": "Careers", "url": "/careers" }
            ]
          },
          {
            "groupName": "Help",
            "links": [
              { "id": 5, "label": "Contact Us", "url": "/contact" },
              { "id": 6, "label": "FAQs", "url": "/faq" }
            ]
          }
        ],
        "socialIcons": [
          {
            "id": 1,
            "platform": "facebook",
            "iconRef": "fab fa-facebook-f",
            "mediaAssetId": null,
            "url": "https://facebook.com/eshop"
          },
          {
            "id": 2,
            "platform": "instagram",
            "iconRef": "fab fa-instagram",
            "mediaAssetId": 25,
            "url": "https://instagram.com/eshop"
          }
        ]
      }
    }
  ]
}
```

**Section key → data type mapping:**

| `sectionKey` | `data` type | Description |
|--------------|-------------|-------------|
| `navbar` | `NavbarDto` | Object with `links` array (nested tree) |
| `carousel` | `CarouselSlideDto[]` | Array of slide objects |
| `trending` | `ProductDto[]` | Array of product cards |
| `recently_visited` | `ProductDto[]` | Array of product cards (empty if no userId) |
| `collections` | `CollectionDto[]` | Array of collection cards |
| `footer` | `FooterDto` | Object with `linkGroups` + `socialIcons` |

**`layoutHint` values:** Suggested layout from the server. Current values: `"grid-4"`, `"grid-3"`, `"scroll-horizontal"`, or `null`.

---

### 2. `GET` Navbar

```
GET /api/dashboard/navbar
```

Returns the navbar as a standalone call (same structure as the `navbar` section in the full dashboard).

**Response:**

```json
{
  "links": [
    {
      "id": 1,
      "label": "Home",
      "url": "/",
      "icon": "home",
      "displayOrder": 1,
      "children": null
    },
    {
      "id": 2,
      "label": "Shop",
      "url": "/shop",
      "icon": "storefront",
      "displayOrder": 2,
      "children": [
        {
          "id": 3,
          "label": "Men",
          "url": "/shop/men",
          "icon": null,
          "displayOrder": 3,
          "children": null
        }
      ]
    }
  ]
}
```

> **Note:** Links are returned in a tree structure. Top-level links have `children: null` or an array of nested `NavbarLinkDto`. The nesting is recursive.

---

### 3. `GET` Carousel Slides

```
GET /api/dashboard/carousel
```

Returns active carousel slides. The server automatically filters by `StartDate` / `EndDate` scheduling.

**Response:**

```json
[
  {
    "id": 1,
    "title": "Summer Collection 2026",
    "subtitle": "Up to 50% off on all summer essentials",
    "mediaAssetId": 5,
    "linkUrl": "/shop/summer",
    "buttonText": "Shop Now",
    "displayOrder": 1
  },
  {
    "id": 2,
    "title": "New Arrivals",
    "subtitle": "Check out the latest trends",
    "mediaAssetId": 6,
    "linkUrl": "/shop/new",
    "buttonText": "Explore",
    "displayOrder": 2
  }
]
```

---

### 4. `GET` Trending Products

```
GET /api/dashboard/trending
```

Returns top trending products (server limits quantity, sorted by `trendingScore` descending).

**Response:**

```json
[
  {
    "id": 2,
    "name": "Wireless Noise-Cancelling Headphones",
    "description": "Premium sound quality",
    "price": 249.99,
    "originalPrice": 349.99,
    "mediaAssetId": 12,
    "categoryLabel": "Electronics",
    "badge": "Bestseller",
    "rating": 4.8,
    "reviewCount": 1205
  },
  {
    "id": 1,
    "name": "Classic White Sneakers",
    "description": null,
    "price": 89.99,
    "originalPrice": 129.99,
    "mediaAssetId": 10,
    "categoryLabel": "Footwear",
    "badge": "Hot",
    "rating": 4.5,
    "reviewCount": 342
  }
]
```

---

### 5. `GET` Recently Visited Products

```
GET /api/dashboard/recently-visited/{userId}
```

| Param | Type | Required | Description |
|-------|------|----------|-------------|
| `userId` | `string` (path) | ✅ | Unique user identifier (can be device ID, session ID, or auth user ID) |

**Response:** Same shape as trending — array of `ProductDto`:

```json
[
  {
    "id": 6,
    "name": "Designer Sunglasses",
    "description": null,
    "price": 199.99,
    "originalPrice": null,
    "mediaAssetId": 18,
    "categoryLabel": "Accessories",
    "badge": "Trending",
    "rating": 4.7,
    "reviewCount": 234
  }
]
```

---

### 6. `POST` Track Product Visit

```
POST /api/dashboard/recently-visited
Content-Type: application/json
```

Call this when a user views a product detail page to track their browsing history.

**Request Body:**

```json
{
  "userId": "device_abc123",
  "productId": 6
}
```

**Response (success):**

```json
{
  "message": "Visit tracked."
}
```

**Response (product not found):**

```json
// HTTP 404
{
  "error": "Product not found."
}
```

---

### 7. `GET` Collections

```
GET /api/dashboard/collections
```

Returns the most visited collections (server limits quantity, sorted by `visitCount` descending).

**Response:**

```json
[
  {
    "id": 1,
    "name": "Summer Essentials",
    "description": "Beat the heat in style",
    "mediaAssetId": 20,
    "linkUrl": "/collections/summer",
    "visitCount": 12500
  },
  {
    "id": 4,
    "name": "Tech Gadgets",
    "description": "Innovation at your fingertips",
    "mediaAssetId": 22,
    "linkUrl": "/collections/tech",
    "visitCount": 11200
  }
]
```

> ⚠️ **Note:** Collections currently don't have a product relationship. The `linkUrl` is a frontend route the admin has configured. In a future update, there will be a `GET /api/collections/{id}/products` endpoint to fetch actual products within a collection.

---

### 8. `GET` Footer

```
GET /api/dashboard/footer
```

Returns footer link groups and social media icons.

**Response:**

```json
{
  "linkGroups": [
    {
      "groupName": "Company",
      "links": [
        { "id": 1, "label": "About Us", "url": "/about" },
        { "id": 2, "label": "Careers", "url": "/careers" },
        { "id": 3, "label": "Press", "url": "/press" },
        { "id": 4, "label": "Blog", "url": "/blog" }
      ]
    },
    {
      "groupName": "Help",
      "links": [
        { "id": 5, "label": "Contact Us", "url": "/contact" },
        { "id": 6, "label": "FAQs", "url": "/faq" },
        { "id": 7, "label": "Shipping Info", "url": "/shipping" },
        { "id": 8, "label": "Returns", "url": "/returns" }
      ]
    },
    {
      "groupName": "Legal",
      "links": [
        { "id": 9, "label": "Privacy Policy", "url": "/privacy" },
        { "id": 10, "label": "Terms of Use", "url": "/terms" },
        { "id": 11, "label": "Cookie Policy", "url": "/cookies" }
      ]
    }
  ],
  "socialIcons": [
    {
      "id": 1,
      "platform": "facebook",
      "iconRef": "fab fa-facebook-f",
      "mediaAssetId": null,
      "url": "https://facebook.com/eshop"
    },
    {
      "id": 2,
      "platform": "instagram",
      "iconRef": "fab fa-instagram",
      "mediaAssetId": null,
      "url": "https://instagram.com/eshop"
    },
    {
      "id": 3,
      "platform": "twitter",
      "iconRef": "fab fa-twitter",
      "mediaAssetId": null,
      "url": "https://twitter.com/eshop"
    }
  ]
}
```

> **Social icons:** If `mediaAssetId` is set, use the uploaded image. If `null`, use `iconRef` as a CSS class (e.g., Font Awesome).

---

## Product API

### 9. `GET` Product Categories

```
GET /api/products/categories
```

Returns all **active** product categories. Use this to build a category navigation sidebar or dropdown.

**Response:**

```json
[
  {
    "id": 1,
    "name": "Smartphones",
    "slug": "smartphones",
    "description": "Mobile phones and accessories",
    "mediaAssetId": 30
  },
  {
    "id": 2,
    "name": "Shoes",
    "slug": "shoes",
    "description": "Footwear for all",
    "mediaAssetId": 31
  },
  {
    "id": 3,
    "name": "Clothing",
    "slug": "clothing",
    "description": "Apparel and fashion",
    "mediaAssetId": 32
  }
]
```

---

### 10. `GET` Category by Slug

```
GET /api/products/categories/{slug}
```

Returns a single category **with its attribute definitions**. The client uses this to build a dynamic filter sidebar for that category.

| Param | Type | Required | Description |
|-------|------|----------|-------------|
| `slug` | `string` (path) | ✅ | Category slug (e.g., `smartphones`, `shoes`) |

**Response:**

```json
{
  "id": 1,
  "name": "Smartphones",
  "slug": "smartphones",
  "description": "Mobile phones and accessories",
  "mediaAssetId": 30,
  "isActive": true,
  "attributes": [
    {
      "id": 1,
      "name": "brand",
      "displayName": "Brand",
      "dataType": "Select",
      "isRequired": true,
      "isFilterable": true,
      "options": ["Apple", "Samsung", "OnePlus", "Xiaomi", "Google"],
      "displayOrder": 1
    },
    {
      "id": 2,
      "name": "ram",
      "displayName": "RAM",
      "dataType": "Select",
      "isRequired": true,
      "isFilterable": true,
      "options": ["4GB", "6GB", "8GB", "12GB", "16GB"],
      "displayOrder": 2
    },
    {
      "id": 3,
      "name": "storage",
      "displayName": "Storage",
      "dataType": "Select",
      "isRequired": false,
      "isFilterable": true,
      "options": ["64GB", "128GB", "256GB", "512GB", "1TB"],
      "displayOrder": 3
    },
    {
      "id": 4,
      "name": "5g_supported",
      "displayName": "5G Supported",
      "dataType": "Boolean",
      "isRequired": false,
      "isFilterable": true,
      "options": null,
      "displayOrder": 4
    },
    {
      "id": 5,
      "name": "screen_size",
      "displayName": "Screen Size (inches)",
      "dataType": "Number",
      "isRequired": false,
      "isFilterable": false,
      "options": null,
      "displayOrder": 5
    }
  ]
}
```

**Attribute `dataType` values and how to render them:**

| `dataType` | Filter UI Component | Example Values |
|------------|-------------------|----------------|
| `Text` | Text input | Any free text |
| `Number` | Number input / range slider | `6.7`, `128` |
| `Boolean` | Checkbox / Toggle | `true` or `false` |
| `Select` | Dropdown / Radio buttons | One of `options[]` array |
| `MultiSelect` | Multi-checkbox / Tag list | Multiple from `options[]` array |

**Only show attributes where `isFilterable: true`** in the filter sidebar. Non-filterable attributes are still shown on the product detail page but shouldn't be used as filters.

---

### 11. `GET` Browse / Filter Products

```
GET /api/products?{query params}
```

This is the **main product listing endpoint**. Supports category filtering, price range, text search, dynamic attribute filtering, sorting, and pagination.

#### All Query Parameters

| Param | Type | Required | Default | Description |
|-------|------|----------|---------|-------------|
| `categoryId` | `int` | ❌ | — | Filter by category ID |
| `categorySlug` | `string` | ❌ | — | Filter by category slug (alternative to `categoryId`) |
| `minPrice` | `decimal` | ❌ | — | Minimum price (inclusive) |
| `maxPrice` | `decimal` | ❌ | — | Maximum price (inclusive) |
| `search` | `string` | ❌ | — | Text search across product name, description, and category label |
| `sortBy` | `string` | ❌ | `trending` | Sort field (see table below) |
| `sortDescending` | `bool` | ❌ | `false` | Sort direction |
| `page` | `int` | ❌ | `1` | Page number (1-indexed) |
| `pageSize` | `int` | ❌ | `20` | Items per page (max: 100) |
| `attr_{name}` | `string` | ❌ | — | Dynamic attribute filter (see below) |

**Sort options:**

| `sortBy` value | Orders by |
|---------------|-----------|
| `price` | Product price |
| `name` | Product name (alphabetical) |
| `rating` | Product rating |
| `newest` | Creation date (most recent first, ignores `sortDescending`) |
| *(omitted or anything else)* | Trending score (default, most trending first) |

#### Dynamic Attribute Filtering

To filter by product attributes, prefix the attribute `name` (from the category attributes response) with `attr_`:

```
GET /api/products?categorySlug=smartphones&attr_brand=Samsung&attr_ram=8GB
```

- Attribute names are **case-insensitive**
- Multiple attribute filters use **AND** logic (all must match)
- If an attribute name doesn't exist on a product, that product is excluded
- Invalid attribute names don't cause errors — they just filter out products

#### Example Requests

**All products (default sort by trending):**
```
GET /api/products
```

**Products in a category:**
```
GET /api/products?categorySlug=smartphones
```

**Filtered + sorted + paginated:**
```
GET /api/products?categorySlug=smartphones&attr_brand=Samsung&attr_ram=8GB&minPrice=20000&maxPrice=50000&sortBy=price&sortDescending=false&page=1&pageSize=12
```

**Text search across all categories:**
```
GET /api/products?search=wireless+headphones&sortBy=rating&sortDescending=true
```

#### Response

```json
{
  "items": [
    {
      "id": 1,
      "name": "Samsung Galaxy S26",
      "price": 34999.00,
      "originalPrice": 44999.00,
      "mediaAssetId": 12,
      "categoryLabel": "Electronics",
      "badge": "Bestseller",
      "rating": 4.7,
      "reviewCount": 1205,
      "stock": 45
    },
    {
      "id": 5,
      "name": "Samsung Galaxy A55",
      "price": 27999.00,
      "originalPrice": null,
      "mediaAssetId": 15,
      "categoryLabel": "Electronics",
      "badge": null,
      "rating": 4.3,
      "reviewCount": 567,
      "stock": 120
    }
  ],
  "totalCount": 8,
  "page": 1,
  "pageSize": 12,
  "totalPages": 1
}
```

> ⚠️ **The list response does NOT include product attributes.** It's a lightweight DTO for grid/list views. To get attributes, fetch the individual product via `GET /api/products/{id}`.

---

### 12. `GET` Product Detail

```
GET /api/products/{id}
```

Returns a single product with **full details including all attribute values**.

| Param | Type | Required | Description |
|-------|------|----------|-------------|
| `id` | `int` (path) | ✅ | Product ID |

**Response:**

```json
{
  "id": 1,
  "name": "Samsung Galaxy S26",
  "description": "Flagship smartphone with 200MP camera, Snapdragon 8 Gen 5, and all-day battery",
  "price": 34999.00,
  "originalPrice": 44999.00,
  "mediaAssetId": 12,
  "categoryLabel": "Electronics",
  "badge": "Bestseller",
  "rating": 4.7,
  "reviewCount": 1205,
  "stock": 45,
  "categoryId": 1,
  "categoryName": "Smartphones",
  "categorySlug": "smartphones",
  "attributes": {
    "brand": "Samsung",
    "ram": "8GB",
    "storage": "256GB",
    "5g_supported": true,
    "screen_size": 6.7,
    "color": "Phantom Black",
    "features": ["Water Resistant", "Wireless Charging", "NFC"]
  }
}
```

**Attribute value types in the `attributes` dictionary:**

| Category Attribute `dataType` | JSON Type | Example |
|-------------------------------|-----------|---------|
| `Text` | `string` | `"Phantom Black"` |
| `Number` | `number` | `6.7` |
| `Boolean` | `boolean` | `true` |
| `Select` | `string` | `"Samsung"` |
| `MultiSelect` | `string[]` | `["Water Resistant", "NFC"]` |

---

## TypeScript Interfaces

Copy these into your frontend project for type safety:

```typescript
// ══════════════════════════════════════════════════════════════
//  Dashboard Types
// ══════════════════════════════════════════════════════════════

interface DashboardResponse {
  sections: DashboardSection[];
}

interface DashboardSection {
  sectionKey: "navbar" | "carousel" | "trending" | "recently_visited" | "collections" | "footer";
  title: string;
  displayOrder: number;
  layoutHint: string | null;        // "grid-4", "grid-3", "scroll-horizontal", or null
  data: NavbarDto | CarouselSlideDto[] | ProductDto[] | CollectionDto[] | FooterDto | null;
}

// ── Navbar ──

interface NavbarDto {
  links: NavbarLinkDto[];
}

interface NavbarLinkDto {
  id: number;
  label: string;
  url: string;
  icon: string | null;              // Material icon name or null
  displayOrder: number;
  children: NavbarLinkDto[] | null;  // Recursive nesting
}

// ── Carousel ──

interface CarouselSlideDto {
  id: number;
  title: string;
  subtitle: string | null;
  mediaAssetId: number | null;
  linkUrl: string | null;
  buttonText: string | null;
  displayOrder: number;
}

// ── Product (card / list item) ──

interface ProductDto {
  id: number;
  name: string;
  description: string | null;
  price: number;
  originalPrice: number | null;
  mediaAssetId: number | null;
  categoryLabel: string | null;
  badge: string | null;             // "Hot", "Bestseller", "Sale", "New", etc.
  rating: number;                   // 0.0 – 5.0
  reviewCount: number;
}

// ── Collection ──

interface CollectionDto {
  id: number;
  name: string;
  description: string | null;
  mediaAssetId: number | null;
  linkUrl: string | null;
  visitCount: number;
}

// ── Footer ──

interface FooterDto {
  linkGroups: FooterGroupDto[];
  socialIcons: SocialIconDto[];
}

interface FooterGroupDto {
  groupName: string;
  links: FooterLinkDto[];
}

interface FooterLinkDto {
  id: number;
  label: string;
  url: string;
}

interface SocialIconDto {
  id: number;
  platform: string;               // "facebook", "instagram", "twitter", etc.
  iconRef: string;                 // CSS class like "fab fa-facebook-f"
  mediaAssetId: number | null;     // If set, use uploaded image instead of iconRef
  url: string;                     // Full URL to social profile
}

// ── Recently Visited ──

interface TrackVisitRequest {
  userId: string;
  productId: number;
}

// ══════════════════════════════════════════════════════════════
//  Product Module Types
// ══════════════════════════════════════════════════════════════

// ── Category ──

interface CategoryListResponse {
  id: number;
  name: string;
  slug: string;
  description: string | null;
  mediaAssetId: number | null;
}

interface CategoryResponse {
  id: number;
  name: string;
  slug: string;
  description: string | null;
  mediaAssetId: number | null;
  isActive: boolean;
  attributes: CategoryAttributeResponse[] | null;
}

interface CategoryAttributeResponse {
  id: number;
  name: string;                    // Lowercase key, e.g. "brand", "ram"
  displayName: string;             // UI label, e.g. "Brand", "RAM"
  dataType: "Text" | "Number" | "Boolean" | "Select" | "MultiSelect";
  isRequired: boolean;
  isFilterable: boolean;           // true = show in filter sidebar
  options: string[] | null;        // Available options for Select/MultiSelect
  displayOrder: number;
}

// ── Product List ──

interface ProductListItemResponse {
  id: number;
  name: string;
  price: number;
  originalPrice: number | null;
  mediaAssetId: number | null;
  categoryLabel: string | null;
  badge: string | null;
  rating: number;
  reviewCount: number;
  stock: number;
}

interface PagedResponse<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

// ── Product Detail ──

interface ProductDetailResponse {
  id: number;
  name: string;
  description: string | null;
  price: number;
  originalPrice: number | null;
  mediaAssetId: number | null;
  categoryLabel: string | null;
  badge: string | null;
  rating: number;
  reviewCount: number;
  stock: number;
  categoryId: number | null;
  categoryName: string | null;
  categorySlug: string | null;
  attributes: Record<string, string | number | boolean | string[] | null>;
}
```

---

## Frontend Page Mapping

Suggested pages and which APIs they should call:

| Page | APIs to Call | Notes |
|------|-------------|-------|
| **Home / Dashboard** | `GET /api/dashboard?userId=xxx` | Single call returns everything. Render sections in `displayOrder`. |
| **Category Listing** | `GET /api/products/categories` | Show all categories with images. |
| **Product Listing / Shop** | 1. `GET /api/products/categories/{slug}` (for filter sidebar) | Step 1: Get category attributes for filters. |
| | 2. `GET /api/products?categorySlug=...&attr_...&...` | Step 2: Fetch filtered product list. |
| **Search Results** | `GET /api/products?search=...` | Text search across all categories. |
| **Product Detail** | 1. `GET /api/products/{id}` | Full product with attributes. |
| | 2. `POST /api/dashboard/recently-visited` | Track the visit for later. |
| **Collection Page** | `GET /api/dashboard/collections` | ⚠️ Currently display-only (no product relationship). Use `linkUrl` to navigate. |

### Typical User Flow

```
     Home Page                  Category Page              Product Page
  ┌──────────────┐          ┌──────────────────┐       ┌──────────────────┐
  │ GET /api/    │          │ GET /categories/ │       │ GET /products/1  │
  │  dashboard   │──click──▶│  smartphones     │       │                  │
  │              │ category │                  │       │ Shows all attrs: │
  │ • Navbar     │          │ Renders filters: │       │ brand: Samsung   │
  │ • Carousel   │          │ ☐ Brand          │       │ ram: 8GB         │
  │ • Trending   │          │ ☐ RAM            │       │ 5g: true         │
  │ • Collections│          │ ☐ 5G             │       │                  │
  │ • Footer     │          │ $ Price range    │       │ POST /recently-  │
  │              │          │                  │──click──▶ visited        │
  │              │          │ GET /products?   │ product└──────────────────┘
  │              │          │  categorySlug=   │
  │              │          │  smartphones&    │
  │              │          │  attr_brand=     │
  │              │          │  Samsung&page=1  │
  └──────────────┘          └──────────────────┘
```

---

## Media Asset Resolution

All image references in the API use `mediaAssetId` (an integer or `null`).

**How to resolve `mediaAssetId` to an image URL:**

| Approach | How | Best For |
|----------|-----|----------|
| **Direct file serving** | `{baseUrl}/uploads/{filename}` | If you know the filename pattern |
| **API lookup** | `GET /api/admin/media/{mediaAssetId}` → returns `{ url: "/uploads/abc123.jpg", ... }` | General use |

**Handling `null`:**
- If `mediaAssetId` is `null`, show a placeholder image
- Carousel slides, collections, products, and categories may all have `null` images

**Example helper function:**

```typescript
const BASE_URL = "http://localhost:5142";

function getImageUrl(mediaAssetId: number | null): string {
  if (!mediaAssetId) return "/placeholder.png";
  // Option 1: If you prefetch media assets and cache them
  return cachedMediaMap[mediaAssetId]?.url ?? "/placeholder.png";
  // Option 2: Direct API call (use sparingly, cache results)
  // const response = await fetch(`${BASE_URL}/api/admin/media/${mediaAssetId}`);
  // return (await response.json()).url;
}
```

---

## Error Response Format

All errors follow a consistent format:

```json
{
  "error": "Description of what went wrong"
}
```

| HTTP Status | Meaning |
|-------------|---------|
| `200` | Success |
| `201` | Created (for POST endpoints) |
| `400` | Bad request (validation error) |
| `404` | Resource not found |
| `500` | Server error |
