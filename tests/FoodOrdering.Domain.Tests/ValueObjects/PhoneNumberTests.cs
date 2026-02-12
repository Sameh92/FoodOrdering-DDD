using FluentAssertions;
using FoodOrdering.Domain.Customers.ValueObjects;
using Xunit;

namespace FoodOrdering.Domain.Tests.ValueObjects;

public class PhoneNumberTests
{
    [Theory]
    [InlineData("+60123456789")]
    [InlineData("60123456789")]
    [InlineData("+1-555-123-4567")]
    [InlineData("(555) 123-4567")]
    public void Create_WithValidPhoneNumber_ShouldSucceed(string validPhone)
    {
        // Act
        var phone = new PhoneNumber(validPhone);

        // Assert
        phone.Value.Should().NotBeEmpty();
    }

    [Fact]
    public void Create_WithEmptyPhoneNumber_ShouldThrowException()
    {
        // Act
        var act = () => new PhoneNumber("");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*required*");
    }

    [Fact]
    public void Create_WithTooShortPhoneNumber_ShouldThrowException()
    {
        // Act
        var act = () => new PhoneNumber("1234567"); // 7 digits

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*length*");
    }

    [Fact]
    public void Create_WithTooLongPhoneNumber_ShouldThrowException()
    {
        // Act
        var act = () => new PhoneNumber("+1234567890123456"); // 16 digits

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*length*");
    }

    [Fact]
    public void Create_ShouldRemoveSpecialCharacters()
    {
        // Act
        var phone = new PhoneNumber("+60 (12) 345-6789");

        // Assert
        phone.Value.Should().Be("+60123456789");
    }

    [Fact]
    public void Create_ShouldKeepPlusSign()
    {
        // Act
        var phone = new PhoneNumber("+60123456789");

        // Assert
        phone.Value.Should().StartWith("+");
    }

    [Fact]
    public void Equality_SamePhoneNumber_ShouldBeEqual()
    {
        // Arrange
        var phone1 = new PhoneNumber("+60123456789");
        var phone2 = new PhoneNumber("+60123456789");

        // Assert
        phone1.Should().Be(phone2);
    }

    [Fact]
    public void Equality_SamePhoneNumberDifferentFormat_ShouldBeEqual()
    {
        // Arrange
        var phone1 = new PhoneNumber("+60123456789");
        var phone2 = new PhoneNumber("+60 123-456-789");

        // Assert
        phone1.Should().Be(phone2);
    }

    [Fact]
    public void ToString_ShouldReturnValue()
    {
        // Arrange
        var phone = new PhoneNumber("+60123456789");

        // Act
        var result = phone.ToString();

        // Assert
        result.Should().Be("+60123456789");
    }

    [Fact]
    public void ImplicitConversion_ShouldReturnValue()
    {
        // Arrange
        var phone = new PhoneNumber("+60123456789");

        // Act
        string value = phone;

        // Assert
        value.Should().Be("+60123456789");
    }
}