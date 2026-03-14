using RestaurantManagement.Application.Common;
using RestaurantManagement.Application.Common.DTOs;
using RestaurantManagement.Application.UseCases.Orders;
using RestaurantManagement.Domain.Entities;

namespace RestaurantManagement.Application.Ports.Input;

/// <summary>
/// Input Port (Driving Port): Defines the use cases exposed by the application.
/// The HTTP adapter (controller) depends on this interface, not on the use case implementation.
/// This is the key difference from Onion Architecture, where controllers call MediatR directly.
/// </summary>
public interface IOrderUseCase
{
    Task<Result<OrderDto>> CreateOrderAsync(CreateOrderRequest request, CancellationToken cancellationToken = default);
    Task<Result<OrderDto>> UpdateOrderStatusAsync(int orderId, OrderStatus newStatus, CancellationToken cancellationToken = default);
    Task<Result<List<OrderDto>>> GetKitchenOrdersAsync(CancellationToken cancellationToken = default);
}
