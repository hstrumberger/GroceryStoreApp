# GroceryStoreApp

A full-stack online grocery store built with ASP.NET Core 10 and React 19. Supports product browsing, search, sales/discounts, a persistent shopping cart, and a simulated checkout flow.

---

## Features

### Shopping
- Browse products by category with sorting (name, price, rating)
- Full-text search across product name, description, and SKU
- Product detail pages with image gallery, stock status, and ratings

### Sales & Discounts
- Active sales banner on the home page
- Per-product and per-category discounts (percentage or fixed amount)
- Sale badges and struck-through original prices on product cards and detail pages
- Best available sale automatically applied per product

### Cart
- Persistent, database-backed cart (survives browser close/reopen)
- Add, update quantity, and remove items
- Live sale prices applied at cart and checkout time

### Checkout
- Shipping address form with validation
- Simulated payment form (credit/debit card — no real gateway)
- Tax (8%) and shipping ($9.99, free over $75) calculated server-side
- Stock verified and decremented transactionally on order placement
- Order confirmation page with full breakdown

### Authentication
- Register and login with email/password
- JWT access token (15 min) stored in memory
- Refresh token (7 days) stored in an HttpOnly `SameSite=Strict` cookie
- Automatic silent token refresh on expiry
- Account lockout after 5 failed login attempts
- Users can only access their own cart and orders

---

## Tech Stack

| Layer | Technology |
|---|---|
| Backend | ASP.NET Core 10 Web API |
| ORM | Entity Framework Core 9 |
| Database | SQL Server (dev: SQL Server Express) |
| Auth | ASP.NET Core Identity + JWT Bearer |
| Frontend | React 19 + TypeScript + Vite |
| State | Zustand (auth), TanStack Query (server state) |
| Forms | react-hook-form + zod |
| HTTP | Axios with auto-refresh interceptor |
| API Docs | Swagger / OpenAPI |

---

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js 18+](https://nodejs.org/)
- SQL Server or SQL Server Express

---

## Getting Started

### 1. Clone the repository

```bash
git clone https://github.com/hstrumberger/GroceryStoreApp.git
cd GroceryStoreApp
```

### 2. Configure the backend

Copy the settings template and fill in your values:

```bash
cp GroceryStoreApp/appsettings.template.json GroceryStoreApp/appsettings.json
```

Edit `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Database=GroceryStoreDb;Trusted_Connection=True;"
  },
  "JwtSettings": {
    "SecretKey": "your-random-secret-at-least-32-characters-long"
  }
}
```

> The JWT secret must be at least 32 characters. Generate one with:
> `openssl rand -base64 32`

### 3. Apply the database migration

```bash
cd GroceryStoreApp
dotnet ef database update
```

This creates the database and all tables. Seed data (categories, products, and a sample sale) is loaded automatically on first startup.

### 4. Run the backend

```bash
dotnet run
```

The API will be available at `http://localhost:5001`.
Swagger UI is at `http://localhost:5001/swagger`.

### 5. Run the frontend

In a separate terminal:

```bash
cd grocerystore-client
npm install
npm run dev
```

The app will be available at `http://localhost:5173`.

---

## Project Structure

```
GroceryStoreApp/
├── GroceryStoreApp/                  # ASP.NET Core Web API
│   ├── Controllers/                  # AuthController, ProductsController, CartController, etc.
│   ├── Data/
│   │   ├── AppDbContext.cs
│   │   ├── Migrations/
│   │   └── SeedData.cs
│   ├── DTOs/                         # Request/response shapes
│   ├── Middleware/                   # Global exception handler
│   ├── Models/                       # EF Core entities
│   ├── Services/
│   │   ├── Interfaces/
│   │   └── Implementations/
│   ├── appsettings.template.json     # Config template (copy to appsettings.json)
│   └── Program.cs
│
└── grocerystore-client/              # React SPA
    └── src/
        ├── api/                      # Axios API modules
        ├── components/               # Header, ProductCard, SaleBadge, etc.
        ├── pages/                    # Route-level page components
        ├── store/                    # Zustand auth store
        └── types/                    # TypeScript interfaces
```

---

## API Endpoints

| Method | Path | Auth | Description |
|---|---|---|---|
| POST | `/api/auth/register` | — | Create account |
| POST | `/api/auth/login` | — | Sign in |
| POST | `/api/auth/refresh` | Cookie | Refresh access token |
| POST | `/api/auth/logout` | JWT | Revoke refresh token |
| GET | `/api/auth/me` | JWT | Current user info |
| GET | `/api/products` | — | List products (filter, sort, paginate) |
| GET | `/api/products/{id}` | — | Product detail |
| GET | `/api/categories` | — | All categories |
| GET | `/api/categories/{slug}/products` | — | Products by category |
| GET | `/api/search?q=` | — | Search products |
| GET | `/api/cart` | JWT | Get cart |
| POST | `/api/cart/items` | JWT | Add item |
| PUT | `/api/cart/items/{productId}` | JWT | Update quantity |
| DELETE | `/api/cart/items/{productId}` | JWT | Remove item |
| DELETE | `/api/cart` | JWT | Clear cart |
| POST | `/api/checkout` | JWT | Place order |
| GET | `/api/orders` | JWT | Order history |
| GET | `/api/orders/{id}` | JWT | Order detail |
| GET | `/api/sales/active` | — | Active sales |
| GET | `/api/sales/{id}` | — | Sale detail |

---

## Testing the API

Open `http://localhost:5001/swagger`, then:

1. `POST /api/auth/register` to create an account
2. `POST /api/auth/login` to get an access token
3. Click **Authorize** in Swagger and enter `Bearer <token>`
4. All protected endpoints are now accessible

---

## Seed Data

On first startup the app seeds:

- **7 categories** — Produce, Dairy & Eggs, Bakery, Meat & Seafood, Pantry, Beverages, Frozen Foods
- **24 products** across all categories
- **1 active sale** — 20% off all Produce items
