namespace Northwind.Api.Dtos;

public class CustomerSummaryDto
{
    public string CustomerId { get; init; } = null!;

    public string CompanyName { get; init; } = null!;

    public int OrderCount { get; init; }
}
