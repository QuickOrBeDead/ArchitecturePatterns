using RestaurantManagement.Application.Common;
using RestaurantManagement.Application.Common.DTOs;
using RestaurantManagement.Application.Ports.Input;
using RestaurantManagement.Application.Ports.Output;
using RestaurantManagement.Domain.Entities;

namespace RestaurantManagement.Application.UseCases.Tables;

/// <summary>
/// Use Case implementation that fulfills the ITableUseCase input port.
/// Depends only on output ports (IUnitOfWork), never on adapter implementations.
/// </summary>
public sealed class TableUseCase(IUnitOfWork unitOfWork) : ITableUseCase
{
    public async Task<Result<List<TableDto>>> GetAllTablesAsync(CancellationToken cancellationToken = default)
    {
        var tables = await unitOfWork.Tables.GetAllAsync(cancellationToken);

        var tableDtos = tables.Select(table => new TableDto(
            table.Id,
            table.TableNumber,
            table.Capacity,
            table.Status.ToString(),
            table.ReservedAt)).ToList();

        return Result<List<TableDto>>.Success(tableDtos);
    }

    public async Task<Result<TableDto>> UpdateTableStatusAsync(int tableId, TableStatus newStatus, CancellationToken cancellationToken = default)
    {
        if (tableId <= 0)
            return Result<TableDto>.Failure("Table ID must be greater than 0");

        var table = await unitOfWork.Tables.GetByIdAsync(tableId, cancellationToken);
        if (table is null)
            return Result<TableDto>.NotFound($"Table {tableId} not found");

        try
        {
            switch (newStatus)
            {
                case TableStatus.Available:
                    table.MakeAvailable();
                    break;
                case TableStatus.Occupied:
                    table.Occupy();
                    break;
                case TableStatus.Reserved:
                    table.Reserve(DateTime.UtcNow);
                    break;
                case TableStatus.OutOfService:
                    table.TakeOutOfService();
                    break;
                default:
                    return Result<TableDto>.Failure($"Cannot update table to status: {newStatus}");
            }
        }
        catch (InvalidOperationException ex)
        {
            return Result<TableDto>.Failure(ex.Message);
        }

        await unitOfWork.Tables.UpdateAsync(table, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var tableDto = new TableDto(
            table.Id,
            table.TableNumber,
            table.Capacity,
            table.Status.ToString(),
            table.ReservedAt);

        return Result<TableDto>.Success(tableDto);
    }
}
