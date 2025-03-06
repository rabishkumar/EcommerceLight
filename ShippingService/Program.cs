// Shipment Service - Independent Deployment in a Single Repo
// - Uses Minimal APIs, Dapper, Azure SQL
// - Independent from other services

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;

var builder = WebApplication.CreateBuilder(args);

// Register Services
builder.Services.AddScoped<IShipmentRepository, ShipmentRepository>();

var app = builder.Build();

app.MapPost("/shipments", async (Shipment shipment, IShipmentRepository repo) =>
{
    await repo.CreateShipment(shipment);
    return Results.Ok(shipment);
});

app.MapGet("/shipments/{id}", async (int id, IShipmentRepository repo) =>
{
    var shipment = await repo.GetShipmentById(id);
    return shipment is not null ? Results.Ok(shipment) : Results.NotFound();
});

app.Run();

// Repository Pattern for Shipments using Dapper
public interface IShipmentRepository
{
    Task CreateShipment(Shipment shipment);
    Task<Shipment?> GetShipmentById(int id);
}

public class ShipmentRepository : IShipmentRepository
{
    private readonly string _connectionString;
    public ShipmentRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("AzureSqlConnection");
    }
    
    public async Task CreateShipment(Shipment shipment)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.ExecuteAsync("INSERT INTO Shipments (OrderId, TrackingNumber, Status) VALUES (@OrderId, @TrackingNumber, @Status)", shipment);
    }
    
    public async Task<Shipment?> GetShipmentById(int id)
    {
        using var connection = new SqlConnection(_connectionString);
        return await connection.QueryFirstOrDefaultAsync<Shipment>("SELECT * FROM Shipments WHERE Id = @Id", new { Id = id });
    }
}

// Shipment Entity
public class Shipment
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public string TrackingNumber { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending";
}
