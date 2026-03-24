using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using TodoApi.Application.DTOs;
using TodoApi.Application.Services;
using TodoApi.Domain.Entities;
using static TodoApi.Application.Constants.TodoMessages;
using static TodoApi.Application.Constants.TodoLogMessages;

namespace TodoApi.Controllers;

[ApiController]
[Route("api/todos")]
[Authorize]
public class TodoController : ControllerBase
{
    private readonly ITodoService _todoService;
    private readonly ILogger<TodoController> _logger;

    public TodoController(ITodoService todoService, ILogger<TodoController> logger)
    {
        _todoService = todoService;
        _logger = logger;
    }

    [HttpPost]
    public IActionResult Create([FromBody] CreateTodoRequest request)
    {
        _logger.LogInformation(TodoApi.Application.Constants.TodoLogMessages.CreatingTodo, request.Title);

        var todo = new Todo
        {
            Title = request.Title,
            Description = request.Description
        };

        var result = _todoService.Create(todo);
        if (result == null)
        {
            _logger.LogWarning(TodoApi.Application.Constants.TodoLogMessages.DuplicateTitle, request.Title);
            return Conflict(new { message = TodoApi.Application.Constants.TodoMessages.DuplicateTitle });
        }

        _logger.LogInformation(TodoApi.Application.Constants.TodoLogMessages.TodoCreated, result.Id);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, MapToResponse(result));
    }

    [HttpGet]
    public IActionResult GetAll([
        FromQuery] string? search,
        [FromQuery] string? sortBy,
        [FromQuery] bool descending = false,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        _logger.LogInformation(TodoApi.Application.Constants.TodoLogMessages.RetrievingTodos, search, sortBy, descending, page, pageSize);
        var todos = _todoService.GetAll(search, sortBy, descending, page, pageSize);
        _logger.LogInformation(TodoApi.Application.Constants.TodoLogMessages.TodosRetrieved, todos.Count);
        return Ok(new { items = todos.Select(MapToResponse).ToList(), total = todos.Count });
    }

    [HttpGet("{id}")]
    public IActionResult GetById(int id)
    {
        _logger.LogInformation(TodoApi.Application.Constants.TodoLogMessages.RetrievingTodo, id);
        var todo = _todoService.GetById(id);
        if (todo == null)
        {
            _logger.LogWarning(TodoApi.Application.Constants.TodoLogMessages.TodoNotFound, id);
            return NotFound(new { message = TodoApi.Application.Constants.TodoMessages.NotFound });
        }

        return Ok(new { todo = MapToResponse(todo) });
    }

    [HttpPut("{id}")]
    public IActionResult Update(int id, [FromBody] UpdateTodoRequest request)
    {
        _logger.LogInformation(TodoApi.Application.Constants.TodoLogMessages.UpdatingTodo, id);

        var todo = new Todo
        {
            Title = request.Title,
            Description = request.Description,
            IsCompleted = request.IsCompleted
        };

        var result = _todoService.Update(id, todo);
        if (result == null)
        {
            _logger.LogWarning(TodoApi.Application.Constants.TodoLogMessages.TodoNotFoundForUpdate, id);
            return NotFound(new { message = TodoApi.Application.Constants.TodoMessages.NotFound });
        }

        _logger.LogInformation(TodoApi.Application.Constants.TodoLogMessages.TodoUpdated, id);
        return Ok(new { message = TodoApi.Application.Constants.TodoMessages.Updated, todo = MapToResponse(result) });
    }

    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        _logger.LogInformation(TodoApi.Application.Constants.TodoLogMessages.DeletingTodo, id);
        var deleted = _todoService.Delete(id);
        if (!deleted)
        {
            _logger.LogWarning(TodoApi.Application.Constants.TodoLogMessages.TodoNotFoundForDelete, id);
            return NotFound(new { message = TodoApi.Application.Constants.TodoMessages.NotFound });
        }

        _logger.LogInformation(TodoApi.Application.Constants.TodoLogMessages.TodoDeleted, id);
        return Ok(new { message = TodoApi.Application.Constants.TodoMessages.Deleted });
    }

    private static TodoResponse MapToResponse(Todo todo)
    {
        return new TodoResponse
        {
            Id = todo.Id,
            Title = todo.Title,
            Description = todo.Description,
            IsCompleted = todo.IsCompleted,
            CreatedAt = todo.CreatedAt
        };
    }
}
