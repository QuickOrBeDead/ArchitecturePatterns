using RestaurantManagement.Application.Common;
using RestaurantManagement.Application.Common.DTOs;
using RestaurantManagement.Application.Ports.Input;
using RestaurantManagement.Application.Ports.Output;

namespace RestaurantManagement.Application.UseCases.MenuItems;

/// <summary>
/// Use Case implementation that fulfills the IMenuItemUseCase input port.
/// Depends only on output ports (IUnitOfWork), never on adapter implementations.
/// </summary>
public sealed class MenuItemUseCase(IUnitOfWork unitOfWork) : IMenuItemUseCase
{
    public async Task<Result<List<MenuItemDto>>> GetMenuItemsAsync(string? category = null, CancellationToken cancellationToken = default)
    {
        var menuItems = string.IsNullOrWhiteSpace(category)
            ? await unitOfWork.MenuItems.GetAvailableAsync(cancellationToken)
            : await unitOfWork.MenuItems.GetByCategoryAsync(category, cancellationToken);

        var menuItemDtos = menuItems.Select(item => new MenuItemDto(
            item.Id,
            item.Name,
            item.Category,
            item.Price,
            item.Description,
            item.IsAvailable)).ToList();

        return Result<List<MenuItemDto>>.Success(menuItemDtos);
    }
}
