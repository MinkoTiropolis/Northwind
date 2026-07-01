namespace Northwind.Api.Dtos;

public class OrderSummaryDto
{
    public int OrderId { get; init; }

    public DateTime? OrderDate { get; init; }

    // Total value of the order: sum of UnitPrice * Quantity * (1 - Discount) across all lines
    public decimal TotalValue { get; init; }

    public int ProductCount { get; init; }
}
