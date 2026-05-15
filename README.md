# KitVault E-Commerce Assignment

KitVault is a demonstrative e-commerce app built with Angular and ASP.NET Core Web API. It covers registration, login, product browsing, local cart state, checkout, stock validation, and user order history.

The assignment brief asked for a vertical slice of an e-commerce cart and checkout app using Angular, .NET, and MS SQL without an ORM. This project uses ADO.NET directly and keeps the backend split into controllers, services, repositories, DTOs, models, mappings, and helpers.

## Prerequisites

- .NET 10 SDK
- Node.js and npm
- SQL Server LocalDB or SQL Server
- VS Code, optional but recommended

The default development connection string uses LocalDB:

```text
Server=(localdb)\MSSQLLocalDB;Database=QuestECommerceDbChiriac;Trusted_Connection=True;TrustServerCertificate=True;
```

## First Step: Create The Database

Before running the app, run the combined database script:

```text
backend/database/000_CreateFullDatabase.sql
```

This creates:

- `Users`
- `Products`
- `ProductSizeStocks`
- `Orders`
- `OrderItems`

The app seeds demo users and demo products automatically when the backend starts in `Development`. Product images are committed under:

```text
backend/Ecommerce.Api/wwwroot/images/products/
```

Demo login accounts:

```text
demo@questshirts.local / Password123!
reviewer@questshirts.local / Password123!
```

## Run With VS Code

After running `000_CreateFullDatabase.sql`, open the repository in VS Code.

Use the Run and Debug panel and start:

```text
Full Stack: Backend + Frontend
```

This launches:

- Backend API: `https://localhost:7214`
- Scalar API UI: `https://localhost:7214/scalar`
- Angular frontend: `http://localhost:4200`

## Manual Run

If you are not using VS Code, run the backend and frontend manually.

Backend:

```bash
dotnet restore QuestAssignmentFullstack.slnx
dotnet run --project backend/Ecommerce.Api/Ecommerce.Api.csproj --launch-profile https
```

Frontend:

```bash
cd frontend
npm install
npm run start
```

Then open:

```text
http://localhost:4200
```

The API documentation is available at:

```text
https://localhost:7214/scalar
```

## What The App Includes

Required by the assignment:

- Register user
- Login user
- Product catalog fetched from the API
- Angular cart state managed locally with Signals
- Checkout form with shipping details
- Backend-calculated order totals
- ADO.NET repositories, no Entity Framework or ORM
- SQL Server database schema
- Dependency Injection in the backend

Added beyond the brief:

- JWT Bearer authentication
- Product details page
- Football shirt sizes: `XS`, `S`, `M`, `L`, `XL`, `XXL`
- Size-specific stock tracking
- Stock conflict handling with `409 Conflict`
- Transaction-safe checkout and inventory decrement
- Product front/back images served from `wwwroot`
- Profile page with user info, lifetime spend, and past orders
- Lazy expandable order details with cart-like item rows
- Scalar API UI for backend exploration

## Tests

Frontend tests can be run with:

```bash
cd frontend
npm test
```

The current frontend includes Angular spec files for app/auth flows. Backend unit tests will be documented here after the backend test project is added.

## Notes

- The cart is intentionally stored locally in Angular until checkout.
- The frontend does not send prices or totals during checkout.
- The backend calculates totals from database product prices.
- If stock changes during checkout, the API returns a structured `409 Conflict` response and the frontend adjusts the cart.
- Product creation/update/delete endpoints are intentionally not included; demo products are seeded for the assignment.
