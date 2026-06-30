# Northwind Customer Lookup

An internal tool for staff to look up customers and review their order history,
built on the Microsoft Northwind sample database.

- **Backend:** ASP.NET Core Web API (.NET 10), Entity Framework Core (SQL Server, database-first)
- **Frontend:** Vue.js (talks to the API over HTTP only)
- **Database:** SQL Server with the Northwind sample database (`db/instnwnd.sql`)

> 🚧 Work in progress. Full run instructions, assumptions, trade-offs, and AI prompts
> will be documented here as the solution is built out.

## Projects

| Project | Description |
|---|---|
| `Northwind.Api` | ASP.NET Core Web API exposing customer & order data |
| `Northwind.Tests` | Automated tests |
