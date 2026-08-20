using BillingService.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using BillingService.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new JsonStringEnumConverter()
        );
    });

builder.Services.AddOpenApi();

var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<BillingDbContext>(options =>
    options.UseMySQL(connectionString!)
);
builder.Services.AddHttpClient<InventoryServiceClient>(client =>
{
    client.BaseAddress = new Uri("http://localhost:5277");
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapControllers();

app.Run();