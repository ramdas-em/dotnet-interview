using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using TodoApi.Application.DTOs;
using TodoApi.Application.Services;
using TodoApi.Controllers;
using TodoApi.Domain.Entities;
using TodoApi.Infrastructure.Data;
using TodoApi.Infrastructure.Repositories;

namespace TodoApi.Tests;

public class TodoControllerTests : IDisposable
{
    private readonly TodoController _controller;
    private readonly ITodoService _service;
    private readonly TodoDbContext _dbContext;

    public TodoControllerTests()
    {
        var options = new DbContextOptionsBuilder<TodoDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _dbContext = new TodoDbContext(options);
        var repository = new SqlServerTodoRepository(_dbContext);
        _service = new TodoService(repository, NullLogger<TodoService>.Instance);
        _controller = new TodoController(_service, NullLogger<TodoController>.Instance);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }

    [Fact]
    public void Create_WithValidRequest_ShouldReturnCreatedAtAction()
    {
        var request = new CreateTodoRequest { Title = "Test", Description = "Desc" };
        var result = _controller.Create(request);
        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        var response = Assert.IsType<TodoResponse>(createdResult.Value);
        Assert.Equal("Test", response.Title);
        Assert.Equal("Desc", response.Description);
        Assert.False(response.IsCompleted);
    }

    [Fact]
    public void Create_WithDuplicateTitle_ShouldReturnConflict()
    {
        var request = new CreateTodoRequest { Title = "Duplicate", Description = "First" };
        _controller.Create(request);
        var duplicateRequest = new CreateTodoRequest { Title = "Duplicate", Description = "Second" };
        var result = _controller.Create(duplicateRequest);
        Assert.IsType<ConflictObjectResult>(result);
    }

    [Fact]
    public void GetAll_WhenEmpty_ShouldReturnOkWithEmptyList()
    {
        var result = _controller.GetAll(null, null, false, 1, 10);
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = okResult.Value;
        Assert.NotNull(response);
        var itemsProperty = response.GetType().GetProperty("items");
        var totalProperty = response.GetType().GetProperty("total");
        Assert.NotNull(itemsProperty);
        Assert.NotNull(totalProperty);
        var items = (IEnumerable<object>)itemsProperty.GetValue(response);
        var total = (int)totalProperty.GetValue(response);
        Assert.Empty(items);
        Assert.Equal(0, total);
    }

    [Fact]
    public void GetAll_WithTodos_ShouldReturnAllTodos()
    {
        _service.Create(new Todo { Title = "First" });
        _service.Create(new Todo { Title = "Second" });
        var result = _controller.GetAll(null, null, false, 1, 10);
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = okResult.Value;
        Assert.NotNull(response);
        var itemsProperty = response.GetType().GetProperty("items");
        var totalProperty = response.GetType().GetProperty("total");
        Assert.NotNull(itemsProperty);
        Assert.NotNull(totalProperty);
        var items = (IEnumerable<object>)itemsProperty.GetValue(response);
        var total = (int)totalProperty.GetValue(response);
        Assert.Equal(2, total);
        Assert.Equal(2, items.Count());
    }

    [Fact]
    public void GetAll_Search_ShouldReturnFilteredTodos()
    {
        _service.Create(new Todo { Title = "Alpha" });
        _service.Create(new Todo { Title = "Beta" });
        var result = _controller.GetAll("Alpha", null, false, 1, 10);
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = okResult.Value;
        var itemsProperty = response.GetType().GetProperty("items");
        Assert.NotNull(itemsProperty);
        var items = (IEnumerable<object>)itemsProperty.GetValue(response);
        Assert.Single(items);
        Assert.Equal("Alpha", (string)((dynamic)items.ElementAt(0)).Title);
    }

    [Fact]
    public void GetAll_SortDescending_ShouldReturnSortedTodos()
    {
        _service.Create(new Todo { Title = "A" });
        _service.Create(new Todo { Title = "B" });
        var result = _controller.GetAll(null, "title", true, 1, 10);
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = okResult.Value;
        var itemsProperty = response.GetType().GetProperty("items");
        Assert.NotNull(itemsProperty);
        var items = (IEnumerable<object>)itemsProperty.GetValue(response);
        Assert.Equal("B", (string)((dynamic)items.ElementAt(0)).Title);
        Assert.Equal("A", (string)((dynamic)items.ElementAt(1)).Title);
    }

    [Fact]
    public void GetAll_Pagination_ShouldReturnPagedTodos()
    {
        for (int i = 1; i <= 15; i++)
            _service.Create(new Todo { Title = $"Todo {i}" });
        var result = _controller.GetAll(null, "id", false, 2, 5);
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = okResult.Value;
        // Use pattern matching to access anonymous type properties
        var itemsProperty = response.GetType().GetProperty("items");
        var totalProperty = response.GetType().GetProperty("total");
        if (itemsProperty != null && totalProperty != null)
        {
            var items = (IEnumerable<object>)itemsProperty.GetValue(response);
            // 'total' is not used, so do not assign it to avoid IDE0059
            Assert.Equal(5, items.Count());
            // Cast the first item to dynamic to access Title property
            Assert.Equal("Todo 6", (string)((dynamic)items.ElementAt(0)).Title);
        }
        else
        {
            throw new InvalidOperationException("Response does not have expected structure.");
        }
    }

    [Fact]
    public void GetById_WithExistingId_ShouldReturnOk()
    {
        var created = _service.Create(new Todo { Title = "Test" });
        var result = _controller.GetById(created.Id);
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = okResult.Value;
        var todoProperty = response.GetType().GetProperty("todo");
        Assert.NotNull(todoProperty);
        var todo = todoProperty.GetValue(response);
        Assert.NotNull(todo);
        // Cast to dynamic to access Title property
        Assert.Equal("Test", (string)((dynamic)todo).Title);
    }

    [Fact]
    public void GetById_WithNonExistingId_ShouldReturnNotFound()
    {
        var result = _controller.GetById(999);
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public void Update_WithExistingId_ShouldReturnOk()
    {
        var created = _service.Create(new Todo { Title = "Original" });
        var request = new UpdateTodoRequest
        {
            Title = "Updated",
            Description = "Updated Desc",
            IsCompleted = true
        };
        var result = _controller.Update(created.Id, request);
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = okResult.Value;
        var todoProperty = response.GetType().GetProperty("todo");
        Assert.NotNull(todoProperty);
        var todo = todoProperty.GetValue(response);
        Assert.NotNull(todo);
        Assert.Equal("Updated", (string)((dynamic)todo).Title);
        Assert.True((bool)((dynamic)todo).IsCompleted);
    }

    [Fact]
    public void Update_WithNonExistingId_ShouldReturnNotFound()
    {
        var request = new UpdateTodoRequest { Title = "Updated" };
        var result = _controller.Update(999, request);
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public void Delete_WithExistingId_ShouldReturnOk()
    {
        var created = _service.Create(new Todo { Title = "To delete" });
        var result = _controller.Delete(created.Id);
        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public void Delete_WithNonExistingId_ShouldReturnNotFound()
    {
        var result = _controller.Delete(999);
        Assert.IsType<NotFoundObjectResult>(result);
    }
}
