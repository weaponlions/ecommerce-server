# eShop Dashboard API — Client Developer Guide

> **Base URL:** `http://localhost:5142`
>
> All endpoints return `application/json`. No authentication is required for public endpoints.

---

## Table of Contents

- [Quick Start](#quick-start)
- [Architecture Overview](#architecture-overview)
- [Public API — Dashboard](#public-api--dashboard)
  - [Get Full Dashboard](#1-get-full-dashboard)
  - [Get Navbar](#2-get-navbar)
  - [Get Carousel](#3-get-carousel)
  - [Get Trending Products](#4-get-trending-products)
  - [Get Recently Visited](#5-get-recently-visited-products)
  - [Track Product Visit](#6-track-a-product-visit)
  - [Get Collections](#7-get-collections)
  - [Get Footer](#8-get-footer)
- [Admin API](#admin-api)
  - [Sections](#sections)
  - [Navbar Links](#navbar-links)
  - [Carousel Slides](#carousel-slides)
  - [Products](#products)
  - [Collections](#collections)
  - [Footer Links](#footer-links)
  - [Social Icons](#social-icons)
- [Building the Dashboard Page](#building-the-dashboard-page)
  - [Recommended Approach](#recommended-approach)
  - [React Example](#react-example)
  - [Vue Example](#vue-example)
  - [Vanilla JS Example](#vanilla-js-example)
- [TypeScript Interfaces](#typescript-interfaces)
- [Error Handling](#error-handling)
- [Tips & Best Practices](#tips--best-practices)

---

## Quick Start

```bash
# 1. Fetch the entire dashboard in a single call
curl http://localhost:5142/api/dashboard?userId=user1

# 2. Or fetch individual sections
curl http://localhost:5142/api/dashboard/navbar
curl http://localhost:5142/api/dashboard/carousel
curl http://localhost:5142/api/dashboard/trending
curl http://localhost:5142/api/dashboard/collections
curl http://localhost:5142/api/dashboard/footer
curl http://localhost:5142/api/dashboard/recently-visited/user1
```

---

## Architecture Overview

```
┌─────────────────────────────────────────────────────────────────┐
│                        CLIENT (Frontend)                        │
│                                                                 │
│   Dashboard Page ─── calls ──▶ GET /api/dashboard?userId=...    │
│   Navbar Component ─ calls ──▶ GET /api/dashboard/navbar        │
│   Product Click ──── calls ──▶ POST /api/dashboard/recently-... │
│   Admin Panel ────── calls ──▶ /api/admin/*                     │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│                        SERVER (ASP.NET)                         │
│                                                                 │
│   DashboardController  →  DashboardService  →  Repositories     │
│   AdminController      →  AdminService      →  Repositories     │
│                                                                 │
│   MySQL (eshop_db)                                              │
└─────────────────────────────────────────────────────────────────┘
```

**Two strategies for building the dashboard:**

| Strategy | Endpoint | When to Use |
|----------|----------|-------------|
| **Single call** | `GET /api/dashboard` | Initial page load — fetches everything at once |
| **Per-section calls** | `GET /api/dashboard/navbar`, etc. | Lazy loading, refreshing a single section |

---

## Public API — Dashboard

### 1. Get Full Dashboard

Fetches all visible dashboard sections in server-defined order, with their data pre-loaded.

```
GET /api/dashboard?userId={userId}
```

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `userId` | `string` (query) | No | Pass to include "Recently Visited" section. Omit for anonymous users. |

**Response** `200 OK`

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
          "imageUrl": "https://images.unsplash.com/...",
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
          "id": 2,
          "name": "Wireless Noise-Cancelling",
          "description": null,
          "price": 249.99,
          "originalPrice": 349.99,
          "imageUrl": "https://images.unsplash.com/...",
          "category": "Electronics",
          "badge": "Bestseller",
          "rating": 4.8,
          "reviewCount": 1205
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
          "imageUrl": "https://images.unsplash.com/...",
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
          }
        ],
        "socialIcons": [
          { "id": 1, "platform": "facebook", "iconRef": "fab fa-facebook-f", "url": "https://facebook.com/eshop" }
        ]
      }
    }
  ]
}
```

**Key: The `data` field is polymorphic — its shape depends on `sectionKey`:**

| `sectionKey` | `data` Type | Description |
|--------------|-------------|-------------|
| `navbar` | `NavbarDto` | `{ links: NavbarLinkDto[] }` |
| `carousel` | `CarouselSlideDto[]` | Array of slides |
| `trending` | `ProductDto[]` | Array of products |
| `recently_visited` | `ProductDto[]` | Array of products (empty if no `userId`) |
| `collections` | `CollectionDto[]` | Array of collections |
| `footer` | `FooterDto` | `{ linkGroups: [...], socialIcons: [...] }` |

---

### 2. Get Navbar

```
GET /api/dashboard/navbar
```

**Response** `200 OK`

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
        { "id": 3, "label": "Men", "url": "/shop/men", "icon": null, "displayOrder": 3, "children": null },
        { "id": 4, "label": "Women", "url": "/shop/women", "icon": null, "displayOrder": 4, "children": null },
        { "id": 5, "label": "Kids", "url": "/shop/kids", "icon": null, "displayOrder": 5, "children": null }
      ]
    }
  ]
}
```

> **Note:** Links are nested via `children`. Top-level links have `children: null` unless they have sub-items. This naturally supports dropdown menus.

---

### 3. Get Carousel

```
GET /api/dashboard/carousel
```

**Response** `200 OK`

```json
[
  {
    "id": 1,
    "title": "Summer Collection 2026",
    "subtitle": "Up to 50% off on all summer essentials",
    "imageUrl": "https://images.unsplash.com/photo-1441986300917-64674bd600d8?w=1920",
    "linkUrl": "/shop/summer",
    "buttonText": "Shop Now",
    "displayOrder": 1
  }
]
```

> **Note:** Only currently active slides are returned (server filters by `startDate`/`endDate` and `isVisible`).

---

### 4. Get Trending Products

```
GET /api/dashboard/trending
```

**Response** `200 OK`

```json
[
  {
    "id": 2,
    "name": "Wireless Noise-Cancelling",
    "description": null,
    "price": 249.99,
    "originalPrice": 349.99,
    "imageUrl": "https://images.unsplash.com/...",
    "category": "Electronics",
    "badge": "Bestseller",
    "rating": 4.8,
    "reviewCount": 1205
  }
]
```

> **Note:** Server limits to top 12 products, sorted by `trendingScore` descending. Only visible products are returned.

---

### 5. Get Recently Visited Products

```
GET /api/dashboard/recently-visited/{userId}
```

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `userId` | `string` (path) | **Yes** | User or session identifier |

**Response** `200 OK` — Returns an array of `ProductDto` (same shape as trending).

> **Note:** Server limits to 20 most recent visits per user, ordered by `visitedAt` descending.

---

### 6. Track a Product Visit

Call this whenever a user views a product detail page.

```
POST /api/dashboard/recently-visited
Content-Type: application/json
```

**Request Body:**

```json
{
  "userId": "user1",
  "productId": 2
}
```

**Response** `200 OK`

```json
{ "message": "Visit tracked." }
```

**Error Responses:**

| Status | Body | Cause |
|--------|------|-------|
| `400` | `{ "error": "UserId is required." }` | Missing or blank `userId` |
| `404` | `{ "error": "Product not found." }` | Invalid `productId` |

> **Note:** If the user already visited this product, the visit timestamp is updated instead of creating a duplicate. The server automatically enforces a max of 20 visits per user and removes the oldest.

---

### 7. Get Collections

```
GET /api/dashboard/collections
```

**Response** `200 OK`

```json
[
  {
    "id": 1,
    "name": "Summer Essentials",
    "description": "Beat the heat in style",
    "imageUrl": "https://images.unsplash.com/...",
    "linkUrl": "/collections/summer",
    "visitCount": 12500
  }
]
```

> **Note:** Server limits to top 10 collections, sorted by `visitCount` descending.

---

### 8. Get Footer

```
GET /api/dashboard/footer
```

**Response** `200 OK`

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
        { "id": 6, "label": "FAQs", "url": "/faq" }
      ]
    },
    {
      "groupName": "Legal",
      "links": [
        { "id": 9, "label": "Privacy Policy", "url": "/privacy" },
        { "id": 10, "label": "Terms of Use", "url": "/terms" }
      ]
    }
  ],
  "socialIcons": [
    { "id": 1, "platform": "facebook", "iconRef": "fab fa-facebook-f", "url": "https://facebook.com/eshop" },
    { "id": 2, "platform": "instagram", "iconRef": "fab fa-instagram", "url": "https://instagram.com/eshop" }
  ]
}
```

---

## Admin API

> **⚠️ These endpoints should be protected by authentication in production.**

All admin endpoints are under `/api/admin`.

### Sections

Dashboard sections control which blocks appear and in what order.

| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/api/admin/sections` | List all sections |
| `PUT` | `/api/admin/sections/{id}` | Update a section |

**PUT Body (`UpsertDashboardSectionRequest`):**
```json
{
  "sectionKey": "trending",
  "title": "Trending Now",
  "displayOrder": 3,
  "isVisible": true,
  "layoutHint": "grid-4"
}
```

> **Note:** Sections cannot be created or deleted — they are structural. You can only toggle `isVisible`, reorder via `displayOrder`, or change the `layoutHint`.

---

### Navbar Links

| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/api/admin/navbar` | List all links |
| `POST` | `/api/admin/navbar` | Create a link → `201 Created` |
| `PUT` | `/api/admin/navbar/{id}` | Update a link |
| `DELETE` | `/api/admin/navbar/{id}` | Delete a link → `204 No Content` |

**POST / PUT Body (`UpsertNavbarLinkRequest`):**
```json
{
  "label": "New Arrivals",
  "url": "/shop/new",
  "icon": "fiber_new",
  "displayOrder": 9,
  "isVisible": true,
  "parentId": null
}
```

> Set `parentId` to another link's `id` to make it a dropdown child.

---

### Carousel Slides

| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/api/admin/carousel` | List all slides |
| `POST` | `/api/admin/carousel` | Create a slide → `201 Created` |
| `PUT` | `/api/admin/carousel/{id}` | Update a slide |
| `DELETE` | `/api/admin/carousel/{id}` | Delete a slide → `204 No Content` |

**POST / PUT Body (`UpsertCarouselSlideRequest`):**
```json
{
  "title": "Flash Sale",
  "subtitle": "24 hours only!",
  "imageUrl": "https://images.unsplash.com/...",
  "linkUrl": "/deals/flash",
  "buttonText": "Shop Now",
  "displayOrder": 4,
  "isVisible": true,
  "startDate": "2026-02-14T00:00:00Z",
  "endDate": "2026-02-15T00:00:00Z"
}
```

> `startDate` and `endDate` are optional. When set, the slide only appears in public API responses while the current time is between these dates.

---

### Products

| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/api/admin/products` | List all products |
| `POST` | `/api/admin/products` | Create a product → `201 Created` |
| `PUT` | `/api/admin/products/{id}` | Update a product |
| `DELETE` | `/api/admin/products/{id}` | Delete a product → `204 No Content` |

**POST / PUT Body (`UpsertProductRequest`):**
```json
{
  "name": "Eco-Friendly Water Bottle",
  "description": "Sustainable hydration solution",
  "price": 24.99,
  "originalPrice": 34.99,
  "imageUrl": "https://images.unsplash.com/...",
  "category": "Lifestyle",
  "badge": "New",
  "rating": 4.5,
  "reviewCount": 42,
  "trendingScore": 65,
  "isVisible": true
}
```

| Field | Rules |
|-------|-------|
| `price` | Must be ≥ 0 |
| `originalPrice` | Optional (strikethrough price). Must be ≥ 0 if set |
| `rating` | Must be between 0 and 5 |
| `trendingScore` | Higher = appears earlier in trending. Must be ≥ 0 |
| `badge` | Optional. Display labels like `"Hot"`, `"Sale"`, `"New"`, `"Bestseller"` |

---

### Collections

| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/api/admin/collections` | List all collections |
| `POST` | `/api/admin/collections` | Create a collection → `201 Created` |
| `PUT` | `/api/admin/collections/{id}` | Update a collection |
| `DELETE` | `/api/admin/collections/{id}` | Delete a collection → `204 No Content` |

**POST / PUT Body (`UpsertCollectionRequest`):**
```json
{
  "name": "Winter Warmers",
  "description": "Cozy essentials for cold days",
  "imageUrl": "https://images.unsplash.com/...",
  "linkUrl": "/collections/winter",
  "visitCount": 0,
  "displayOrder": 7,
  "isVisible": true
}
```

---

### Footer Links

| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/api/admin/footer-links` | List all footer links |
| `POST` | `/api/admin/footer-links` | Create a link → `201 Created` |
| `PUT` | `/api/admin/footer-links/{id}` | Update a link |
| `DELETE` | `/api/admin/footer-links/{id}` | Delete a link → `204 No Content` |

**POST / PUT Body (`UpsertFooterLinkRequest`):**
```json
{
  "groupName": "Company",
  "label": "Sustainability",
  "url": "/sustainability",
  "displayOrder": 5,
  "isVisible": true
}
```

> Links are grouped visually in the footer by `groupName`.

---

### Social Icons

| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/api/admin/social-icons` | List all social icons |
| `POST` | `/api/admin/social-icons` | Create an icon → `201 Created` |
| `PUT` | `/api/admin/social-icons/{id}` | Update an icon |
| `DELETE` | `/api/admin/social-icons/{id}` | Delete an icon → `204 No Content` |

**POST / PUT Body (`UpsertSocialIconRequest`):**
```json
{
  "platform": "tiktok",
  "iconRef": "fab fa-tiktok",
  "url": "https://tiktok.com/@eshop",
  "displayOrder": 6,
  "isVisible": true
}
```

---

## Building the Dashboard Page

### Recommended Approach

The fastest way to build a complete dashboard is to use the **single endpoint** `GET /api/dashboard?userId=...` and dynamically render sections based on `sectionKey`:

```
1. Fetch:    GET /api/dashboard?userId=currentUser
2. Loop:     for each section in response.sections
3. Render:   switch(section.sectionKey) → render the right component
4. Layout:   use section.layoutHint for CSS grid/layout decisions
```

This approach is **server-driven** — the backend controls which sections are visible and in what order. Your frontend just renders whatever it receives.

---

### React Example

```tsx
// api.ts
const API_BASE = "http://localhost:5142/api";

export const fetchDashboard = (userId?: string) =>
  fetch(`${API_BASE}/dashboard${userId ? `?userId=${userId}` : ""}`)
    .then(res => res.json());

export const trackVisit = (userId: string, productId: number) =>
  fetch(`${API_BASE}/dashboard/recently-visited`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ userId, productId }),
  });
```

```tsx
// DashboardPage.tsx
import { useEffect, useState } from "react";
import { fetchDashboard } from "./api";

function DashboardPage({ userId }: { userId?: string }) {
  const [sections, setSections] = useState([]);

  useEffect(() => {
    fetchDashboard(userId).then(data => setSections(data.sections));
  }, [userId]);

  return (
    <div>
      {sections.map(section => (
        <DashboardSection key={section.sectionKey} section={section} />
      ))}
    </div>
  );
}

function DashboardSection({ section }) {
  switch (section.sectionKey) {
    case "navbar":
      return <Navbar links={section.data.links} />;
    case "carousel":
      return <Carousel slides={section.data} />;
    case "trending":
      return (
        <ProductGrid
          title={section.title}
          products={section.data}
          layoutHint={section.layoutHint}    // "grid-4"
        />
      );
    case "recently_visited":
      return section.data.length > 0 ? (
        <ProductScroller
          title={section.title}
          products={section.data}
          layoutHint={section.layoutHint}    // "scroll-horizontal"
        />
      ) : null;
    case "collections":
      return (
        <CollectionGrid
          title={section.title}
          collections={section.data}
          layoutHint={section.layoutHint}    // "grid-3"
        />
      );
    case "footer":
      return (
        <Footer
          linkGroups={section.data.linkGroups}
          socialIcons={section.data.socialIcons}
        />
      );
    default:
      return null;
  }
}
```

---

### Vue Example

```vue
<!-- DashboardPage.vue -->
<template>
  <div>
    <component
      v-for="section in sections"
      :key="section.sectionKey"
      :is="componentMap[section.sectionKey]"
      :section="section"
    />
  </div>
</template>

<script setup>
import { ref, onMounted } from "vue";

const API_BASE = "http://localhost:5142/api";
const sections = ref([]);

const componentMap = {
  navbar: "NavbarSection",
  carousel: "CarouselSection",
  trending: "ProductGridSection",
  recently_visited: "RecentlyVisitedSection",
  collections: "CollectionGridSection",
  footer: "FooterSection",
};

onMounted(async () => {
  const userId = localStorage.getItem("userId");
  const res = await fetch(`${API_BASE}/dashboard${userId ? `?userId=${userId}` : ""}`);
  const data = await res.json();
  sections.value = data.sections;
});
</script>
```

---

### Vanilla JS Example

```html
<div id="dashboard"></div>

<script>
  const API = "http://localhost:5142/api";

  async function loadDashboard(userId) {
    const url = userId
      ? `${API}/dashboard?userId=${userId}`
      : `${API}/dashboard`;
    const res = await fetch(url);
    const { sections } = await res.json();

    const container = document.getElementById("dashboard");
    container.innerHTML = "";

    for (const section of sections) {
      const el = renderSection(section);
      if (el) container.appendChild(el);
    }
  }

  function renderSection(section) {
    switch (section.sectionKey) {
      case "carousel":
        return renderCarousel(section.data);
      case "trending":
        return renderProductGrid(section.title, section.data, section.layoutHint);
      case "collections":
        return renderCollectionGrid(section.title, section.data);
      case "footer":
        return renderFooter(section.data);
      default:
        return null;
    }
  }

  // Track a visit when user clicks a product
  async function onProductClick(userId, productId) {
    await fetch(`${API}/dashboard/recently-visited`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ userId, productId }),
    });
  }

  loadDashboard("user1");
</script>
```

---

## TypeScript Interfaces

Copy these into your frontend project for type safety:

```typescript
// types/dashboard.ts

export interface DashboardResponse {
  sections: DashboardSection[];
}

export interface DashboardSection {
  sectionKey: string;
  title: string;
  displayOrder: number;
  layoutHint: string | null;
  data: NavbarData | CarouselSlide[] | Product[] | Collection[] | FooterData | null;
}

// ── Navbar ──
export interface NavbarData {
  links: NavbarLink[];
}

export interface NavbarLink {
  id: number;
  label: string;
  url: string;
  icon: string | null;
  displayOrder: number;
  children: NavbarLink[] | null;
}

// ── Carousel ──
export interface CarouselSlide {
  id: number;
  title: string;
  subtitle: string | null;
  imageUrl: string;
  linkUrl: string | null;
  buttonText: string | null;
  displayOrder: number;
}

// ── Product ──
export interface Product {
  id: number;
  name: string;
  description: string | null;
  price: number;
  originalPrice: number | null;
  imageUrl: string;
  category: string | null;
  badge: string | null;
  rating: number;
  reviewCount: number;
}

// ── Collection ──
export interface Collection {
  id: number;
  name: string;
  description: string | null;
  imageUrl: string;
  linkUrl: string | null;
  visitCount: number;
}

// ── Footer ──
export interface FooterData {
  linkGroups: FooterGroup[];
  socialIcons: SocialIcon[];
}

export interface FooterGroup {
  groupName: string;
  links: FooterLink[];
}

export interface FooterLink {
  id: number;
  label: string;
  url: string;
}

export interface SocialIcon {
  id: number;
  platform: string;
  iconRef: string;
  url: string;
}

// ── Request Types ──
export interface TrackVisitRequest {
  userId: string;
  productId: number;
}
```

---

## Error Handling

| HTTP Status | Meaning | Action |
|-------------|---------|--------|
| `200` | Success | Parse the JSON body |
| `201` | Created (admin) | Resource was created. `Location` header points to it |
| `204` | Deleted (admin) | No body returned |
| `400` | Bad Request | Check the `error` field in the response body |
| `404` | Not Found | Resource doesn't exist |
| `500` | Server Error | Unexpected error — log and show a fallback UI |

**Example error handling:**

```typescript
async function safeFetch<T>(url: string): Promise<T | null> {
  try {
    const res = await fetch(url);
    if (!res.ok) {
      console.error(`API error ${res.status}: ${await res.text()}`);
      return null;
    }
    return await res.json();
  } catch (err) {
    console.error("Network error:", err);
    return null;
  }
}
```

---

## Tips & Best Practices

### 1. Use `layoutHint` for responsive grids

The `layoutHint` field tells you how the admin intends the section to be displayed:

| `layoutHint` | Suggested CSS |
|--------------|---------------|
| `"grid-4"` | `display: grid; grid-template-columns: repeat(4, 1fr)` |
| `"grid-3"` | `display: grid; grid-template-columns: repeat(3, 1fr)` |
| `"scroll-horizontal"` | `display: flex; overflow-x: auto` |
| `null` | Use your default layout for that section type |

### 2. Show product badges

Use the `badge` field on products to render small labels:

```tsx
{product.badge && <span className="badge">{product.badge}</span>}
```

Common values: `"Hot"`, `"Sale"`, `"New"`, `"Bestseller"`, `"Trending"`, `"Popular"`, `"Deal"`

### 3. Show discount percentages

Calculate from `price` and `originalPrice`:

```typescript
const discount = product.originalPrice
  ? Math.round(((product.originalPrice - product.price) / product.originalPrice) * 100)
  : null;
```

### 4. Handle the `recently_visited` section gracefully

- If the user is anonymous, the `data` array will be empty — **hide the section entirely**.
- Call `POST /api/dashboard/recently-visited` on every product page view.
- Use `localStorage` or your auth system for `userId`.

### 5. Navbar dropdown support

Links with `children !== null` should render as dropdown menus. The nesting is recursive (children can have children), but typically only 1 level deep.

### 6. Social icons use Font Awesome classes

The `iconRef` field uses Font Awesome class names (e.g., `"fab fa-facebook-f"`). Make sure Font Awesome is loaded in your frontend:

```html
<link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.5.0/css/all.min.css" />
```

Then render:
```html
<a href={icon.url}><i className={icon.iconRef}></i></a>
```

### 7. CORS

The server allows all origins in development. No special headers are needed from the client side.

---

*Last updated: February 10, 2026*
