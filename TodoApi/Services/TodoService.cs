using TodoApi.Models;
using TodoApi.Models;
using TodoApi.Repositories;

namespace TodoApi.Services;

public class TodoService : ITodoService
{
    private readonly ITodoRepository _repository;
    private readonly ILogger<TodoService> _logger;

    public TodoService(ITodoRepository repository, ILogger<TodoService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public Todo? Create(Todo todo)
    {
        _logger.LogInformation("Checking for duplicate title: {Title}", todo.Title);
        if (_repository.ExistsByTitle(todo.Title))
        {
            _logger.LogWarning("Todo with title '{Title}' already exists. Skipping creation.", todo.Title);
            return null;
        }

        _logger.LogInformation("Creating new todo with title: {Title}", todo.Title);
        return _repository.Create(todo);
    }

    public List<Todo> GetAll()
    {
        _logger.LogInformation("Fetching all todos from repository");
        return _repository.GetAll();
    }

    public Todo? GetById(int id)
    {
        _logger.LogInformation("Fetching todo with Id: {Id}", id);
        return _repository.GetById(id);
    }

    public Todo? Update(int id, Todo todo)
    {
        _logger.LogInformation("Attempting to update todo with Id: {Id}", id);
        var existing = _repository.GetById(id);
        if (existing == null)
        {
            _logger.LogWarning("Todo with Id: {Id} not found for update", id);
            return null;
        }

        existing.Title = todo.Title;
        existing.Description = todo.Description;
        existing.IsCompleted = todo.IsCompleted;

        _repository.Update(id, existing);
        _logger.LogInformation("Todo with Id: {Id} updated successfully", id);
        return existing;
    }

    public bool Delete(int id)
    {
        _logger.LogInformation("Attempting to delete todo with Id: {Id}", id);
        var result = _repository.Delete(id);
        if (!result)
        {
            _logger.LogWarning("Todo with Id: {Id} not found for deletion", id);
        }
        else
        {
            _logger.LogInformation("Todo with Id: {Id} deleted successfully", id);
        }
        return result;
    }
}
