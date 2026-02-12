using FluentAssertions;
using FoodOrdering.Domain.Common;
using FoodOrdering.Domain.Customers.Aggregates.Customer;
using FoodOrdering.Domain.Ordering.Aggregates.Order;
using FoodOrdering.Domain.Ordering.Events;
using FoodOrdering.Domain.Ordering.ValueObjects;
using FoodOrdering.Domain.Restaurants.Aggregates.Restaurant;
using FoodOrdering.Domain.Restaurants.ValueObjects;
using Xunit;

namespace FoodOrdering.Domain.Tests.Aggregates;

public class OrderTests
{
    private readonly CustomerId _customerId = CustomerId.New();
    private readonly RestaurantId _restaurantId = RestaurantId.New();
    private readonly Address _deliveryAddress = new("123 Main St", "Kuala Lumpur", "WP", "50000", "Malaysia");
    private readonly MenuItemId _menuItemId = MenuItemId.New();
    private readonly Money _itemPrice = new(15.90m, "MYR");

    #region Order Creation Tests

    [Fact]
    public void Create_WithValidData_ShouldSucceed()
    {
        // Act
        var order = Order.Create(_customerId, _restaurantId, _deliveryAddress);

        // Assert
        order.Should().NotBeNull();
        order.Id.Should().NotBeNull();
        order.CustomerId.Should().Be(_customerId);
        order.RestaurantId.Should().Be(_restaurantId);
        order.DeliveryAddress.Should().Be(_deliveryAddress);
        order.Status.Should().Be(OrderStatus.Draft);
        order.Items.Should().BeEmpty();
        order.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Create_WithSpecialInstructions_ShouldSetInstructions()
    {
        // Act
        var order = Order.Create(_customerId, _restaurantId, _deliveryAddress, "Ring doorbell twice");

        // Assert
        order.SpecialInstructions.Should().Be("Ring doorbell twice");
    }

    [Fact]
    public void Create_WithNullCustomerId_ShouldThrowException()
    {
        // Act
        var act = () => Order.Create(null!, _restaurantId, _deliveryAddress);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Create_WithNullRestaurantId_ShouldThrowException()
    {
        // Act
        var act = () => Order.Create(_customerId, null!, _deliveryAddress);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Create_WithNullAddress_ShouldThrowException()
    {
        // Act
        var act = () => Order.Create(_customerId, _restaurantId, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    #endregion

    #region Add Item Tests

    [Fact]
    public void AddItem_ToDraftOrder_ShouldAddItem()
    {
        // Arrange
        var order = CreateDraftOrder();

        // Act
        order.AddItem(_menuItemId, "Classic Burger", _itemPrice, 2);

        // Assert
        order.Items.Should().HaveCount(1);
        order.Items[0].MenuItemId.Should().Be(_menuItemId);
        order.Items[0].Name.Should().Be("Classic Burger");
        order.Items[0].Price.Should().Be(_itemPrice);
        order.Items[0].Quantity.Should().Be(2);
    }

    [Fact]
    public void AddItem_WithSpecialInstructions_ShouldSetInstructions()
    {
        // Arrange
        var order = CreateDraftOrder();

        // Act
        order.AddItem(_menuItemId, "Classic Burger", _itemPrice, 1, "No onions please");

        // Assert
        order.Items[0].SpecialInstructions.Should().Be("No onions please");
    }

    [Fact]
    public void AddItem_SameItemTwice_ShouldIncreaseQuantity()
    {
        // Arrange
        var order = CreateDraftOrder();
        order.AddItem(_menuItemId, "Classic Burger", _itemPrice, 2);

        // Act
        order.AddItem(_menuItemId, "Classic Burger", _itemPrice, 3);

        // Assert
        order.Items.Should().HaveCount(1);
        order.Items[0].Quantity.Should().Be(5);
    }

    [Fact]
    public void AddItem_DifferentItems_ShouldAddBoth()
    {
        // Arrange
        var order = CreateDraftOrder();
        var menuItemId2 = MenuItemId.New();

        // Act
        order.AddItem(_menuItemId, "Classic Burger", _itemPrice, 1);
        order.AddItem(menuItemId2, "Cheese Burger", new Money(17.90m, "MYR"), 1);

        // Assert
        order.Items.Should().HaveCount(2);
    }

    [Fact]
    public void AddItem_WithZeroQuantity_ShouldThrowException()
    {
        // Arrange
        var order = CreateDraftOrder();

        // Act
        var act = () => order.AddItem(_menuItemId, "Classic Burger", _itemPrice, 0);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Quantity*positive*");
    }

    [Fact]
    public void AddItem_WithNegativeQuantity_ShouldThrowException()
    {
        // Arrange
        var order = CreateDraftOrder();

        // Act
        var act = () => order.AddItem(_menuItemId, "Classic Burger", _itemPrice, -1);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Quantity*positive*");
    }

    [Fact]
    public void AddItem_ToPlacedOrder_ShouldThrowException()
    {
        // Arrange
        var order = CreatePlacedOrder();

        // Act
        var act = () => order.AddItem(MenuItemId.New(), "French Fries", new Money(6.90m, "MYR"), 1);

        // Assert
        act.Should().Throw<InvalidStateTransitionException>();
    }

    #endregion

    #region Remove Item Tests

    [Fact]
    public void RemoveItem_ExistingItem_ShouldRemoveItem()
    {
        // Arrange
        var order = CreateDraftOrder();
        order.AddItem(_menuItemId, "Classic Burger", _itemPrice, 2);

        // Act
        order.RemoveItem(_menuItemId);

        // Assert
        order.Items.Should().BeEmpty();
    }

    [Fact]
    public void RemoveItem_NonExistingItem_ShouldThrowException()
    {
        // Arrange
        var order = CreateDraftOrder();

        // Act
        var act = () => order.RemoveItem(_menuItemId);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*not found*");
    }

    [Fact]
    public void RemoveItem_FromPlacedOrder_ShouldThrowException()
    {
        // Arrange
        var order = CreatePlacedOrder();

        // Act
        var act = () => order.RemoveItem(_menuItemId);

        // Assert
        act.Should().Throw<InvalidStateTransitionException>();
    }

    #endregion

    #region Update Item Quantity Tests

    [Fact]
    public void UpdateItemQuantity_ToPositiveValue_ShouldUpdateQuantity()
    {
        // Arrange
        var order = CreateDraftOrder();
        order.AddItem(_menuItemId, "Classic Burger", _itemPrice, 2);

        // Act
        order.UpdateItemQuantity(_menuItemId, 5);

        // Assert
        order.Items[0].Quantity.Should().Be(5);
    }

    [Fact]
    public void UpdateItemQuantity_ToZero_ShouldRemoveItem()
    {
        // Arrange
        var order = CreateDraftOrder();
        order.AddItem(_menuItemId, "Classic Burger", _itemPrice, 2);

        // Act
        order.UpdateItemQuantity(_menuItemId, 0);

        // Assert
        order.Items.Should().BeEmpty();
    }

    [Fact]
    public void UpdateItemQuantity_NonExistingItem_ShouldThrowException()
    {
        // Arrange
        var order = CreateDraftOrder();

        // Act
        var act = () => order.UpdateItemQuantity(_menuItemId, 5);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*not found*");
    }

    #endregion

    #region Place Order Tests

    [Fact]
    public void Place_WithItems_ShouldChangeStatusToPendingConfirmation()
    {
        // Arrange
        var order = CreateDraftOrder();
        order.AddItem(_menuItemId, "Classic Burger", _itemPrice, 1);

        // Act
        order.Place();

        // Assert
        order.Status.Should().Be(OrderStatus.PendingConfirmation);
        order.PlacedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Place_ShouldRaiseOrderPlacedEvent()
    {
        // Arrange
        var order = CreateDraftOrder();
        order.AddItem(_menuItemId, "Classic Burger", _itemPrice, 1);

        // Act
        order.Place();

        // Assert
        order.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<OrderPlacedEvent>();

        var domainEvent = order.DomainEvents[0] as OrderPlacedEvent;
        domainEvent!.OrderId.Should().Be(order.Id);
        domainEvent.CustomerId.Should().Be(_customerId);
        domainEvent.RestaurantId.Should().Be(_restaurantId);
    }

    [Fact]
    public void Place_WithNoItems_ShouldThrowException()
    {
        // Arrange
        var order = CreateDraftOrder();

        // Act
        var act = () => order.Place();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*empty*");
    }

    [Fact]
    public void Place_AlreadyPlacedOrder_ShouldThrowException()
    {
        // Arrange
        var order = CreatePlacedOrder();

        // Act
        var act = () => order.Place();

        // Assert
        act.Should().Throw<InvalidStateTransitionException>();
    }

    #endregion

    #region Confirm Order Tests

    [Fact]
    public void Confirm_PendingOrder_ShouldChangeStatusToConfirmed()
    {
        // Arrange
        var order = CreatePlacedOrder();
        var estimatedReadyTime = DateTime.UtcNow.AddMinutes(20);

        // Act
        order.Confirm(estimatedReadyTime);

        // Assert
        order.Status.Should().Be(OrderStatus.Confirmed);
        order.ConfirmedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        order.EstimatedReadyTime.Should().Be(estimatedReadyTime);
    }

    [Fact]
    public void Confirm_ShouldRaiseOrderConfirmedEvent()
    {
        // Arrange
        var order = CreatePlacedOrder();
        order.ClearDomainEvents(); // Clear previous events
        var estimatedReadyTime = DateTime.UtcNow.AddMinutes(20);

        // Act
        order.Confirm(estimatedReadyTime);

        // Assert
        order.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<OrderConfirmedEvent>();
    }

    [Fact]
    public void Confirm_DraftOrder_ShouldThrowException()
    {
        // Arrange
        var order = CreateDraftOrder();
        order.AddItem(_menuItemId, "Classic Burger", _itemPrice, 1);

        // Act
        var act = () => order.Confirm(DateTime.UtcNow.AddMinutes(20));

        // Assert
        act.Should().Throw<InvalidStateTransitionException>();
    }

    [Fact]
    public void Confirm_AlreadyConfirmedOrder_ShouldThrowException()
    {
        // Arrange
        var order = CreateConfirmedOrder();

        // Act
        var act = () => order.Confirm(DateTime.UtcNow.AddMinutes(20));

        // Assert
        act.Should().Throw<InvalidStateTransitionException>();
    }

    #endregion

    #region Order Lifecycle Tests

    [Fact]
    public void StartPreparing_ConfirmedOrder_ShouldChangeStatusToPreparing()
    {
        // Arrange
        var order = CreateConfirmedOrder();

        // Act
        order.StartPreparing();

        // Assert
        order.Status.Should().Be(OrderStatus.Preparing);
    }

    [Fact]
    public void StartPreparing_NonConfirmedOrder_ShouldThrowException()
    {
        // Arrange
        var order = CreatePlacedOrder();

        // Act
        var act = () => order.StartPreparing();

        // Assert
        act.Should().Throw<InvalidStateTransitionException>();
    }

    [Fact]
    public void MarkReadyForPickup_PreparingOrder_ShouldChangeStatusToReadyForPickup()
    {
        // Arrange
        var order = CreatePreparingOrder();

        // Act
        order.MarkReadyForPickup();

        // Assert
        order.Status.Should().Be(OrderStatus.ReadyForPickup);
    }

    [Fact]
    public void PickUp_ReadyForPickupOrder_ShouldChangeStatusToOutForDelivery()
    {
        // Arrange
        var order = CreateReadyForPickupOrder();

        // Act
        order.PickUp();

        // Assert
        order.Status.Should().Be(OrderStatus.OutForDelivery);
        order.PickedUpAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Deliver_OutForDeliveryOrder_ShouldChangeStatusToDelivered()
    {
        // Arrange
        var order = CreateOutForDeliveryOrder();

        // Act
        order.Deliver();

        // Assert
        order.Status.Should().Be(OrderStatus.Delivered);
        order.DeliveredAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Deliver_ShouldRaiseOrderDeliveredEvent()
    {
        // Arrange
        var order = CreateOutForDeliveryOrder();
        order.ClearDomainEvents();

        // Act
        order.Deliver();

        // Assert
        order.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<OrderDeliveredEvent>();
    }

    #endregion

    #region Cancel Order Tests

    [Fact]
    public void Cancel_DraftOrder_ShouldChangeStatusToCancelled()
    {
        // Arrange
        var order = CreateDraftOrder();
        order.AddItem(_menuItemId, "Classic Burger", _itemPrice, 1);

        // Act
        order.Cancel("Changed my mind");

        // Assert
        order.Status.Should().Be(OrderStatus.Cancelled);
        order.CancelledAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        order.CancellationReason.Should().Be("Changed my mind");
    }

    [Fact]
    public void Cancel_PendingOrder_ShouldChangeStatusToCancelled()
    {
        // Arrange
        var order = CreatePlacedOrder();

        // Act
        order.Cancel("Restaurant too busy");

        // Assert
        order.Status.Should().Be(OrderStatus.Cancelled);
    }

    [Fact]
    public void Cancel_ConfirmedOrder_ShouldChangeStatusToCancelled()
    {
        // Arrange
        var order = CreateConfirmedOrder();

        // Act
        order.Cancel("Customer requested cancellation");

        // Assert
        order.Status.Should().Be(OrderStatus.Cancelled);
    }

    [Fact]
    public void Cancel_ShouldRaiseOrderCancelledEvent()
    {
        // Arrange
        var order = CreatePlacedOrder();
        order.ClearDomainEvents();

        // Act
        order.Cancel("Test reason");

        // Assert
        order.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<OrderCancelledEvent>();

        var domainEvent = order.DomainEvents[0] as OrderCancelledEvent;
        domainEvent!.Reason.Should().Be("Test reason");
    }

    [Fact]
    public void Cancel_OutForDeliveryOrder_ShouldThrowException()
    {
        // Arrange
        var order = CreateOutForDeliveryOrder();

        // Act
        var act = () => order.Cancel("Too late to cancel");

        // Assert
        act.Should().Throw<InvalidStateTransitionException>();
    }

    [Fact]
    public void Cancel_DeliveredOrder_ShouldThrowException()
    {
        // Arrange
        var order = CreateDeliveredOrder();

        // Act
        var act = () => order.Cancel("Already delivered");

        // Assert
        act.Should().Throw<InvalidStateTransitionException>();
    }

    [Fact]
    public void Cancel_WithEmptyReason_ShouldThrowException()
    {
        // Arrange
        var order = CreatePlacedOrder();

        // Act
        var act = () => order.Cancel("");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*reason*required*");
    }

    #endregion

    #region Calculation Tests

    [Fact]
    public void GetSubtotal_WithMultipleItems_ShouldReturnCorrectSum()
    {
        // Arrange
        var order = CreateDraftOrder();
        order.AddItem(_menuItemId, "Classic Burger", new Money(15.90m, "MYR"), 2);
        order.AddItem(MenuItemId.New(), "French Fries", new Money(6.90m, "MYR"), 1);

        // Act
        var subtotal = order.GetSubtotal();

        // Assert
        subtotal.Amount.Should().Be(38.70m); // (15.90 * 2) + 6.90
        subtotal.Currency.Should().Be("MYR");
    }

    [Fact]
    public void GetSubtotal_WithNoItems_ShouldReturnZero()
    {
        // Arrange
        var order = CreateDraftOrder();

        // Act
        var subtotal = order.GetSubtotal();

        // Assert
        subtotal.Amount.Should().Be(0m);
    }

    [Fact]
    public void GetTotal_WithDeliveryFee_ShouldIncludeDeliveryFee()
    {
        // Arrange
        var order = CreateDraftOrder();
        order.AddItem(_menuItemId, "Classic Burger", new Money(15.90m, "MYR"), 2);
        order.SetDeliveryFee(new Money(5.00m, "MYR"));

        // Act
        var total = order.GetTotal();

        // Assert
        total.Amount.Should().Be(36.80m); // (15.90 * 2) + 5.00
    }

    [Fact]
    public void GetTotal_WithoutDeliveryFee_ShouldEqualSubtotal()
    {
        // Arrange
        var order = CreateDraftOrder();
        order.AddItem(_menuItemId, "Classic Burger", new Money(15.90m, "MYR"), 2);

        // Act
        var total = order.GetTotal();

        // Assert
        total.Amount.Should().Be(31.80m);
    }

    [Fact]
    public void GetItemCount_ShouldReturnTotalQuantity()
    {
        // Arrange
        var order = CreateDraftOrder();
        order.AddItem(_menuItemId, "Classic Burger", _itemPrice, 2);
        order.AddItem(MenuItemId.New(), "French Fries", new Money(6.90m, "MYR"), 3);

        // Act
        var count = order.GetItemCount();

        // Assert
        count.Should().Be(5);
    }

    #endregion

    #region Query Tests

    [Fact]
    public void CanBeCancelled_DraftOrder_ShouldReturnTrue()
    {
        // Arrange
        var order = CreateDraftOrder();

        // Assert
        order.CanBeCancelled().Should().BeTrue();
    }

    [Fact]
    public void CanBeCancelled_DeliveredOrder_ShouldReturnFalse()
    {
        // Arrange
        var order = CreateDeliveredOrder();

        // Assert
        order.CanBeCancelled().Should().BeFalse();
    }

    [Fact]
    public void CanBeCancelled_OutForDeliveryOrder_ShouldReturnFalse()
    {
        // Arrange
        var order = CreateOutForDeliveryOrder();

        // Assert
        order.CanBeCancelled().Should().BeFalse();
    }

    [Fact]
    public void CanBeModified_DraftOrder_ShouldReturnTrue()
    {
        // Arrange
        var order = CreateDraftOrder();

        // Assert
        order.CanBeModified().Should().BeTrue();
    }

    [Fact]
    public void CanBeModified_PlacedOrder_ShouldReturnFalse()
    {
        // Arrange
        var order = CreatePlacedOrder();

        // Assert
        order.CanBeModified().Should().BeFalse();
    }

    [Fact]
    public void IsFinal_DeliveredOrder_ShouldReturnTrue()
    {
        // Arrange
        var order = CreateDeliveredOrder();

        // Assert
        order.IsFinal().Should().BeTrue();
    }

    [Fact]
    public void IsFinal_CancelledOrder_ShouldReturnTrue()
    {
        // Arrange
        var order = CreatePlacedOrder();
        order.Cancel("Test");

        // Assert
        order.IsFinal().Should().BeTrue();
    }

    [Fact]
    public void IsFinal_ConfirmedOrder_ShouldReturnFalse()
    {
        // Arrange
        var order = CreateConfirmedOrder();

        // Assert
        order.IsFinal().Should().BeFalse();
    }

    [Fact]
    public void ContainsItem_ExistingItem_ShouldReturnTrue()
    {
        // Arrange
        var order = CreateDraftOrder();
        order.AddItem(_menuItemId, "Classic Burger", _itemPrice, 1);

        // Assert
        order.ContainsItem(_menuItemId).Should().BeTrue();
    }

    [Fact]
    public void ContainsItem_NonExistingItem_ShouldReturnFalse()
    {
        // Arrange
        var order = CreateDraftOrder();

        // Assert
        order.ContainsItem(_menuItemId).Should().BeFalse();
    }

    [Fact]
    public void GetItem_ExistingItem_ShouldReturnItem()
    {
        // Arrange
        var order = CreateDraftOrder();
        order.AddItem(_menuItemId, "Classic Burger", _itemPrice, 2);

        // Act
        var item = order.GetItem(_menuItemId);

        // Assert
        item.Should().NotBeNull();
        item!.Name.Should().Be("Classic Burger");
    }

    [Fact]
    public void GetItem_NonExistingItem_ShouldReturnNull()
    {
        // Arrange
        var order = CreateDraftOrder();

        // Act
        var item = order.GetItem(_menuItemId);

        // Assert
        item.Should().BeNull();
    }

    #endregion

    #region Helper Methods

    private Order CreateDraftOrder()
    {
        return Order.Create(_customerId, _restaurantId, _deliveryAddress);
    }

    private Order CreatePlacedOrder()
    {
        var order = CreateDraftOrder();
        order.AddItem(_menuItemId, "Classic Burger", _itemPrice, 1);
        order.Place();
        return order;
    }

    private Order CreateConfirmedOrder()
    {
        var order = CreatePlacedOrder();
        order.Confirm(DateTime.UtcNow.AddMinutes(20));
        return order;
    }

    private Order CreatePreparingOrder()
    {
        var order = CreateConfirmedOrder();
        order.StartPreparing();
        return order;
    }

    private Order CreateReadyForPickupOrder()
    {
        var order = CreatePreparingOrder();
        order.MarkReadyForPickup();
        return order;
    }

    private Order CreateOutForDeliveryOrder()
    {
        var order = CreateReadyForPickupOrder();
        order.PickUp();
        return order;
    }

    private Order CreateDeliveredOrder()
    {
        var order = CreateOutForDeliveryOrder();
        order.Deliver();
        return order;
    }

    #endregion
}