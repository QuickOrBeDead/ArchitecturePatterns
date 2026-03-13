using FluentValidation;
using Microsoft.EntityFrameworkCore;
using RestaurantManagement.Adapters.Secondary.Persistence.Data;
using RestaurantManagement.Adapters.Secondary.Persistence.Repositories;
using RestaurantManagement.Application.Ports.Input;
using RestaurantManagement.Application.Ports.Output;
using RestaurantManagement.Application.UseCases.MenuItems;
using RestaurantManagement.Application.UseCases.Orders;
using RestaurantManagement.Application.UseCases.Tables;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

// ─── Secondary Adapters (Driven) ─────────────────────────────────────────────
// EF Core In-Memory Database
builder.Services.AddDbContext<RestaurantDbContext>(options =>
    options.UseInMemoryDatabase("RestaurantDb"));

// Output Port implementations (repository adapters)
builder.Services.AddScoped<ITableRepository, TableRepository>();
builder.Services.AddScoped<IMenuItemRepository, MenuItemRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// ─── Application Core ─────────────────────────────────────────────────────────
// FluentValidation validators
builder.Services.AddValidatorsFromAssembly(
    typeof(CreateOrderValidator).Assembly, ServiceLifetime.Singleton);

// Input Port implementations (use case services)
builder.Services.AddScoped<ITableUseCase, TableUseCase>();
builder.Services.AddScoped<IOrderUseCase, OrderUseCase>();
builder.Services.AddScoped<IMenuItemUseCase, MenuItemUseCase>();

var app = builder.Build();

// Seed database
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<RestaurantDbContext>();
    await context.Database.EnsureCreatedAsync().ConfigureAwait(false);
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

await app.RunAsync().ConfigureAwait(false);

// Make Program class accessible for testing
public partial class Program { }
