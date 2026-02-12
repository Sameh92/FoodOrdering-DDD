using FoodOrdering.Application.Common;
using FoodOrdering.Application.Orders.DTOs;
using FoodOrdering.Domain.Ordering.Aggregates.Order;
using FoodOrdering.Domain.Ordering.Repositories;
using MediatR;

namespace FoodOrdering.Application.Orders.Commands.ConfirmOrder;

/// <summary>
/// Command to confirm an order (by restaurant).
/// </summary>
public record ConfirmOrderCommand : IRequest<Result<OrderDto>>
{
    public Guid OrderId { get; init; }
    public int EstimatedPrepTimeMinutes { get; init; } = 20;
}

/// <summary>
/// Handler for ConfirmOrderCommand.
/// </summary>
public class ConfirmOrderCommandHandler : IRequestHandler<ConfirmOrderCommand, Result<OrderDto>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ConfirmOrderCommandHandler(IOrderRepository orderRepository, IUnitOfWork unitOfWork)
    {
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<OrderDto>> Handle(ConfirmOrderCommand request, CancellationToken cancellationToken)
    {
        var orderId = OrderId.From(request.OrderId);
        var order = await _orderRepository.GetByIdAsync(orderId, cancellationToken);

        if (order == null)
            return Result.Failure<OrderDto>("Order not found");

        try
        {
            var estimatedReadyTime = DateTime.UtcNow.AddMinutes(request.EstimatedPrepTimeMinutes);
            order.Confirm(estimatedReadyTime);

            _orderRepository.Update(order);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success(OrderDto.FromOrder(order));
        }
        catch (Exception ex)
        {
            return Result.Failure<OrderDto>(ex.Message);
        }
    }
}