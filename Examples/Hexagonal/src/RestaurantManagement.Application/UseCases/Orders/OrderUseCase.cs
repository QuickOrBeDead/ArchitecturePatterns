using FluentValidation;
using RestaurantManagement.Application.Common;
using RestaurantManagement.Application.Common.DTOs;
using RestaurantManagement.Application.Ports.Input;
using RestaurantManagement.Application.Ports.Output;
using RestaurantManagement.Domain.Entities;

namespace RestaurantManagement.Application.UseCases.Orders;

/// <summary>
/// Use Case implementation that fulfills the IOrderUseCase input port.
/// Depends only on output ports (IUnitOfWork), never on adapter implementations.
/// Validation is performed explicitly via FluentValidation, without a pipeline.
/// </summary>
public sealed class OrderUseCase(
    IUnitOfWork unitOfWork,
    IValidator<CreateOrderRequest> createOrderValidator) : IOrderUseCase
{
    public async Task<Result<List<OrderDto>>> GetKitchenOrdersAsync(CancellationToken cancellationToken = default)
    {
        var orders = await unitOfWork.Orders.GetKitchenOrdersAsync(cancellationToken);

        if (!orders.Any())
        {
            return Result<List<OrderDto>>.Success([]);
        }

        var menuItemIds = orders.SelectMany(o => o.OrderItems.Select(oi => oi.MenuItemId)).Distinct();
        var menuItems = await unitOfWork.MenuItems.GetByIdsAsync(menuItemIds, cancellationToken);

        var orderDtos = orders.Select(order =>
        {
            var orderItemDtos = order.OrderItems.Select(oi =>
            {
                var menuItem = menuItems.First(m => m.Id == oi.MenuItemId);
                return new OrderItemDto(oi.Id, menuItem.Name, oi.Quantity, oi.Price, oi.SpecialInstructions);
            }).ToList();

            return new OrderDto(
                order.Id,
                order.OrderNumber,
                order.TableId,
                order.OrderDate,
                order.Status.ToString(),
                order.TotalAmount,
                order.Notes,
                orderItemDtos);
        }).ToList();

        return Result<List<OrderDto>>.Success(orderDtos);
    }

    public async Task<Result<OrderDto>> CreateOrderAsync(CreateOrderRequest request, CancellationToken cancellationToken = default)
    {
        var validationResult = await createOrderValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var propertyErrors = validationResult.Errors
                .GroupBy(f => f.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(f => f.ErrorMessage).ToArray());

            return Result<OrderDto>.ValidationFailure(
                validationResult.Errors.Select(f => f.ErrorMessage).ToList(),
                propertyErrors);
        }

        var table = await unitOfWork.Tables.GetByIdAsync(request.TableId, cancellationToken);
        if (table is null)
            return Result<OrderDto>.NotFound($"Table {request.TableId} not found");

        var menuItemIds = request.Items.Select(i => i.MenuItemId).Distinct();
        var availableMenuItems = await unitOfWork.MenuItems.GetByIdsAsync(menuItemIds, cancellationToken);

        var requestedMenuItemIds = request.Items.Select(i => i.MenuItemId).Distinct().ToList();
        var unavailableMenuItemIds = requestedMenuItemIds
            .Where(id => !availableMenuItems.Any(m => m.Id == id && m.IsAvailable))
            .ToList();

        if (unavailableMenuItemIds.Any())
        {
            return Result<OrderDto>.Failure(
                "Some menu items are unavailable",
                ResultType.Failure,
                errorDetails: new Dictionary<string, object> { ["UnavailableMenuItemIds"] = unavailableMenuItemIds });
        }

        var orderNumber = $"ORD-{DateTime.UtcNow:yyyyMMdd-HHmmss}";
        var order = new Order(orderNumber, request.TableId, request.Notes);

        foreach (var itemRequest in request.Items)
        {
            var menuItem = availableMenuItems.First(m => m.Id == itemRequest.MenuItemId);
            order.AddOrderItem(itemRequest.MenuItemId, itemRequest.Quantity, menuItem.Price, itemRequest.SpecialInstructions);
        }

        await unitOfWork.Orders.AddAsync(order, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var orderItemDtos = order.OrderItems.Select(oi =>
        {
            var menuItem = availableMenuItems.First(m => m.Id == oi.MenuItemId);
            return new OrderItemDto(oi.Id, menuItem.Name, oi.Quantity, oi.Price, oi.SpecialInstructions);
        }).ToList();

        var orderDto = new OrderDto(
            order.Id,
            order.OrderNumber,
            order.TableId,
            order.OrderDate,
            order.Status.ToString(),
            order.TotalAmount,
            order.Notes,
            orderItemDtos);

        return Result<OrderDto>.Success(orderDto);
    }

    public async Task<Result<OrderDto>> UpdateOrderStatusAsync(int orderId, OrderStatus newStatus, CancellationToken cancellationToken = default)
    {
        if (orderId <= 0)
            return Result<OrderDto>.Failure("Order ID must be greater than 0");

        var order = await unitOfWork.Orders.GetByIdAsync(orderId, cancellationToken);
        if (order is null)
            return Result<OrderDto>.NotFound($"Order {orderId} not found");

        try
        {
            switch (newStatus)
            {
                case OrderStatus.InPreparation:
                    order.StartPreparation();
                    break;
                case OrderStatus.Ready:
                    order.MarkAsReady();
                    break;
                case OrderStatus.Served:
                    order.Serve();
                    break;
                case OrderStatus.Cancelled:
                    order.Cancel();
                    break;
                default:
                    return Result<OrderDto>.Failure($"Cannot update order to status: {newStatus}");
            }
        }
        catch (InvalidOperationException ex)
        {
            return Result<OrderDto>.Failure(ex.Message);
        }

        await unitOfWork.Orders.UpdateAsync(order, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var menuItems = await unitOfWork.MenuItems.GetByIdsAsync(
            order.OrderItems.Select(oi => oi.MenuItemId),
            cancellationToken);

        var orderItemDtos = order.OrderItems.Select(oi =>
        {
            var menuItem = menuItems.First(m => m.Id == oi.MenuItemId);
            return new OrderItemDto(oi.Id, menuItem.Name, oi.Quantity, oi.Price, oi.SpecialInstructions);
        }).ToList();

        var orderDto = new OrderDto(
            order.Id,
            order.OrderNumber,
            order.TableId,
            order.OrderDate,
            order.Status.ToString(),
            order.TotalAmount,
            order.Notes,
            orderItemDtos);

        return Result<OrderDto>.Success(orderDto);
    }
}
