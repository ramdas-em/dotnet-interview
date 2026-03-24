using TodoApi.Domain.Entities;
using TodoApi.Application.Services;
using TodoApi.Infrastructure.Data;
using TodoApi.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace TodoApi.Tests;

public class TodoServiceTests : IDisposable
{
    private readonly TodoDbContext _dbContext;
    private readonly TodoService _service;

    public TodoServiceTests()
    {
        var options = new DbContextOptionsBuilder<TodoDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _dbContext = new TodoDbContext(options);
        var repository = new SqlServerTodoRepository(_dbContext);
        _service = new TodoService(repository, NullLogger<TodoService>.Instance);
    }

    public void Dispose() => _dbContext.Dispose();

    [Fact]
    public void Create_ShouldAddTodo()
    {
        var todo = new Todo { Title = "Test" };
        var result = _service.Create(todo);
        Assert.NotNull(result);
        Assert.Equal("Test", result.Title);
    }

    [Fact]
    public void Create_DuplicateTitle_ShouldReturnNull()
    {
        _service.Create(new Todo { Title = "Test" });
        var result = _service.Create(new Todo { Title = "Test" });
        Assert.Null(result);
    }

    [Fact]
    public void GetAll_ShouldReturnAll()
    {
        _service.Create(new Todo { Title = "A" });
        _service.Create(new Todo { Title = "B" });
        var all = _service.GetAll();
        Assert.Equal(2, all.Count);
    }

    [Fact]
    public void GetById_ShouldReturnTodo()
    {
        var created = _service.Create(new Todo { Title = "Test" });
        var found = _service.GetById(created.Id);
        Assert.NotNull(found);
        Assert.Equal(created.Id, found.Id);
    }

    [Fact]
    public void GetById_NotFound_ShouldReturnNull()
    {
        var found = _service.GetById(999);
        Assert.Null(found);
    }

    [Fact]
    public void Update_ShouldModifyTodo()
    {
        var created = _service.Create(new Todo { Title = "Test" });
        created.Title = "Updated";
        var updated = _service.Update(created.Id, created);
        Assert.NotNull(updated);
        Assert.Equal("Updated", updated.Title);
    }

    [Fact]
    public void Update_NotFound_ShouldReturnNull()
    {
        var updated = _service.Update(999, new Todo { Title = "X" });
        Assert.Null(updated);
    }

    [Fact]
    public void Delete_ShouldRemoveTodo()
    {
        var created = _service.Create(new Todo { Title = "Test" });
        var deleted = _service.Delete(created.Id);
        Assert.True(deleted);
        Assert.Null(_service.GetById(created.Id));
    }

    [Fact]
    public void Delete_NotFound_ShouldReturnFalse()
    {
        var deleted = _service.Delete(999);
        Assert.False(deleted);
    }
}
