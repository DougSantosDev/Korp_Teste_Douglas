using InventoryService.Data;
using InventoryService.Infrastructure;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy
            .WithOrigins("http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection");

if (connectionString?.Contains("server=localhost", StringComparison.OrdinalIgnoreCase) == true &&
    !connectionString.Contains("SslMode", StringComparison.OrdinalIgnoreCase))
{
    connectionString += ";SslMode=Disabled;";
}

builder.Services.AddDbContext<InventoryDbContext>(options =>
    options.UseMySQL(connectionString!)
);

var app = builder.Build();

app.UseExceptionHandler();
app.UseCors("AllowAngular");

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapControllers();

app.Run();
