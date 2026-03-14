using RestaurantManagement.Domain.Entities;

namespace RestaurantManagement.Application.Ports.Output;

/// <summary>
/// Output Port (Driven Port): Defines the contract that the persistence adapter must fulfill.
/// In Hexagonal Architecture, this interface lives in the Application core,
/// NOT in the Domain layer (unlike Onion Architecture).
/// </summary>
public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Order?> GetByOrderNumberAsync(string orderNumber, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Order>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Order>> GetByTableIdAsync(int tableId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Order>> GetByStatusAsync(OrderStatus status, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Order>> GetKitchenOrdersAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Order order, CancellationToken cancellationToken = default);
    Task UpdateAsync(Order order, CancellationToken cancellationToken = default);
    Task DeleteAsync(Order order, CancellationToken cancellationToken = default);
}
