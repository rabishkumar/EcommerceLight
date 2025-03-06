// Payment Service - Independent Deployment in a Single Repo
// - Uses Minimal APIs, Dapper, Azure SQL
// - Independent from other services

using Microsoft.Data.Sqlite;
using Dapper;

var builder = WebApplication.CreateBuilder(args);

// Register Services
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();

var app = builder.Build();

app.MapPost("/payments", async (Payment payment, IPaymentRepository repo) =>
{
    try{
    await repo.ProcessPayment(payment);
    return Results.Ok(payment);
    }
    catch(Exception ex){
    return Results.BadRequest(ex.Message);
    }
});

app.Run();

// Repository Pattern for Payments using Dapper
public interface IPaymentRepository
{
    Task ProcessPayment(Payment payment);
}

public class PaymentRepository : IPaymentRepository
{
    private readonly string _connectionString;
    
    public PaymentRepository(IConfiguration configuration)
    {
        _connectionString = "Data Source=payment_database.db";
      // EnsureDatabaseCreated();
    }
    void EnsureDatabaseCreated()
{
    using var connection = new SqliteConnection(_connectionString);
    connection.Execute("DROP TABLE IF EXISTS Payments;");

    connection.Execute(@"
       CREATE TABLE Payments (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    OrderId INTEGER NOT NULL,
    Amount REAL NOT NULL
    
);");
}

    public async Task ProcessPayment(Payment payment)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.ExecuteAsync("INSERT INTO Payments (OrderId, Amount) VALUES (@OrderId, @Amount)", payment);
    }
}

// Payment Entity
public class Payment
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public decimal Amount { get; set; }
}
