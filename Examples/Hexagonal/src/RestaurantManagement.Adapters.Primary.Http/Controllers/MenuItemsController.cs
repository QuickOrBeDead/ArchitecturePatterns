using Microsoft.AspNetCore.Mvc;
using RestaurantManagement.Adapters.Primary.Http.Common;
using RestaurantManagement.Application.Ports.Input;

namespace RestaurantManagement.Adapters.Primary.Http.Controllers;

/// <summary>
/// Primary Adapter (Driving): Translates HTTP requests into use case calls via the IMenuItemUseCase input port.
/// Has no direct dependency on the use case implementation or persistence layer.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class MenuItemsController(IMenuItemUseCase menuItemUseCase) : ControllerBase
{
    [HttpGet]
    public async Task<IResult> GetMenuItems([FromQuery] string? category, CancellationToken cancellationToken)
    {
        var result = await menuItemUseCase.GetMenuItemsAsync(category, cancellationToken);
        return result.ToApiResult();
    }
}
