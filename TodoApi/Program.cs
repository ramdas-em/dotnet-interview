using TodoApi.Data;
using TodoApi.Middleware;
using TodoApi.Repositories;
using TodoApi.Services;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddControllers();
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
