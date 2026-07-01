
# Northwind Customer Lookup

An internal tool that lets staff look up customers and review their order history, built on the Microsoft Northwind sample database.

It exposes a small, clean back-end API and a lightweight Vue front-end:

- **Customer overview** — list all customers with the number of orders each has placed, filterable by name.
- **Customer detail** — a customer's details plus a summary of every order, showing the order's **total value** (discount-aware) and the **number of products** it contains.

## Tech stack

| Layer | Technology |
|---|---|
| Back-end | ASP.NET Core Web API (.NET 10) |
| Data access | Entity Framework Core (SQL Server provider, database-first) |
| Database | SQL Server + the Northwind sample database (`db/instnwnd.sql`) |
| Tests | xUnit + EF Core InMemory provider |
| Front-end | Vue 3 + Vite + TypeScript |

## Solution structure

```
Northwind/
├── Northwind.Api/            ASP.NET Core Web API
│   ├── Controllers/          CustomersController (HTTP endpoints)
│   ├── Services/             CustomerService + ICustomerService (query logic)
│   ├── Dtos/                 API response shapes (kept separate from EF entities)
│   ├── Models/               EF entities scaffolded from the database
│   ├── Data/                 NorthwindContext (DbContext)
│   └── appsettings.json      Connection string lives here
├── Northwind.Tests/          xUnit tests for the service logic
├── frontend/                 Vue 3 + Vite + TypeScript client
│   └── src/
│       ├── api.ts            Typed HTTP client
│       └── components/       CustomerList.vue, CustomerDetail.vue
└── db/instnwnd.sql           Northwind database install script
```

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js](https://nodejs.org/) 20.19+ / 22.12+ (for the front-end)
- A local **SQL Server** instance (Express, Developer, or LocalDB)

## Getting started

### 1. Database

The `db/instnwnd.sql` script creates the Northwind **objects** (tables, data, views), but **not the database itself** — so create the `Northwind` database first, then run the script into it.

In SSMS: create a database named `Northwind`, open `db/instnwnd.sql` with that database selected, and execute.

### 2. Back-end API

The connection string lives in `Northwind.Api/appsettings.json` under `ConnectionStrings:Northwind`.
It uses **Windows authentication** (integrated security), so it contains no credentials:

```json
"ConnectionStrings": {
  "Northwind": "Server=localhost;Database=Northwind;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

Adjust the `Server=` value to match your instance (e.g. `(localdb)\MSSQLLocalDB` or `.\SQLEXPRESS`), then run:

```powershell
dotnet run --project Northwind.Api
```

The API starts on **http://localhost:5179**. 

> **Important — run the HTTP profile.** The front-end dev proxy targets `http://localhost:5179`.
> `dotnet run` uses the `http` launch profile by default. If you run from Visual Studio,  select the **http** profile — the `https` profile triggers an HTTPS redirect that the dev proxy won't follow.

### 3. Front-end

```powershell
cd frontend
npm install
npm run dev
```

Open **http://localhost:5173**. The Vite dev server proxies `/api` to the back-end, so the front-end only ever talks HTTP and never touches the database directly.

### 4. Tests

```powershell
dotnet test
```

## API endpoints

| Method | Route | Description |
|---|---|---|
| `GET` | `/api/customers` | All customers with their order counts. Optional `?search=` filters by company name. |
| `GET` | `/api/customers/{id}` | A customer's details and order history. Returns `404` if the id is unknown. |

Example:

```
GET /api/customers?search=alfreds
GET /api/customers/ALFKI
```

## Design decisions & trade-offs

- **Database-first EF scaffolding.** The model was reverse-engineered from Northwind, scaffolding  only the four tables this tool needs (`Customers`, `Orders`, `Order Details`, `Products`) to keep
  the model focused. Trade-off: it couples the code to the existing schema, which is appropriate for a read-only tool over a fixed database.
- **DTOs instead of returning EF entities.** Responses use dedicated DTOs, which keeps the API shape clean, avoids leaking database columns, prevents JSON reference cycles, and provides a natural home for computed values (`orderCount`, `totalValue`, `productCount`).
- **Order total is discount-aware.** Each line is `UnitPrice × Quantity × (1 - Discount)`. The order  total is the sum of its lines. The `Sum`/`Count` aggregation runs in **SQL**. The result is then
  mapped to DTOs in memory, where the total is rounded to 2 decimal places (SQL `ROUND` keeps the column scale, so rounding in C# gives a clean money value). Currency formatting is done in the UI,  not the API — the API returns a plain number.
- **Product count** is the number of order lines (one line = one product on the order).
- **Search uses `Contains`**, which EF translates to a parameterised `LIKE` with wildcards escaped  (so `%`/`_` are treated literally). Case sensitivity follows the database collation (case-insensitive by default).
- **No repository pattern.** `DbContext` already implements the unit-of-work/repository roles, so an
  extra layer would add complexity without value. Layering is deliberately minimal: controller → service → EF.
- **No secrets in the repo.** Windows authentication means the connection string carries no username/password. In production this would come from environment variables or a secret manager.
- **Tests focus on the non-trivial logic.** Three xUnit tests verify the discount-aware total, product  and order counts, ordering, and the not-found path — using EF Core's **InMemory** provider. This reflects "what's worth verifying" rather than chasing   full coverage.
- **Front-end kept intentionally small.** Two components, a typed API client, and a dev proxy — no router or state-management library, since the scope doesn't warrant them.

## What I would iomprove with more time

- Pagination / server-side paging for larger customer sets.
- Integration tests against a real SQL Server (query translation, the search filter end-to-end).
- Authentication/authorization for a real internal tool.
- A CI pipeline (build + test) and containerised setup.

## AI usage

I used an AI coding assistant (Claude Opus 4.8) as a pair programmer throughout this assessment. I directed the work, made the architecture and technology decisions, and reviewed, adjusted, and verified every change. AI accelerated scaffolding, boilerplate, styling, tests, and
documentation, and acted as a sounding board for design trade-offs. The judgement calls (keeping it simple, the layering, discount handling, what to test, what to leave out) were mine.

Representative prompts, in commit order:

**Database scaffold**
> Give me the EF Core database first scaffold command, keeping only the Customers, Orders, Order Details and Products tables from my local SQL server Northwind. Keep the structure of the project organised, by placing DbContext in a `Data` folder, entities in `Models`. Don't hardcode the connection string in the context.

**Customer endpoints**
> Build a clean ASP.NET Core Web API with two endpoints: `GET /api/customers` (name + order count, filterable by name) and `GET /api/customers/{id}` (details + each order's total value and product count). Use a service + DTOs (not EF entities). Make the order total apply the per-line Northwind discount.

**Money formatting**
> The order total is displayed as `933.500000`. I want a 2 decimal value in the response but keep the aggregation in SQL. What is the best approach?  Why doesnt SQL `ROUND` fix the trailing zeros?

**Search correctness**
> Is `EF.Functions.Like` with an interpolated `%term%` a problem for wildcard characters like `%`? Show a simpler, safer alternative.

**Tests**
> Write focused xUnit tests for the customer service: the discount aware total, product/order counts, ordering, and the not-found case. 

**Frontend**
> In the frontend folder, build a user friendly UI for the APIs. It must talk to  the API over HTTP only!

**Review & hardening**
> Review the current front-end and back-end for bugs — race conditions, edge cases. Tell me which are worth fixing.

**Documentation**
> Write a detailed README including how to run the database, API, frontend, tests. Include any other important detail related with the project.