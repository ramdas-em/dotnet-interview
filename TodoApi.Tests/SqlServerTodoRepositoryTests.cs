using TodoApi.Domain.Entities;
using TodoApi.Domain.Repositories;
using TodoApi.Infrastructure.Data;
using TodoApi.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace TodoApi.Tests;

public class SqlServerTodoRepositoryTests : IDisposable
{
    private readonly TodoDbContext _dbContext;
    private readonly SqlServerTodoRepository _repository;

    public SqlServerTodoRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<TodoDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _dbContext = new TodoDbContext(options);
        _repository = new SqlServerTodoRepository(_dbContext);
    }

    public void Dispose() => _dbContext.Dispose();

    [Fact]
    public void Create_ShouldAddTodo()
    {
        var todo = new Todo { Title = "Test" };
        var result = _repository.Create(todo);
        Assert.NotNull(result);
        Assert.Equal("Test", result.Title);
    }

    [Fact]
    public void ExistsByTitle_ShouldReturnTrueIfExists()
    {
        _repository.Create(new Todo { Title = "Test" });
        Assert.True(_repository.ExistsByTitle("Test"));
    }

    [Fact]
    public void ExistsByTitle_ShouldReturnFalseIfNotExists()
    {
        Assert.False(_repository.ExistsByTitle("NotExist"));
    }

    [Fact]
    public void GetAll_ShouldReturnAll()
    {
        _repository.Create(new Todo { Title = "A" });
        _repository.Create(new Todo { Title = "B" });
        var all = _repository.GetAll();
        Assert.Equal(2, all.Count);
    }

    [Fact]
    public void GetById_ShouldReturnTodo()
    {
        var created = _repository.Create(new Todo { Title = "Test" });
        var found = _repository.GetById(created.Id);
        Assert.NotNull(found);
        Assert.Equal(created.Id, found.Id);
    }

    [Fact]
    public void GetById_NotFound_ShouldReturnNull()
    {
        var found = _repository.GetById(999);
        Assert.Null(found);
    }

    [Fact]
    public void Update_ShouldModifyTodo()
    {
        var created = _repository.Create(new Todo { Title = "Test" });
        created.Title = "Updated";
        var updated = _repository.Update(created.Id, created);
        Assert.True(updated);
        var found = _repository.GetById(created.Id);
        Assert.Equal("Updated", found.Title);
    }

    [Fact]
    public void Update_NotFound_ShouldReturnFalse()
    {
        var updated = _repository.Update(999, new Todo { Title = "X" });
        Assert.False(updated);
    }

    [Fact]
    public void Delete_ShouldRemoveTodo()
    {
        var created = _repository.Create(new Todo { Title = "Test" });
        var deleted = _repository.Delete(created.Id);
        Assert.True(deleted);
        Assert.Null(_repository.GetById(created.Id));
    }

    [Fact]
    public void Delete_NotFound_ShouldReturnFalse()
    {
        var deleted = _repository.Delete(999);
        Assert.False(deleted);
    }
}
