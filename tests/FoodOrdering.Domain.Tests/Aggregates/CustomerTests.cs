using FluentAssertions;
using FoodOrdering.Domain.Customers.Aggregates.Customer;
using FoodOrdering.Domain.Customers.ValueObjects;
using FoodOrdering.Domain.Ordering.ValueObjects;
using Xunit;

namespace FoodOrdering.Domain.Tests.Aggregates;

public class CustomerTests
{
    private readonly CustomerName _validName = new("John", "Doe");
    private readonly Email _validEmail = new("john.doe@example.com");
    private readonly PhoneNumber _validPhone = new("+60123456789");
    private readonly Address _validAddress = new("123 Main St", "Kuala Lumpur", "WP", "50000", "Malaysia");

    #region Customer Creation Tests

    [Fact]
    public void Create_WithValidData_ShouldSucceed()
    {
        // Act
        var customer = Customer.Create(_validName, _validEmail, _validPhone);

        // Assert
        customer.Should().NotBeNull();
        customer.Id.Should().NotBeNull();
        customer.Name.Should().Be(_validName);
        customer.Email.Should().Be(_validEmail);
        customer.Phone.Should().Be(_validPhone);
        customer.MembershipLevel.Should().Be(MembershipLevel.Standard);
        customer.TotalOrders.Should().Be(0);
        customer.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Create_WithDefaultAddress_ShouldSetAddress()
    {
        // Act
        var customer = Customer.Create(_validName, _validEmail, _validPhone, _validAddress);

        // Assert
        customer.DefaultDeliveryAddress.Should().Be(_validAddress);
    }

    [Fact]
    public void Create_WithoutDefaultAddress_ShouldHaveNullAddress()
    {
        // Act
        var customer = Customer.Create(_validName, _validEmail, _validPhone);

        // Assert
        customer.DefaultDeliveryAddress.Should().BeNull();
    }

    #endregion

    #region Update Contact Info Tests

    [Fact]
    public void UpdateContactInfo_WithValidData_ShouldUpdateEmailAndPhone()
    {
        // Arrange
        var customer = CreateCustomer();
        var newEmail = new Email("new.email@example.com");
        var newPhone = new PhoneNumber("+60987654321");

        // Act
        customer.UpdateContactInfo(newEmail, newPhone);

        // Assert
        customer.Email.Should().Be(newEmail);
        customer.Phone.Should().Be(newPhone);
        customer.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void UpdateContactInfo_ShouldSetUpdatedAt()
    {
        // Arrange
        var customer = CreateCustomer();
        var newEmail = new Email("new.email@example.com");
        var newPhone = new PhoneNumber("+60987654321");

        // Act
        customer.UpdateContactInfo(newEmail, newPhone);

        // Assert
        customer.UpdatedAt.Should().NotBeNull();
        customer.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    #endregion

    #region Update Name Tests

    [Fact]
    public void UpdateName_WithValidName_ShouldUpdateName()
    {
        // Arrange
        var customer = CreateCustomer();
        var newName = new CustomerName("Jane", "Smith");

        // Act
        customer.UpdateName(newName);

        // Assert
        customer.Name.Should().Be(newName);
        customer.Name.FullName.Should().Be("Jane Smith");
    }

    [Fact]
    public void UpdateName_ShouldSetUpdatedAt()
    {
        // Arrange
        var customer = CreateCustomer();
        var newName = new CustomerName("Jane", "Smith");

        // Act
        customer.UpdateName(newName);

        // Assert
        customer.UpdatedAt.Should().NotBeNull();
    }

    #endregion

    #region Default Delivery Address Tests

    [Fact]
    public void SetDefaultDeliveryAddress_ShouldSetAddress()
    {
        // Arrange
        var customer = CreateCustomer();
        var newAddress = new Address("456 New St", "Petaling Jaya", "Selangor", "47800", "Malaysia");

        // Act
        customer.SetDefaultDeliveryAddress(newAddress);

        // Assert
        customer.DefaultDeliveryAddress.Should().Be(newAddress);
    }

    [Fact]
    public void SetDefaultDeliveryAddress_ShouldSetUpdatedAt()
    {
        // Arrange
        var customer = CreateCustomer();

        // Act
        customer.SetDefaultDeliveryAddress(_validAddress);

        // Assert
        customer.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void SetDefaultDeliveryAddress_ShouldOverwritePreviousAddress()
    {
        // Arrange
        var customer = Customer.Create(_validName, _validEmail, _validPhone, _validAddress);
        var newAddress = new Address("789 Another St", "Shah Alam", "Selangor", "40000", "Malaysia");

        // Act
        customer.SetDefaultDeliveryAddress(newAddress);

        // Assert
        customer.DefaultDeliveryAddress.Should().Be(newAddress);
        customer.DefaultDeliveryAddress!.Street.Should().Be("789 Another St");
    }

    #endregion

    #region Order Count and Membership Tests

    [Fact]
    public void IncrementOrderCount_ShouldIncreaseTotalOrders()
    {
        // Arrange
        var customer = CreateCustomer();

        // Act
        customer.IncrementOrderCount();

        // Assert
        customer.TotalOrders.Should().Be(1);
    }

    [Fact]
    public void IncrementOrderCount_MultipleTimes_ShouldAccumulate()
    {
        // Arrange
        var customer = CreateCustomer();

        // Act
        customer.IncrementOrderCount();
        customer.IncrementOrderCount();
        customer.IncrementOrderCount();

        // Assert
        customer.TotalOrders.Should().Be(3);
    }

    [Fact]
    public void IncrementOrderCount_ShouldSetUpdatedAt()
    {
        // Arrange
        var customer = CreateCustomer();

        // Act
        customer.IncrementOrderCount();

        // Assert
        customer.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void MembershipLevel_With0Orders_ShouldBeStandard()
    {
        // Arrange
        var customer = CreateCustomer();

        // Assert
        customer.MembershipLevel.Should().Be(MembershipLevel.Standard);
    }

    [Fact]
    public void MembershipLevel_With4Orders_ShouldBeStandard()
    {
        // Arrange
        var customer = CreateCustomer();
        IncrementOrders(customer, 4);

        // Assert
        customer.MembershipLevel.Should().Be(MembershipLevel.Standard);
    }

    [Fact]
    public void MembershipLevel_With5Orders_ShouldBeSilver()
    {
        // Arrange
        var customer = CreateCustomer();
        IncrementOrders(customer, 5);

        // Assert
        customer.MembershipLevel.Should().Be(MembershipLevel.Silver);
    }

    [Fact]
    public void MembershipLevel_With19Orders_ShouldBeSilver()
    {
        // Arrange
        var customer = CreateCustomer();
        IncrementOrders(customer, 19);

        // Assert
        customer.MembershipLevel.Should().Be(MembershipLevel.Silver);
    }

    [Fact]
    public void MembershipLevel_With20Orders_ShouldBeGold()
    {
        // Arrange
        var customer = CreateCustomer();
        IncrementOrders(customer, 20);

        // Assert
        customer.MembershipLevel.Should().Be(MembershipLevel.Gold);
    }

    [Fact]
    public void MembershipLevel_With49Orders_ShouldBeGold()
    {
        // Arrange
        var customer = CreateCustomer();
        IncrementOrders(customer, 49);

        // Assert
        customer.MembershipLevel.Should().Be(MembershipLevel.Gold);
    }

    [Fact]
    public void MembershipLevel_With50Orders_ShouldBePremium()
    {
        // Arrange
        var customer = CreateCustomer();
        IncrementOrders(customer, 50);

        // Assert
        customer.MembershipLevel.Should().Be(MembershipLevel.Premium);
    }

    [Fact]
    public void MembershipLevel_With100Orders_ShouldBePremium()
    {
        // Arrange
        var customer = CreateCustomer();
        IncrementOrders(customer, 100);

        // Assert
        customer.MembershipLevel.Should().Be(MembershipLevel.Premium);
    }

    #endregion

    #region IsPremiumMember Tests

    [Fact]
    public void IsPremiumMember_StandardMember_ShouldReturnFalse()
    {
        // Arrange
        var customer = CreateCustomer();

        // Assert
        customer.IsPremiumMember().Should().BeFalse();
    }

    [Fact]
    public void IsPremiumMember_SilverMember_ShouldReturnFalse()
    {
        // Arrange
        var customer = CreateCustomer();
        IncrementOrders(customer, 10);

        // Assert
        customer.IsPremiumMember().Should().BeFalse();
    }

    [Fact]
    public void IsPremiumMember_GoldMember_ShouldReturnFalse()
    {
        // Arrange
        var customer = CreateCustomer();
        IncrementOrders(customer, 30);

        // Assert
        customer.IsPremiumMember().Should().BeFalse();
    }

    [Fact]
    public void IsPremiumMember_PremiumMember_ShouldReturnTrue()
    {
        // Arrange
        var customer = CreateCustomer();
        IncrementOrders(customer, 50);

        // Assert
        customer.IsPremiumMember().Should().BeTrue();
    }

    #endregion

    #region CustomerName Value Object Tests

    [Fact]
    public void CustomerName_Create_WithValidData_ShouldSucceed()
    {
        // Act
        var name = new CustomerName("John", "Doe");

        // Assert
        name.FirstName.Should().Be("John");
        name.LastName.Should().Be("Doe");
        name.FullName.Should().Be("John Doe");
    }

    [Fact]
    public void CustomerName_Create_WithEmptyFirstName_ShouldThrowException()
    {
        // Act
        var act = () => new CustomerName("", "Doe");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*First name*required*");
    }

    [Fact]
    public void CustomerName_Create_WithEmptyLastName_ShouldThrowException()
    {
        // Act
        var act = () => new CustomerName("John", "");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Last name*required*");
    }

    [Fact]
    public void CustomerName_Create_ShouldTrimWhitespace()
    {
        // Act
        var name = new CustomerName("  John  ", "  Doe  ");

        // Assert
        name.FirstName.Should().Be("John");
        name.LastName.Should().Be("Doe");
    }

    [Fact]
    public void CustomerName_Equality_SameNames_ShouldBeEqual()
    {
        // Arrange
        var name1 = new CustomerName("John", "Doe");
        var name2 = new CustomerName("John", "Doe");

        // Assert
        name1.Should().Be(name2);
    }

    [Fact]
    public void CustomerName_Equality_DifferentNames_ShouldNotBeEqual()
    {
        // Arrange
        var name1 = new CustomerName("John", "Doe");
        var name2 = new CustomerName("Jane", "Doe");

        // Assert
        name1.Should().NotBe(name2);
    }

    [Fact]
    public void CustomerName_ToString_ShouldReturnFullName()
    {
        // Arrange
        var name = new CustomerName("John", "Doe");

        // Act
        var result = name.ToString();

        // Assert
        result.Should().Be("John Doe");
    }

    #endregion

    #region CustomerId Tests

    [Fact]
    public void CustomerId_New_ShouldGenerateUniqueId()
    {
        // Act
        var id1 = CustomerId.New();
        var id2 = CustomerId.New();

        // Assert
        id1.Should().NotBe(id2);
    }

    [Fact]
    public void CustomerId_From_WithValidGuid_ShouldSucceed()
    {
        // Arrange
        var guid = Guid.NewGuid();

        // Act
        var id = CustomerId.From(guid);

        // Assert
        id.Value.Should().Be(guid);
    }

    [Fact]
    public void CustomerId_From_WithEmptyGuid_ShouldThrowException()
    {
        // Act
        var act = () => CustomerId.From(Guid.Empty);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*empty*");
    }

    [Fact]
    public void CustomerId_FromString_WithValidString_ShouldSucceed()
    {
        // Arrange
        var guid = Guid.NewGuid();

        // Act
        var id = CustomerId.From(guid.ToString());

        // Assert
        id.Value.Should().Be(guid);
    }

    [Fact]
    public void CustomerId_Equality_SameValue_ShouldBeEqual()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var id1 = CustomerId.From(guid);
        var id2 = CustomerId.From(guid);

        // Assert
        id1.Should().Be(id2);
    }

    [Fact]
    public void CustomerId_ImplicitConversion_ShouldReturnGuid()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var id = CustomerId.From(guid);

        // Act
        Guid result = id;

        // Assert
        result.Should().Be(guid);
    }

    #endregion

    #region Helper Methods

    private Customer CreateCustomer()
    {
        return Customer.Create(_validName, _validEmail, _validPhone);
    }

    private void IncrementOrders(Customer customer, int count)
    {
        for (int i = 0; i < count; i++)
        {
            customer.IncrementOrderCount();
        }
    }

    #endregion
}