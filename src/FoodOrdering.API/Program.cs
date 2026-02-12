using FoodOrdering.Application;
using FoodOrdering.Infrastructure;
using FoodOrdering.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Food Ordering API", Version = "v1" });
});

// Add Application layer services
builder.Services.AddApplication();

// Add Infrastructure layer services
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Data Source=foodordering.db";
builder.Services.AddInfrastructure(connectionString);

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Ensure database is created and seeded
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await context.Database.EnsureCreatedAsync();
    await SeedData.SeedAsync(context);
}

app.Run();

// Make the implicit Program class accessible to integration tests
public partial class Program { }