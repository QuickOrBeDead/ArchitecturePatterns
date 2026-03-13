using Microsoft.AspNetCore.Mvc;
using RestaurantManagement.Adapters.Primary.Http.Common;
using RestaurantManagement.Adapters.Primary.Http.Contracts.Orders;
using RestaurantManagement.Application.Ports.Input;
using RestaurantManagement.Domain.Entities;
using AppOrders = RestaurantManagement.Application.UseCases.Orders;

namespace RestaurantManagement.Adapters.Primary.Http.Controllers;

/// <summary>
/// Primary Adapter (Driving): Translates HTTP requests into use case calls via the IOrderUseCase input port.
/// Has no direct dependency on the use case implementation or persistence layer.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class OrdersController(IOrderUseCase orderUseCase) : ControllerBase
{
    [HttpPost]
    public async Task<IResult> CreateOrder([FromBody] CreateOrderRequest request, CancellationToken cancellationToken)
    {
        var appRequest = new AppOrders.CreateOrderRequest(
            request.TableId,
            request.Items.Select(i => new AppOrders.OrderItemRequest(i.MenuItemId, i.Quantity, i.SpecialInstructions)).ToList(),
            request.Notes);

        var result = await orderUseCase.CreateOrderAsync(appRequest, cancellationToken);
        return result.ToApiResult(data => Results.Created($"/api/orders/{data?.Id}", data));
    }

    [HttpPut("{orderId:int}/status")]
    public async Task<IResult> UpdateOrderStatus(int orderId, [FromBody] UpdateOrderStatusRequest request, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<OrderStatus>(request.NewStatus, true, out var status))
            return Results.BadRequest("Invalid order status");

        var result = await orderUseCase.UpdateOrderStatusAsync(orderId, status, cancellationToken);
        return result.ToApiResult();
    }

    [HttpGet("kitchen")]
    public async Task<IResult> GetKitchenOrders(CancellationToken cancellationToken)
    {
        var result = await orderUseCase.GetKitchenOrdersAsync(cancellationToken);
        return result.ToApiResult();
    }
}
