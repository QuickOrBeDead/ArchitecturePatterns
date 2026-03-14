namespace RestaurantManagement.Application.UseCases.Orders;

public record CreateOrderRequest(int TableId, List<OrderItemRequest> Items, string? Notes);

public record OrderItemRequest(int MenuItemId, int Quantity, string? SpecialInstructions);
