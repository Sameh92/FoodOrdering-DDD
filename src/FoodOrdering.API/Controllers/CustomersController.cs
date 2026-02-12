using FoodOrdering.Domain.Customers.Aggregates.Customer;
using FoodOrdering.Domain.Customers.Repositories;
using FoodOrdering.Domain.Customers.ValueObjects;
using FoodOrdering.Domain.Ordering.ValueObjects;
using FoodOrdering.Application.Common;
using Microsoft.AspNetCore.Mvc;
using FoodOrdering.API.DTOs.CustomerDTOs;
using FoodOrdering.API.DTOs.OrdersDTOs;

namespace FoodOrdering.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomersController : ControllerBase
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CustomersController(ICustomerRepository customerRepository, IUnitOfWork unitOfWork)
    {
        _customerRepository = customerRepository;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Get a customer by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(CustomerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCustomer(Guid id)
    {
        var customerId = CustomerId.From(id);
        var customer = await _customerRepository.GetByIdAsync(customerId);

        if (customer == null)
            return NotFound(new { error = "Customer not found" });

        return Ok(CustomerDto.FromCustomer(customer));
    }

    /// <summary>
    /// Get a customer by email.
    /// </summary>
    [HttpGet("by-email/{email}")]
    [ProducesResponseType(typeof(CustomerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCustomerByEmail(string email)
    {
        try
        {
            var emailVO = new Email(email);
            var customer = await _customerRepository.GetByEmailAsync(emailVO);

            if (customer == null)
                return NotFound(new { error = "Customer not found" });

            return Ok(CustomerDto.FromCustomer(customer));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Create a new customer.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(CustomerDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateCustomer([FromBody] CreateCustomerRequest request)
    {
        try
        {
            // Check if email already exists
            var emailVO = new Email(request.Email);
            if (await _customerRepository.ExistsAsync(emailVO))
                return BadRequest(new { error = "Email already registered" });

            // Create value objects
            var name = new CustomerName(request.FirstName, request.LastName);
            var phone = new PhoneNumber(request.PhoneNumber);

            Address? defaultAddress = null;
            if (request.DefaultAddress != null)
            {
                defaultAddress = new Address(
                    request.DefaultAddress.Street,
                    request.DefaultAddress.City,
                    request.DefaultAddress.State,
                    request.DefaultAddress.PostalCode,
                    request.DefaultAddress.Country
                );
            }

            // Create customer
            var customer = Customer.Create(name, emailVO, phone, defaultAddress);

            await _customerRepository.AddAsync(customer);
            await _unitOfWork.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetCustomer),
                new { id = customer.Id.Value },
                CustomerDto.FromCustomer(customer)
            );
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Update customer contact info.
    /// </summary>
    [HttpPut("{id:guid}/contact")]
    [ProducesResponseType(typeof(CustomerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateContactInfo(Guid id, [FromBody] UpdateContactRequest request)
    {
        try
        {
            var customerId = CustomerId.From(id);
            var customer = await _customerRepository.GetByIdAsync(customerId);

            if (customer == null)
                return NotFound(new { error = "Customer not found" });

            var email = new Email(request.Email);
            var phone = new PhoneNumber(request.PhoneNumber);

            customer.UpdateContactInfo(email, phone);

            _customerRepository.Update(customer);
            await _unitOfWork.SaveChangesAsync();

            return Ok(CustomerDto.FromCustomer(customer));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Update customer name.
    /// </summary>
    [HttpPut("{id:guid}/name")]
    [ProducesResponseType(typeof(CustomerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateName(Guid id, [FromBody] UpdateNameRequest request)
    {
        try
        {
            var customerId = CustomerId.From(id);
            var customer = await _customerRepository.GetByIdAsync(customerId);

            if (customer == null)
                return NotFound(new { error = "Customer not found" });

            var name = new CustomerName(request.FirstName, request.LastName);
            customer.UpdateName(name);

            _customerRepository.Update(customer);
            await _unitOfWork.SaveChangesAsync();

            return Ok(CustomerDto.FromCustomer(customer));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Set customer's default delivery address.
    /// </summary>
    [HttpPut("{id:guid}/address")]
    [ProducesResponseType(typeof(CustomerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SetDefaultAddress(Guid id, [FromBody] DTOs.Common.AddressRequest request)
    {
        try
        {
            var customerId = CustomerId.From(id);
            var customer = await _customerRepository.GetByIdAsync(customerId);

            if (customer == null)
                return NotFound(new { error = "Customer not found" });

            var address = new Address(
                request.Street,
                request.City,
                request.State,
                request.PostalCode,
                request.Country
            );

            customer.SetDefaultDeliveryAddress(address);

            _customerRepository.Update(customer);
            await _unitOfWork.SaveChangesAsync();

            return Ok(CustomerDto.FromCustomer(customer));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
