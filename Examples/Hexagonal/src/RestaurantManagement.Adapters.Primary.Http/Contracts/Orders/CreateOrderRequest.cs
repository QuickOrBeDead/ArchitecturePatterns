namespace RestaurantManagement.Adapters.Primary.Http.Contracts.Orders;

public record CreateOrderRequest(int TableId, List<OrderItemRequest> Items, string? Notes);
