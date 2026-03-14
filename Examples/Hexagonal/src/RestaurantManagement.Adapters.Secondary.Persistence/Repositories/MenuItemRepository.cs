using Microsoft.EntityFrameworkCore;
using RestaurantManagement.Adapters.Secondary.Persistence.Data;
using RestaurantManagement.Application.Ports.Output;
using RestaurantManagement.Domain.Entities;

namespace RestaurantManagement.Adapters.Secondary.Persistence.Repositories;

/// <summary>
/// Secondary Adapter (Driven): Implements the IMenuItemRepository output port
/// defined in the Application core. The Application core has no knowledge of EF Core.
/// </summary>
public sealed class MenuItemRepository(RestaurantDbContext context) : IMenuItemRepository
{
    public async Task<MenuItem?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await context.MenuItems.FindAsync([id], cancellationToken);
    }

    public async Task<IReadOnlyList<MenuItem>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await context.MenuItems
            .OrderBy(m => m.Category)
            .ThenBy(m => m.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<MenuItem>> GetAvailableAsync(CancellationToken cancellationToken = default)
    {
        return await context.MenuItems
            .Where(m => m.IsAvailable)
            .OrderBy(m => m.Category)
            .ThenBy(m => m.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<MenuItem>> GetByCategoryAsync(string category, CancellationToken cancellationToken = default)
    {
        return await context.MenuItems
            .Where(m => m.Category == category && m.IsAvailable)
            .OrderBy(m => m.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<MenuItem>> GetByIdsAsync(IEnumerable<int> ids, CancellationToken cancellationToken = default)
    {
        var idList = ids.ToList();
        return await context.MenuItems
            .Where(m => idList.Contains(m.Id))
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(MenuItem menuItem, CancellationToken cancellationToken = default)
    {
        await context.MenuItems.AddAsync(menuItem, cancellationToken);
    }

    public Task UpdateAsync(MenuItem menuItem, CancellationToken cancellationToken = default)
    {
        context.MenuItems.Update(menuItem);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(MenuItem menuItem, CancellationToken cancellationToken = default)
    {
        context.MenuItems.Remove(menuItem);
        return Task.CompletedTask;
    }
}
