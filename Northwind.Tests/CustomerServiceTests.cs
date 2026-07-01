using Microsoft.EntityFrameworkCore;
using Northwind.Api.Data;
using Northwind.Api.Models;
using Northwind.Api.Services;

namespace Northwind.Tests;

public class CustomerServiceTests
{
    // A fresh in-memory database per test keeps them isolated.
    // The InMemory provider is used because it evaluates the LINQ
    // in C#, so decimal aggregation behaves exactly like it does at runtime.
    private static NorthwindContext CreateContext() =>
        new(new DbContextOptionsBuilder<NorthwindContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options);

    private static Customer SeededCustomer() => new()
    {
        CustomerId = "TEST1",
        CompanyName = "Test Trading Co",
        ContactName = "Pat Tester",
        City = "London",
        Country = "UK",
        Phone = "0123",
        Orders =
        {
            new Order
            {
                OrderId = 10001,
                OrderDate = new DateTime(1998, 1, 1),
                OrderDetails =
                {
                    new OrderDetail { ProductId = 1, UnitPrice = 10.00m, Quantity = 2, Discount = 0f },      // => 20.00
                    new OrderDetail { ProductId = 2, UnitPrice = 20.00m, Quantity = 3, Discount = 0.25f },   // => 45.00
                },
            },
            new Order
            {
                OrderId = 10002,
                OrderDate = new DateTime(1998, 2, 1),
                OrderDetails =
                {
                    new OrderDetail { ProductId = 3, UnitPrice = 50.00m, Quantity = 1, Discount = 0.5f },    // => 25.00
                },
            },
        },
    };

    [Fact]
    public async Task GetCustomerByIdAsync_AppliesDiscount_AndCountsProducts()
    {
        using var context = CreateContext();
        context.Customers.Add(SeededCustomer());
        await context.SaveChangesAsync();
        var service = new CustomerService(context);

        var result = await service.GetCustomerByIdAsync("TEST1");

        Assert.NotNull(result);
        Assert.Equal("Test Trading Co", result!.CompanyName);
        Assert.Equal(2, result.Orders.Count);

        // Orders come back most-recent first.
        Assert.Equal(10002, result.Orders.First().OrderId);

        var order1 = result.Orders.Single(o => o.OrderId == 10001);
        Assert.Equal(65.00m, order1.TotalValue);   // 20.00 + 45.00 (25% off the second line)
        Assert.Equal(2, order1.ProductCount);

        var order2 = result.Orders.Single(o => o.OrderId == 10002);
        Assert.Equal(25.00m, order2.TotalValue);    // 50.00 with 50% discount
        Assert.Equal(1, order2.ProductCount);
    }

    [Fact]
    public async Task GetCustomerByIdAsync_ReturnsNull_WhenCustomerNotFound()
    {
        using var context = CreateContext();
        context.Customers.Add(SeededCustomer());
        await context.SaveChangesAsync();
        var service = new CustomerService(context);

        var result = await service.GetCustomerByIdAsync("NOPE");

        Assert.Null(result);
    }

    [Fact]
    public async Task GetCustomersAsync_ReturnsOrderCounts_OrderedByName()
    {
        using var context = CreateContext();
        context.Customers.AddRange(
            MakeCustomer("ZETA", "Zeta Foods", orderCount: 1, firstOrderId: 100),
            MakeCustomer("ALPHA", "Alpha Supplies", orderCount: 3, firstOrderId: 200),
            MakeCustomer("MIDCO", "Mid Traders", orderCount: 0, firstOrderId: 300));
        await context.SaveChangesAsync();
        var service = new CustomerService(context);

        var result = await service.GetCustomersAsync(search: null);

        Assert.Equal(3, result.Count);

        // Returned alphabetically by company name.
        Assert.Equal(
            new[] { "Alpha Supplies", "Mid Traders", "Zeta Foods" },
            result.Select(c => c.CompanyName).ToArray());

        Assert.Equal(3, result.Single(c => c.CustomerId == "ALPHA").OrderCount);
        Assert.Equal(1, result.Single(c => c.CustomerId == "ZETA").OrderCount);
        Assert.Equal(0, result.Single(c => c.CustomerId == "MIDCO").OrderCount);
    }

    private static Customer MakeCustomer(string id, string companyName, int orderCount, int firstOrderId)
    {
        var customer = new Customer { CustomerId = id, CompanyName = companyName };
        for (var i = 0; i < orderCount; i++)
        {
            customer.Orders.Add(new Order { OrderId = firstOrderId + i, OrderDate = new DateTime(1998, 1, 1) });
        }
        return customer;
    }
}
