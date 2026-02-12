using FoodOrdering.Domain.Customers.Aggregates.Customer;

namespace FoodOrdering.API.DTOs.CustomerDTOs;

// DTOs
public record CustomerDto
{
    public Guid Id { get; init; }
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string PhoneNumber { get; init; } = string.Empty;
    public CustomerAddressDto? DefaultAddress { get; init; }
    public string MembershipLevel { get; init; } = string.Empty;
    public int TotalOrders { get; init; }
    public DateTime CreatedAt { get; init; }

    public static CustomerDto FromCustomer(Customer customer)
    {
        return new CustomerDto
        {
            Id = customer.Id.Value,
            FirstName = customer.Name.FirstName,
            LastName = customer.Name.LastName,
            FullName = customer.Name.FullName,
            Email = customer.Email.Value,
            PhoneNumber = customer.Phone.Value,
            DefaultAddress = customer.DefaultDeliveryAddress != null
                ? new CustomerAddressDto
                {
                    Street = customer.DefaultDeliveryAddress.Street,
                    City = customer.DefaultDeliveryAddress.City,
                    State = customer.DefaultDeliveryAddress.State,
                    PostalCode = customer.DefaultDeliveryAddress.PostalCode,
                    Country = customer.DefaultDeliveryAddress.Country
                }
                : null,
            MembershipLevel = customer.MembershipLevel.ToString(),
            TotalOrders = customer.TotalOrders,
            CreatedAt = customer.CreatedAt
        };
    }
}
