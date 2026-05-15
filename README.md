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

## First Step: Restore Or Create The Database

The repository includes a populated SQL Server backup:

```text
backend/database/QuestECommerceDbChiriac.bak
```

Recommended reviewer setup:

1. Restore `QuestECommerceDbChiriac.bak` in SQL Server Management Studio.
2. Keep the restored database name as:

```text
QuestECommerceDbChiriac
```

If you prefer not to restore the backup, run the combined database script instead:

```text
backend/database/000_CreateFullDatabase.sql
```

The script creates:

- `Users`
- `Products`
- `ProductSizeStocks`
- `Orders`
- `OrderItems`

When the backend starts in `Development`, it also inserts demo users and demo products if they are missing. Product images are committed under:

```text
backend/Ecommerce.Api/wwwroot/images/products/
```

Demo login accounts:

```text
demo@questshirts.local / Password123!
reviewer@questshirts.local / Password123!
```

## Run With VS Code

After restoring the `.bak` file or running `000_CreateFullDatabase.sql`, open the repository in VS Code.

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

Backend tests can be run with:

```bash
dotnet test QuestAssignmentFullstack.slnx
```

Frontend tests can be run with:

```bash
cd frontend
npm test
```

The backend test project uses xUnit, Moq, and Shouldly. It focuses on service-layer business logic such as checkout totals, stock conflicts, auth behavior, and order mapping.

## Notes

- The cart is intentionally stored locally in Angular until checkout.
- The frontend does not send prices or totals during checkout.
- The backend calculates totals from database product prices.
- If stock changes during checkout, the API returns a structured `409 Conflict` response and the frontend adjusts the cart.
- Product creation/update/delete endpoints are intentionally not included; demo products are seeded for the assignment.
