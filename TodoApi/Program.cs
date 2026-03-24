using FluentValidation;
using FluentValidation.AspNetCore;
using TodoApi.Application.Services;
using TodoApi.Application.Services;
using TodoApi.Domain.Repositories;
using TodoApi.Infrastructure.Data;
using TodoApi.Infrastructure.Repositories;
using TodoApi.Middleware;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddControllers();
// Add FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();
builder.Services.AddValidatorsFromAssemblyContaining<TodoApi.Application.Validators.CreateTodoRequestValidator>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<ITodoRepository>(_ => new SqlServerTodoRepository(connectionString));
builder.Services.AddScoped<ITodoService, TodoService>();

// Run database migrations
var migrator = new DatabaseMigrator(connectionString);
migrator.Migrate();

var app = builder.Build();

// Global exception handling middleware — must be first in the pipeline
app.UseMiddleware<GlobalExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
