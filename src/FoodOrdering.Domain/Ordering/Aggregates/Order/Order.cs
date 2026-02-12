using FoodOrdering.Domain.Common;
using FoodOrdering.Domain.Customers.Aggregates.Customer;
using FoodOrdering.Domain.Ordering.Events;
using FoodOrdering.Domain.Ordering.ValueObjects;
using FoodOrdering.Domain.Restaurants.Aggregates.Restaurant;

namespace FoodOrdering.Domain.Ordering.Aggregates.Order;

/// <summary>
/// Order Aggregate Root.
/// 
/// This is the heart of our Ordering bounded context.
/// 
/// Key DDD principles demonstrated:
/// 1. Aggregate Root - Only entry point to the aggregate
/// 2. Encapsulation - Private setters, public methods
/// 3. Business Rules - All rules enforced here
/// 4. Domain Events - Announces what happened
/// 5. Ubiquitous Language - Methods match business language
/// </summary>
public class Order : AggregateRoot<OrderId>
{
    // References to other Aggregates BY ID ONLY (not direct references)
    public CustomerId CustomerId { get; private set; } = null!;
    public RestaurantId RestaurantId { get; private set; } = null!;

    // Value Objects
    public Address DeliveryAddress { get; private set; } = null!;
    public Money? DeliveryFee { get; private set; }

    // State
    public OrderStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? PlacedAt { get; private set; }
    public DateTime? ConfirmedAt { get; private set; }
    public DateTime? EstimatedReadyTime { get; private set; }
    public DateTime? PickedUpAt { get; private set; }
    public DateTime? DeliveredAt { get; private set; }
    public DateTime? CancelledAt { get; private set; }
    public string? CancellationReason { get; private set; }
    public string? SpecialInstructions { get; private set; }

    // Child entities - private collection, exposed as read-only
    private readonly List<OrderItem> _items = new();
    public IReadOnlyList<OrderItem> Items => _items.AsReadOnly();

    #region Factory Methods

    /// <summary>
    /// Create a new Order. This is the ONLY way to create an Order.
    /// Using factory method ensures Order is always valid.
    /// </summary>
    public static Order Create(
        CustomerId customerId,
        RestaurantId restaurantId,
        Address deliveryAddress,
        string? specialInstructions = null)
    {
        ArgumentNullException.ThrowIfNull(customerId);
        ArgumentNullException.ThrowIfNull(restaurantId);
        ArgumentNullException.ThrowIfNull(deliveryAddress);

        return new Order
        {
            Id = OrderId.New(),
            CustomerId = customerId,
            RestaurantId = restaurantId,
            DeliveryAddress = deliveryAddress,
            SpecialInstructions = specialInstructions,
            Status = OrderStatus.Draft,
            CreatedAt = DateTime.UtcNow
        };
    }

    #endregion

    #region Item Management

    /// <summary>
    /// Add an item to the order.
    /// Business Rule: Can only add items to draft orders.
    /// </summary>
    public void AddItem(
        MenuItemId menuItemId,
        string name,
        Money price,
        int quantity,
        string? specialInstructions = null)
    {
        EnsureCanModify();

        if (quantity <= 0)
            throw new ArgumentException("Quantity must be positive", nameof(quantity));

        // If item already exists, increase quantity
        var existingItem = _items.FirstOrDefault(i => i.MenuItemId == menuItemId);
        if (existingItem != null)
        {
            existingItem.IncreaseQuantity(quantity);
            if (!string.IsNullOrWhiteSpace(specialInstructions))
            {
                existingItem.SetSpecialInstructions(specialInstructions);
            }
        }
        else
        {
            _items.Add(new OrderItem(menuItemId, name, price, quantity, specialInstructions));
        }
    }

    /// <summary>
    /// Remove an item from the order.
    /// Business Rule: Can only remove items from draft orders.
    /// </summary>
    public void RemoveItem(MenuItemId menuItemId)
    {
        EnsureCanModify();

        var item = _items.FirstOrDefault(i => i.MenuItemId == menuItemId)
            ?? throw new InvalidOperationException("Item not found in order");

        _items.Remove(item);
    }

    /// <summary>
    /// Update item quantity.
    /// Business Rule: Can only update items in draft orders.
    /// </summary>
    public void UpdateItemQuantity(MenuItemId menuItemId, int newQuantity)
    {
        EnsureCanModify();

        var item = _items.FirstOrDefault(i => i.MenuItemId == menuItemId)
            ?? throw new InvalidOperationException("Item not found in order");

        if (newQuantity <= 0)
        {
            _items.Remove(item);
        }
        else
        {
            item.SetQuantity(newQuantity);
        }
    }

    /// <summary>
    /// Update special instructions for an item.
    /// </summary>
    public void UpdateItemInstructions(MenuItemId menuItemId, string? instructions)
    {
        EnsureCanModify();

        var item = _items.FirstOrDefault(i => i.MenuItemId == menuItemId)
            ?? throw new InvalidOperationException("Item not found in order");

        item.SetSpecialInstructions(instructions);
    }

    /// <summary>
    /// Clear all items from the order.
    /// </summary>
    public void ClearItems()
    {
        EnsureCanModify();
        _items.Clear();
    }

    #endregion

    #region Order Lifecycle

    /// <summary>
    /// Set the delivery fee.
    /// Typically called by a Domain Service (DeliveryFeeCalculator).
    /// </summary>
    public void SetDeliveryFee(Money fee)
    {
        EnsureCanModify();
        DeliveryFee = fee;
    }

    /// <summary>
    /// Update the delivery address.
    /// </summary>
    public void UpdateDeliveryAddress(Address address)
    {
        EnsureCanModify();
        DeliveryAddress = address;
    }

    /// <summary>
    /// Update special instructions for the entire order.
    /// </summary>
    public void UpdateSpecialInstructions(string? instructions)
    {
        EnsureCanModify();
        SpecialInstructions = instructions;
    }

    /// <summary>
    /// Place the order.
    /// Business Rules:
    /// - Must have at least one item
    /// - Changes status to PendingConfirmation
    /// - Records the timestamp
    /// - Raises OrderPlacedEvent
    /// </summary>
    public void Place()
    {
        EnsureCanModify();

        if (_items.Count == 0)
            throw new InvalidOperationException("Cannot place an empty order");

        Status = OrderStatus.PendingConfirmation;
        PlacedAt = DateTime.UtcNow;

        // Raise domain event
        var itemsData = _items.Select(i => new OrderItemData(
            i.MenuItemId,
            i.Name,
            i.Quantity,
            i.Price,
            i.SpecialInstructions
        )).ToList();

        AddDomainEvent(new OrderPlacedEvent(
            Id,
            CustomerId,
            RestaurantId,
            GetTotal(),
            itemsData,
            DeliveryAddress
        ));
    }

    /// <summary>
    /// Restaurant confirms the order.
    /// Business Rule: Only pending orders can be confirmed.
    /// </summary>
    public void Confirm(DateTime estimatedReadyTime)
    {
        if (Status != OrderStatus.PendingConfirmation)
            throw new InvalidStateTransitionException(Status.ToString(), "Confirm");

        Status = OrderStatus.Confirmed;
        ConfirmedAt = DateTime.UtcNow;
        EstimatedReadyTime = estimatedReadyTime;

        AddDomainEvent(new OrderConfirmedEvent(Id, RestaurantId, estimatedReadyTime));
    }

    /// <summary>
    /// Start preparing the order.
    /// </summary>
    public void StartPreparing()
    {
        if (Status != OrderStatus.Confirmed)
            throw new InvalidStateTransitionException(Status.ToString(), "StartPreparing");

        Status = OrderStatus.Preparing;

        AddDomainEvent(new OrderPreparationStartedEvent(Id, RestaurantId));
    }

    /// <summary>
    /// Mark order as ready for pickup.
    /// </summary>
    public void MarkReadyForPickup()
    {
        if (Status != OrderStatus.Preparing)
            throw new InvalidStateTransitionException(Status.ToString(), "MarkReadyForPickup");

        Status = OrderStatus.ReadyForPickup;

        AddDomainEvent(new OrderReadyForPickupEvent(Id, RestaurantId));
    }

    /// <summary>
    /// Driver picked up the order.
    /// </summary>
    public void PickUp()
    {
        if (Status != OrderStatus.ReadyForPickup)
            throw new InvalidStateTransitionException(Status.ToString(), "PickUp");

        Status = OrderStatus.OutForDelivery;
        PickedUpAt = DateTime.UtcNow;

        AddDomainEvent(new OrderPickedUpEvent(Id, CustomerId));
    }

    /// <summary>
    /// Order delivered to customer.
    /// </summary>
    public void Deliver()
    {
        if (Status != OrderStatus.OutForDelivery)
            throw new InvalidStateTransitionException(Status.ToString(), "Deliver");

        Status = OrderStatus.Delivered;
        DeliveredAt = DateTime.UtcNow;

        AddDomainEvent(new OrderDeliveredEvent(Id, CustomerId));
    }

    /// <summary>
    /// Cancel the order.
    /// Business Rule: Cannot cancel delivered orders or orders out for delivery.
    /// </summary>
    public void Cancel(string reason)
    {
        if (!Status.CanBeCancelled())
            throw new InvalidStateTransitionException(Status.ToString(), "Cancel");

        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Cancellation reason is required", nameof(reason));

        Status = OrderStatus.Cancelled;
        CancelledAt = DateTime.UtcNow;
        CancellationReason = reason;

        AddDomainEvent(new OrderCancelledEvent(Id, CustomerId, RestaurantId, reason));
    }

    #endregion

    #region Queries

    /// <summary>
    /// Check if order can be cancelled.
    /// </summary>
    public bool CanBeCancelled() => Status.CanBeCancelled();

    /// <summary>
    /// Check if order can be modified.
    /// </summary>
    public bool CanBeModified() => Status.CanBeModified();

    /// <summary>
    /// Check if order is in a final state.
    /// </summary>
    public bool IsFinal() => Status.IsFinal();

    /// <summary>
    /// Get the number of items in the order.
    /// </summary>
    public int GetItemCount() => _items.Sum(i => i.Quantity);

    /// <summary>
    /// Calculate subtotal (items only, no delivery fee).
    /// </summary>
    public Money GetSubtotal()
    {
        if (_items.Count == 0)
            return Money.Zero("MYR");

        var currency = _items[0].Price.Currency;
        var total = _items.Sum(item => item.Price.Amount * item.Quantity);
        return new Money(total, currency);
    }

    /// <summary>
    /// Calculate total (items + delivery fee).
    /// </summary>
    public Money GetTotal()
    {
        var subtotal = GetSubtotal();

        if (DeliveryFee == null)
            return subtotal;

        return subtotal + DeliveryFee;
    }

    /// <summary>
    /// Get a specific item by menu item ID.
    /// </summary>
    public OrderItem? GetItem(MenuItemId menuItemId)
    {
        return _items.FirstOrDefault(i => i.MenuItemId == menuItemId);
    }

    /// <summary>
    /// Check if order contains a specific menu item.
    /// </summary>
    public bool ContainsItem(MenuItemId menuItemId)
    {
        return _items.Any(i => i.MenuItemId == menuItemId);
    }

    #endregion

    #region Private Helpers

    private void EnsureCanModify()
    {
        if (!Status.CanBeModified())
            throw new InvalidStateTransitionException(Status.ToString(), "Modify");
    }

    #endregion

    // Private constructor for EF Core
    private Order() { }
}