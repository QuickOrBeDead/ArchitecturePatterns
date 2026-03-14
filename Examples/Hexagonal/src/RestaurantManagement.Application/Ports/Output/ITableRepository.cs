using RestaurantManagement.Domain.Entities;

namespace RestaurantManagement.Application.Ports.Output;

/// <summary>
/// Output Port (Driven Port): Defines the contract that the persistence adapter must fulfill.
/// In Hexagonal Architecture, this interface lives in the Application core,
/// NOT in the Domain layer (unlike Onion Architecture).
/// </summary>
public interface ITableRepository
{
    Task<Table?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Table?> GetByTableNumberAsync(int tableNumber, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Table>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Table>> GetAvailableTablesAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Table table, CancellationToken cancellationToken = default);
    Task UpdateAsync(Table table, CancellationToken cancellationToken = default);
    Task DeleteAsync(Table table, CancellationToken cancellationToken = default);
}
