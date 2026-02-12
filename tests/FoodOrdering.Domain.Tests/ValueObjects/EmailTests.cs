using FluentAssertions;
using FoodOrdering.Domain.Customers.ValueObjects;
using Xunit;

namespace FoodOrdering.Domain.Tests.ValueObjects;

public class EmailTests
{
    [Theory]
    [InlineData("test@example.com")]
    [InlineData("user.name@domain.com")]
    [InlineData("user+tag@example.org")]
    [InlineData("firstname.lastname@company.co.uk")]
    public void Create_WithValidEmail_ShouldSucceed(string validEmail)
    {
        // Act
        var email = new Email(validEmail);

        // Assert
        email.Value.Should().Be(validEmail.ToLowerInvariant());
    }

    [Fact]
    public void Create_WithEmptyEmail_ShouldThrowException()
    {
        // Act
        var act = () => new Email("");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*required*");
    }

    [Fact]
    public void Create_WithWhitespaceEmail_ShouldThrowException()
    {
        // Act
        var act = () => new Email("   ");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*required*");
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("invalid@")]
    [InlineData("@example.com")]
    [InlineData("invalid@.com")]
    [InlineData("invalid@com")]
    public void Create_WithInvalidEmail_ShouldThrowException(string invalidEmail)
    {
        // Act
        var act = () => new Email(invalidEmail);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Invalid email*");
    }

    [Fact]
    public void Create_ShouldNormalizeToLowercase()
    {
        // Act
        var email = new Email("Test@EXAMPLE.COM");

        // Assert
        email.Value.Should().Be("test@example.com");
    }

    [Fact]
    public void Create_ShouldTrimWhitespace()
    {
        // Act
        var email = new Email("  test@example.com  ");

        // Assert
        email.Value.Should().Be("test@example.com");
    }

    [Fact]
    public void Equality_SameEmail_ShouldBeEqual()
    {
        // Arrange
        var email1 = new Email("test@example.com");
        var email2 = new Email("test@example.com");

        // Assert
        email1.Should().Be(email2);
        (email1 == email2).Should().BeTrue();
    }

    [Fact]
    public void Equality_SameEmailDifferentCase_ShouldBeEqual()
    {
        // Arrange
        var email1 = new Email("test@example.com");
        var email2 = new Email("TEST@EXAMPLE.COM");

        // Assert
        email1.Should().Be(email2);
    }

    [Fact]
    public void Equality_DifferentEmail_ShouldNotBeEqual()
    {
        // Arrange
        var email1 = new Email("test1@example.com");
        var email2 = new Email("test2@example.com");

        // Assert
        email1.Should().NotBe(email2);
    }

    [Fact]
    public void ImplicitConversion_ShouldReturnValue()
    {
        // Arrange
        var email = new Email("test@example.com");

        // Act
        string value = email;

        // Assert
        value.Should().Be("test@example.com");
    }

    [Fact]
    public void ToString_ShouldReturnValue()
    {
        // Arrange
        var email = new Email("test@example.com");

        // Act
        var result = email.ToString();

        // Assert
        result.Should().Be("test@example.com");
    }
}