using FoodOrdering.Application.Common;
using FoodOrdering.Application.Orders.DTOs;
using FoodOrdering.Domain.Customers.Aggregates.Customer;
using FoodOrdering.Domain.Ordering.Repositories;
using MediatR;

namespace FoodOrdering.Application.Orders.Queries.GetCustomerOrders;

/// <summary>
/// Query to get all orders for a customer.
/// </summary>
public record GetCustomerOrdersQuery : IRequest<Result<List<OrderSummaryDto>>>
{
    public Guid CustomerId { get; init; }
}

/// <summary>
/// Handler for GetCustomerOrdersQuery.
/// </summary>
public class GetCustomerOrdersQueryHandler : IRequestHandler<GetCustomerOrdersQuery, Result<List<OrderSummaryDto>>>
{
    private readonly IOrderRepository _orderRepository;

    public GetCustomerOrdersQueryHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<Result<List<OrderSummaryDto>>> Handle(GetCustomerOrdersQuery request, CancellationToken cancellationToken)
    {
        var customerId = CustomerId.From(request.CustomerId);
        var orders = await _orderRepository.GetByCustomerIdAsync(customerId, cancellationToken);

        var orderDtos = orders.Select(OrderSummaryDto.FromOrder).ToList();

        return Result.Success(orderDtos);
    }
}