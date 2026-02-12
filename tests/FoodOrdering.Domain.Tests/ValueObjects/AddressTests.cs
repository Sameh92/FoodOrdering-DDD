using FluentAssertions;
using FoodOrdering.Domain.Ordering.ValueObjects;
using Xunit;

namespace FoodOrdering.Domain.Tests.ValueObjects;

public class AddressTests
{
    [Fact]
    public void Create_WithValidData_ShouldSucceed()
    {
        // Act
        var address = new Address(
            "123 Main Street",
            "Kuala Lumpur",
            "Wilayah Persekutuan",
            "50000",
            "Malaysia"
        );

        // Assert
        address.Street.Should().Be("123 Main Street");
        address.City.Should().Be("Kuala Lumpur");
        address.State.Should().Be("Wilayah Persekutuan");
        address.PostalCode.Should().Be("50000");
        address.Country.Should().Be("Malaysia");
    }

    [Fact]
    public void Create_WithEmptyStreet_ShouldThrowException()
    {
        // Act
        var act = () => new Address("", "Kuala Lumpur", "WP", "50000", "Malaysia");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Street*");
    }

    [Fact]
    public void Create_WithEmptyCity_ShouldThrowException()
    {
        // Act
        var act = () => new Address("123 Main St", "", "WP", "50000", "Malaysia");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*City*");
    }

    [Fact]
    public void Create_WithEmptyPostalCode_ShouldThrowException()
    {
        // Act
        var act = () => new Address("123 Main St", "KL", "WP", "", "Malaysia");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Postal*");
    }

    [Fact]
    public void Create_WithEmptyCountry_ShouldThrowException()
    {
        // Act
        var act = () => new Address("123 Main St", "KL", "WP", "50000", "");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Country*");
    }

    [Fact]
    public void Create_ShouldTrimWhitespace()
    {
        // Act
        var address = new Address(
            "  123 Main Street  ",
            "  Kuala Lumpur  ",
            "  WP  ",
            "  50000  ",
            "  Malaysia  "
        );

        // Assert
        address.Street.Should().Be("123 Main Street");
        address.City.Should().Be("Kuala Lumpur");
        address.State.Should().Be("WP");
        address.PostalCode.Should().Be("50000");
        address.Country.Should().Be("Malaysia");
    }

    [Fact]
    public void Create_WithNullState_ShouldUseEmptyString()
    {
        // Act
        var address = new Address("123 Main St", "KL", null!, "50000", "Malaysia");

        // Assert
        address.State.Should().BeEmpty();
    }

    [Fact]
    public void GetFullAddress_ShouldReturnFormattedAddress()
    {
        // Arrange
        var address = new Address(
            "123 Main Street",
            "Kuala Lumpur",
            "Wilayah Persekutuan",
            "50000",
            "Malaysia"
        );

        // Act
        var result = address.GetFullAddress();

        // Assert
        result.Should().Be("123 Main Street, Kuala Lumpur, Wilayah Persekutuan, 50000, Malaysia");
    }

    [Fact]
    public void GetFullAddress_WithEmptyState_ShouldOmitState()
    {
        // Arrange
        var address = new Address("123 Main Street", "Kuala Lumpur", "", "50000", "Malaysia");

        // Act
        var result = address.GetFullAddress();

        // Assert
        result.Should().Be("123 Main Street, Kuala Lumpur, 50000, Malaysia");
    }

    [Fact]
    public void Equality_SameValues_ShouldBeEqual()
    {
        // Arrange
        var address1 = new Address("123 Main St", "KL", "WP", "50000", "Malaysia");
        var address2 = new Address("123 Main St", "KL", "WP", "50000", "Malaysia");

        // Assert
        address1.Should().Be(address2);
        (address1 == address2).Should().BeTrue();
    }

    [Fact]
    public void Equality_DifferentStreet_ShouldNotBeEqual()
    {
        // Arrange
        var address1 = new Address("123 Main St", "KL", "WP", "50000", "Malaysia");
        var address2 = new Address("456 Other St", "KL", "WP", "50000", "Malaysia");

        // Assert
        address1.Should().NotBe(address2);
    }

    [Fact]
    public void ToString_ShouldReturnFullAddress()
    {
        // Arrange
        var address = new Address("123 Main St", "KL", "WP", "50000", "Malaysia");

        // Act
        var result = address.ToString();

        // Assert
        result.Should().Be(address.GetFullAddress());
    }
}