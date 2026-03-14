namespace RestaurantManagement.Adapters.Primary.Http.Contracts.Orders;

public record OrderItemRequest(int MenuItemId, int Quantity, string? SpecialInstructions);
