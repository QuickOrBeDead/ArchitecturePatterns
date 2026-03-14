namespace RestaurantManagement.Application.Ports.Output;

/// <summary>
/// Output Port (Driven Port): Unit of Work pattern to coordinate persistence operations.
/// In Hexagonal Architecture, this interface lives in the Application core,
/// NOT in the Domain layer (unlike Onion Architecture).
/// </summary>
public interface IUnitOfWork
{
    ITableRepository Tables { get; }
    IMenuItemRepository MenuItems { get; }
    IOrderRepository Orders { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}
