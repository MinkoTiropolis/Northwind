using Northwind.Api.Dtos;

namespace Northwind.Api.Services;

public interface ICustomerService
{
    Task<IReadOnlyList<CustomerSummaryDto>> GetCustomersAsync(string? search, CancellationToken cancellationToken = default);

    Task<CustomerDetailDto?> GetCustomerByIdAsync(string id, CancellationToken cancellationToken = default);
}
