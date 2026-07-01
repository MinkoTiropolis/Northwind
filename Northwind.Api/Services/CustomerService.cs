using Microsoft.EntityFrameworkCore;
using Northwind.Api.Data;
using Northwind.Api.Dtos;

namespace Northwind.Api.Services;

public class CustomerService : ICustomerService
{
    private readonly NorthwindContext _context;

    public CustomerService(NorthwindContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<CustomerSummaryDto>> GetCustomersAsync(
        string? search, CancellationToken cancellationToken = default)
    {
        var query = _context.Customers.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(c => EF.Functions.Like(c.CompanyName, $"%{term}%"));
        }

        return await query
            .OrderBy(c => c.CompanyName)
            .Select(c => new CustomerSummaryDto
            {
                CustomerId = c.CustomerId,
                CompanyName = c.CompanyName,
                OrderCount = c.Orders.Count
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<CustomerDetailDto?> GetCustomerByIdAsync(
        string id, CancellationToken cancellationToken = default)
    {
        // The Sum/Count aggregation runs in SQL. It pulls a lightweight result back,
        // then map to DTOs in memory so the money total can be rounded.
        var customer = await _context.Customers
            .AsNoTracking()
            .Where(c => c.CustomerId == id)
            .Select(c => new
            {
                c.CustomerId,
                c.CompanyName,
                c.ContactName,
                c.City,
                c.Country,
                c.Phone,
                Orders = c.Orders
                    .OrderByDescending(o => o.OrderDate)
                    .Select(o => new
                    {
                        o.OrderId,
                        o.OrderDate,
                        ProductCount = o.OrderDetails.Count,
                        TotalValue = o.OrderDetails.Sum(od =>
                            od.UnitPrice * od.Quantity * (1 - (decimal)od.Discount))
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (customer is null)
        {
            return null;
        }

        return new CustomerDetailDto
        {
            CustomerId = customer.CustomerId,
            CompanyName = customer.CompanyName,
            ContactName = customer.ContactName,
            City = customer.City,
            Country = customer.Country,
            Phone = customer.Phone,
            Orders = customer.Orders
                .Select(o => new OrderSummaryDto
                {
                    OrderId = o.OrderId,
                    OrderDate = o.OrderDate,
                    ProductCount = o.ProductCount,
                    TotalValue = Math.Round(o.TotalValue, 2)
                })
                .ToList()
        };
    }
}
