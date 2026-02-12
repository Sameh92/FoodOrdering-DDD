using FoodOrdering.Application.Common;
using FoodOrdering.Application.Orders.DTOs;
using FoodOrdering.Domain.Ordering.Aggregates.Order;
using FoodOrdering.Domain.Ordering.Repositories;
using MediatR;

namespace FoodOrdering.Application.Orders.Queries.GetOrderById;

/// <summary>
/// Query to get an order by ID.
/// </summary>
public record GetOrderByIdQuery : IRequest<Result<OrderDto>>
{
    public Guid OrderId { get; init; }
}

/// <summary>
/// Handler for GetOrderByIdQuery.
/// </summary>
public class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery, Result<OrderDto>>
{
    private readonly IOrderRepository _orderRepository;

    public GetOrderByIdQueryHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<Result<OrderDto>> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var orderId = OrderId.From(request.OrderId);
        var order = await _orderRepository.GetByIdAsync(orderId, cancellationToken);

        if (order == null)
            return Result.Failure<OrderDto>("Order not found");

        return Result.Success(OrderDto.FromOrder(order));
    }
}