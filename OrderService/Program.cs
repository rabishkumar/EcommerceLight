// Order Service - Independent Deployment in a Single Repo
// - Uses Minimal APIs, EF Core, Azure SQL
// - Independent from other services

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

var builder = WebApplication.CreateBuilder(args);

// Configure EF Core with Azure SQL

 // Creates DB if it doesn’t exist
builder.Services.AddDbContext<OrderDbContext>(options =>
    options.UseSqlite("Data Source=order_service.db")
    );

// Register Services
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp",
        policy => policy.WithOrigins("http://localhost:4200") // ✅ Allow Angular frontend
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials()); // ✅ Required if using cookies/authentication
});

var app = builder.Build();
app.UseCors("AllowAngularApp");
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
    dbContext.Database.EnsureCreated();
}

app.MapPost("/orders", async (Order order, IOrderRepository repo) =>
{
    await repo.CreateOrder(order);
    return Results.Ok(order);
});

app.MapGet("/orders/{id}", async (int id, IOrderRepository repo) =>
{
    var order = await repo.GetOrderById(id);
    return order is not null ? Results.Ok(order) : Results.NotFound();
});
app.UseCors();
app.Run();

// DbContext
public class OrderDbContext : DbContext
{
    public OrderDbContext(DbContextOptions<OrderDbContext> options) : base(options) { }
    public DbSet<Order> Orders => Set<Order>();
}

// Repository Pattern
public interface IOrderRepository
{
    Task<Order?> GetOrderById(int id);
    Task CreateOrder(Order order);
}

public class OrderRepository : IOrderRepository
{
    private readonly OrderDbContext _context;
    public OrderRepository(OrderDbContext context) => _context = context;
    public async Task<Order?> GetOrderById(int id) => await _context.Orders.FindAsync(id);
    public async Task CreateOrder(Order order)
    {
        _context.Orders.Add(order);
        await _context.SaveChangesAsync();
    }
}

// Order Entity
public class Order
{
    public int Id { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
}
