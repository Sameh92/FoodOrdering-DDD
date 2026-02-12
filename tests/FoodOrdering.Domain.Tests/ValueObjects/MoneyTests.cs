using FluentAssertions;
using FoodOrdering.Domain.Ordering.ValueObjects;
using Xunit;

namespace FoodOrdering.Domain.Tests.ValueObjects;

public class MoneyTests
{
    [Fact]
    public void Create_WithValidAmount_ShouldSucceed()
    {
        // Act
        var money = new Money(10.50m, "MYR");

        // Assert
        money.Amount.Should().Be(10.50m);
        money.Currency.Should().Be("MYR");
    }

    [Fact]
    public void Create_WithNegativeAmount_ShouldThrowException()
    {
        // Act
        var act = () => new Money(-10m, "MYR");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*negative*");
    }

    [Fact]
    public void Create_WithEmptyCurrency_ShouldThrowException()
    {
        // Act
        var act = () => new Money(10m, "");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Currency*");
    }

    [Fact]
    public void Create_WithInvalidCurrencyLength_ShouldThrowException()
    {
        // Act
        var act = () => new Money(10m, "USDD");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*3-letter*");
    }

    [Fact]
    public void Create_ShouldNormalizeCurrencyToUppercase()
    {
        // Act
        var money = new Money(10m, "myr");

        // Assert
        money.Currency.Should().Be("MYR");
    }

    [Fact]
    public void Create_ShouldRoundToTwoDecimalPlaces()
    {
        // Act
        var money = new Money(10.555m, "MYR");

        // Assert
        money.Amount.Should().Be(10.56m);
    }

    [Fact]
    public void Add_WithSameCurrency_ShouldReturnSum()
    {
        // Arrange
        var money1 = new Money(10m, "MYR");
        var money2 = new Money(5.50m, "MYR");

        // Act
        var result = money1.Add(money2);

        // Assert
        result.Amount.Should().Be(15.50m);
        result.Currency.Should().Be("MYR");
    }

    [Fact]
    public void Add_WithDifferentCurrency_ShouldThrowException()
    {
        // Arrange
        var money1 = new Money(10m, "MYR");
        var money2 = new Money(5m, "USD");

        // Act
        var act = () => money1.Add(money2);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*different currencies*");
    }

    [Fact]
    public void Subtract_WithSameCurrency_ShouldReturnDifference()
    {
        // Arrange
        var money1 = new Money(10m, "MYR");
        var money2 = new Money(3m, "MYR");

        // Act
        var result = money1.Subtract(money2);

        // Assert
        result.Amount.Should().Be(7m);
    }

    [Fact]
    public void Subtract_ResultingInNegative_ShouldThrowException()
    {
        // Arrange
        var money1 = new Money(5m, "MYR");
        var money2 = new Money(10m, "MYR");

        // Act
        var act = () => money1.Subtract(money2);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*negative*");
    }

    [Fact]
    public void Multiply_WithPositiveFactor_ShouldReturnProduct()
    {
        // Arrange
        var money = new Money(10m, "MYR");

        // Act
        var result = money.Multiply(3);

        // Assert
        result.Amount.Should().Be(30m);
    }

    [Fact]
    public void Multiply_WithDecimalFactor_ShouldReturnProduct()
    {
        // Arrange
        var money = new Money(10m, "MYR");

        // Act
        var result = money.Multiply(1.5m);

        // Assert
        result.Amount.Should().Be(15m);
    }

    [Fact]
    public void Multiply_WithNegativeFactor_ShouldThrowException()
    {
        // Arrange
        var money = new Money(10m, "MYR");

        // Act
        var act = () => money.Multiply(-2);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*negative*");
    }

    [Fact]
    public void OperatorPlus_ShouldAddMoney()
    {
        // Arrange
        var money1 = new Money(10m, "MYR");
        var money2 = new Money(5m, "MYR");

        // Act
        var result = money1 + money2;

        // Assert
        result.Amount.Should().Be(15m);
    }

    [Fact]
    public void OperatorMinus_ShouldSubtractMoney()
    {
        // Arrange
        var money1 = new Money(10m, "MYR");
        var money2 = new Money(3m, "MYR");

        // Act
        var result = money1 - money2;

        // Assert
        result.Amount.Should().Be(7m);
    }

    [Fact]
    public void OperatorMultiply_ShouldMultiplyMoney()
    {
        // Arrange
        var money = new Money(10m, "MYR");

        // Act
        var result = money * 2;

        // Assert
        result.Amount.Should().Be(20m);
    }

    [Fact]
    public void Zero_ShouldReturnZeroAmount()
    {
        // Act
        var money = Money.Zero("MYR");

        // Assert
        money.Amount.Should().Be(0m);
        money.Currency.Should().Be("MYR");
    }

    [Fact]
    public void Equality_SameAmountAndCurrency_ShouldBeEqual()
    {
        // Arrange
        var money1 = new Money(10m, "MYR");
        var money2 = new Money(10m, "MYR");

        // Assert
        money1.Should().Be(money2);
        (money1 == money2).Should().BeTrue();
    }

    [Fact]
    public void Equality_DifferentAmount_ShouldNotBeEqual()
    {
        // Arrange
        var money1 = new Money(10m, "MYR");
        var money2 = new Money(20m, "MYR");

        // Assert
        money1.Should().NotBe(money2);
    }

    [Fact]
    public void ToString_ShouldReturnFormattedString()
    {
        // Arrange
        var money = new Money(10.50m, "MYR");

        // Act
        var result = money.ToString();

        // Assert
        result.Should().Be("MYR 10.50");
    }
}