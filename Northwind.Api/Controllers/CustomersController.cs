using Microsoft.AspNetCore.Mvc;
using Northwind.Api.Dtos;
using Northwind.Api.Services;

namespace Northwind.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomersController : ControllerBase
{
    private readonly ICustomerService _customerService;

    public CustomersController(ICustomerService customerService)
    {
        _customerService = customerService;
    }

    /// <summary>Get all customers (optionally filtered by name) with their order counts.</summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CustomerSummaryDto>>> GetCustomers(
        [FromQuery] string? search, CancellationToken cancellationToken)
    {
        var customers = await _customerService.GetCustomersAsync(search, cancellationToken);
        return Ok(customers);
    }

    /// <summary>Get a single customer's details and order history summary.</summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<CustomerDetailDto>> GetCustomer(
        string id, CancellationToken cancellationToken)
    {
        var customer = await _customerService.GetCustomerByIdAsync(id, cancellationToken);
        return customer is null ? NotFound() : Ok(customer);
    }
}
