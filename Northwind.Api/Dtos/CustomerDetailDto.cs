namespace Northwind.Api.Dtos;

public class CustomerDetailDto
{
    public string CustomerId { get; init; } = null!;

    public string CompanyName { get; init; } = null!;

    public string? ContactName { get; init; }

    public string? City { get; init; }

    public string? Country { get; init; }

    public string? Phone { get; init; }

    public IReadOnlyList<OrderSummaryDto> Orders { get; init; } = new List<OrderSummaryDto>();
}
