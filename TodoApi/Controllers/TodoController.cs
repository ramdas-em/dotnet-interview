using Microsoft.AspNetCore.Mvc;
using TodoApi.Application.DTOs;
using TodoApi.Application.Services;
using TodoApi.Domain.Entities;

namespace TodoApi.Controllers;

[ApiController]
[Route("api/todos")]
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
        _logger.LogInformation("Creating todo with title: {Title}", request.Title);

        var todo = new Todo
        {
            Title = request.Title,
            Description = request.Description
        };

        var result = _todoService.Create(todo);
        if (result == null)
        {
            _logger.LogWarning("Duplicate todo title detected: {Title}", request.Title);
            return Conflict(new { message = $"A todo with the title '{request.Title}' already exists." });
        }

        _logger.LogInformation("Todo created successfully with Id: {Id}", result.Id);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, MapToResponse(result));
    }

    [HttpGet]
    public IActionResult GetAll()
    {
        _logger.LogInformation("Retrieving all todos");
        var todos = _todoService.GetAll();
        _logger.LogInformation("Retrieved {Count} todos", todos.Count);
        return Ok(todos.Select(MapToResponse));
    }

    [HttpGet("{id}")]
    public IActionResult GetById(int id)
    {
        _logger.LogInformation("Retrieving todo with Id: {Id}", id);
        var todo = _todoService.GetById(id);
        if (todo == null)
        {
            _logger.LogWarning("Todo not found with Id: {Id}", id);
            return NotFound();
        }

        return Ok(MapToResponse(todo));
    }

    [HttpPut("{id}")]
    public IActionResult Update(int id, [FromBody] UpdateTodoRequest request)
    {
        _logger.LogInformation("Updating todo with Id: {Id}", id);

        var todo = new Todo
        {
            Title = request.Title,
            Description = request.Description,
            IsCompleted = request.IsCompleted
        };

        var result = _todoService.Update(id, todo);
        if (result == null)
        {
            _logger.LogWarning("Todo not found for update with Id: {Id}", id);
            return NotFound();
        }

        _logger.LogInformation("Todo updated successfully with Id: {Id}", id);
        return Ok(MapToResponse(result));
    }

    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        _logger.LogInformation("Deleting todo with Id: {Id}", id);
        var deleted = _todoService.Delete(id);
        if (!deleted)
        {
            _logger.LogWarning("Todo not found for deletion with Id: {Id}", id);
            return NotFound();
        }

        _logger.LogInformation("Todo deleted successfully with Id: {Id}", id);
        return NoContent();
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
