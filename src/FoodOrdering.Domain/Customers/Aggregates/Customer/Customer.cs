using FoodOrdering.Domain.Common;
using FoodOrdering.Domain.Customers.ValueObjects;
using FoodOrdering.Domain.Ordering.ValueObjects;

namespace FoodOrdering.Domain.Customers.Aggregates.Customer;

/// <summary>
/// Customer Aggregate Root.
/// </summary>
public class Customer : AggregateRoot<CustomerId>
{
    public CustomerName Name { get; private set; } = null!;
    public Email Email { get; private set; } = null!;
    public PhoneNumber Phone { get; private set; } = null!;
    public Address? DefaultDeliveryAddress { get; private set; }
    public MembershipLevel MembershipLevel { get; private set; }
    public int TotalOrders { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    // Factory method
    public static Customer Create(
        CustomerName name,
        Email email,
        PhoneNumber phone,
        Address? defaultAddress = null)
    {
        return new Customer
        {
            Id = CustomerId.New(),
            Name = name,
            Email = email,
            Phone = phone,
            DefaultDeliveryAddress = defaultAddress,
            MembershipLevel = MembershipLevel.Standard,
            TotalOrders = 0,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void UpdateContactInfo(Email email, PhoneNumber phone)
    {
        Email = email;
        Phone = phone;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateName(CustomerName name)
    {
        Name = name;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetDefaultDeliveryAddress(Address address)
    {
        DefaultDeliveryAddress = address;
        UpdatedAt = DateTime.UtcNow;
    }

    public void IncrementOrderCount()
    {
        TotalOrders++;
        UpdateMembershipLevel();
        UpdatedAt = DateTime.UtcNow;
    }

    private void UpdateMembershipLevel()
    {
        MembershipLevel = TotalOrders switch
        {
            >= 50 => MembershipLevel.Premium,
            >= 20 => MembershipLevel.Gold,
            >= 5 => MembershipLevel.Silver,
            _ => MembershipLevel.Standard
        };
    }

    public bool IsPremiumMember() => MembershipLevel == MembershipLevel.Premium;

    // Private constructor for EF Core
    private Customer() { }
}