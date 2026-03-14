using Microsoft.AspNetCore.Mvc;
using RestaurantManagement.Adapters.Primary.Http.Common;
using RestaurantManagement.Adapters.Primary.Http.Contracts.Tables;
using RestaurantManagement.Application.Ports.Input;
using RestaurantManagement.Domain.Entities;

namespace RestaurantManagement.Adapters.Primary.Http.Controllers;

/// <summary>
/// Primary Adapter (Driving): Translates HTTP requests into use case calls via the ITableUseCase input port.
/// Has no direct dependency on the use case implementation or persistence layer.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class TablesController(ITableUseCase tableUseCase) : ControllerBase
{
    [HttpGet]
    public async Task<IResult> GetAllTables(CancellationToken cancellationToken)
    {
        var result = await tableUseCase.GetAllTablesAsync(cancellationToken);
        return result.ToApiResult();
    }

    [HttpPut("{tableId:int}/status")]
    public async Task<IResult> UpdateTableStatus(int tableId, [FromBody] UpdateTableStatusRequest request, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<TableStatus>(request.NewStatus, true, out var status))
            return Results.BadRequest("Invalid table status");

        var result = await tableUseCase.UpdateTableStatusAsync(tableId, status, cancellationToken);
        return result.ToApiResult();
    }
}
