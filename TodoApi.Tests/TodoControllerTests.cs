using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using TodoApi.Application.DTOs;
using TodoApi.Application.Services;
using TodoApi.Controllers;
using TodoApi.Domain.Entities;

namespace TodoApi.Tests;

public class TodoControllerTests : IDisposable
{
    private readonly TodoController _controller;
    private readonly ITodoService _service;
    private readonly FakeTodoRepository _repository;

    public TodoControllerTests()
    {
        _repository = new FakeTodoRepository();
        _service = new TodoService(_repository, NullLogger<TodoService>.Instance);
        _controller = new TodoController(_service, NullLogger<TodoController>.Instance);
    }

    public void Dispose()
    {
        _repository.Dispose();
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
    public void GetAll_WhenEmpty_ShouldReturnOkWithEmptyList()
    {
        var result = _controller.GetAll();

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
    }

    [Fact]
    public void GetAll_WithTodos_ShouldReturnAllTodos()
    {
        _service.Create(new Todo { Title = "First" });
        _service.Create(new Todo { Title = "Second" });

        var result = _controller.GetAll();

        var okResult = Assert.IsType<OkObjectResult>(result);
        var todos = Assert.IsAssignableFrom<IEnumerable<TodoResponse>>(okResult.Value);
        Assert.Equal(2, todos.Count());
    }

    [Fact]
    public void GetById_WithExistingId_ShouldReturnOk()
    {
        var created = _service.Create(new Todo { Title = "Test" });

        var result = _controller.GetById(created.Id);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<TodoResponse>(okResult.Value);
        Assert.Equal("Test", response.Title);
    }

    [Fact]
    public void GetById_WithNonExistingId_ShouldReturnNotFound()
    {
        var result = _controller.GetById(999);

        Assert.IsType<NotFoundResult>(result);
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
        var response = Assert.IsType<TodoResponse>(okResult.Value);
        Assert.Equal("Updated", response.Title);
        Assert.True(response.IsCompleted);
    }

    [Fact]
    public void Update_WithNonExistingId_ShouldReturnNotFound()
    {
        var request = new UpdateTodoRequest { Title = "Updated" };

        var result = _controller.Update(999, request);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public void Delete_WithExistingId_ShouldReturnNoContent()
    {
        var created = _service.Create(new Todo { Title = "To delete" });

        var result = _controller.Delete(created.Id);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public void Delete_WithNonExistingId_ShouldReturnNotFound()
    {
        var result = _controller.Delete(999);

        Assert.IsType<NotFoundResult>(result);
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
}
