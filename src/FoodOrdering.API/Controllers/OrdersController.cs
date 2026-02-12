using FoodOrdering.API.DTOs;
using FoodOrdering.API.DTOs.OrdersDTOs;
using FoodOrdering.Application.Orders.Commands.CancelOrder;
using FoodOrdering.Application.Orders.Commands.ConfirmOrder;
using FoodOrdering.Application.Orders.Commands.PlaceOrder;
using FoodOrdering.Application.Orders.DTOs;
using FoodOrdering.Application.Orders.Queries.GetCustomerOrders;
using FoodOrdering.Application.Orders.Queries.GetOrderById;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FoodOrdering.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IMediator _mediator;

    public OrdersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Place a new order.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> PlaceOrder([FromBody] PlaceOrderRequest request)
    {
        var command = new PlaceOrderCommand
        {
            CustomerId = request.CustomerId,
            RestaurantId = request.RestaurantId,
            DeliveryAddress = new AddressDto
            {
                Street = request.DeliveryAddress.Street,
                City = request.DeliveryAddress.City,
                State = request.DeliveryAddress.State,
                PostalCode = request.DeliveryAddress.PostalCode,
                Country = request.DeliveryAddress.Country
            },
            Items = request.Items.Select(i => new OrderItemRequest
            {
                MenuItemId = i.MenuItemId,
                Quantity = i.Quantity,
                SpecialInstructions = i.SpecialInstructions
            }).ToList(),
            SpecialInstructions = request.SpecialInstructions
        };

        var result = await _mediator.Send(command);

        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return CreatedAtAction(nameof(GetOrder), new { id = result.Value!.Id }, result.Value);
    }

    /// <summary>
    /// Get an order by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOrder(Guid id)
    {
        var query = new GetOrderByIdQuery { OrderId = id };
        var result = await _mediator.Send(query);

        if (result.IsFailure)
            return NotFound(new { error = result.Error });

        return Ok(result.Value);
    }

    /// <summary>
    /// Get all orders for a customer.
    /// </summary>
    [HttpGet("customer/{customerId:guid}")]
    [ProducesResponseType(typeof(List<OrderSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCustomerOrders(Guid customerId)
    {
        var query = new GetCustomerOrdersQuery { CustomerId = customerId };
        var result = await _mediator.Send(query);

        return Ok(result.Value);
    }

    /// <summary>
    /// Confirm an order (restaurant action).
    /// </summary>
    [HttpPost("{id:guid}/confirm")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ConfirmOrder(Guid id, [FromBody] ConfirmOrderRequest? request = null)
    {
        var command = new ConfirmOrderCommand
        {
            OrderId = id,
            EstimatedPrepTimeMinutes = request?.EstimatedPrepTimeMinutes ?? 20
        };

        var result = await _mediator.Send(command);

        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return Ok(result.Value);
    }

    /// <summary>
    /// Cancel an order.
    /// </summary>
    [HttpPost("{id:guid}/cancel")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CancelOrder(Guid id, [FromBody] CancelOrderRequest request)
    {
        var command = new CancelOrderCommand
        {
            OrderId = id,
            Reason = request.Reason
        };

        var result = await _mediator.Send(command);

        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return Ok(result.Value);
    }
}
