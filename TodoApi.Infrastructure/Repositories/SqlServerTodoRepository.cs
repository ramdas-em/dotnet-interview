using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using TodoApi.Domain.Entities;
using TodoApi.Domain.Repositories;
using TodoApi.Infrastructure.Data;

namespace TodoApi.Infrastructure.Repositories;

public class SqlServerTodoRepository : ITodoRepository
{
    private readonly TodoDbContext _dbContext;

    public SqlServerTodoRepository(TodoDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public bool ExistsByTitle(string title)
    {
        return _dbContext.Todos.Any(t => t.Title == title);
    }

    public Todo Create(Todo todo)
    {
        todo.CreatedAt = DateTime.UtcNow;
        _dbContext.Todos.Add(todo);
        _dbContext.SaveChanges();
        return todo;
    }

    public List<Todo> GetAll(string? search = null, string? sortBy = null, bool descending = false, int page = 1, int pageSize = 10)
    {
        var query = _dbContext.Todos.AsQueryable();

        // Search
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(t => t.Title.Contains(search) || (t.Description != null && t.Description.Contains(search)));
        }

        // Sorting
        if (!string.IsNullOrWhiteSpace(sortBy))
        {
            query = sortBy.ToLower() switch
            {
                "title" => descending ? query.OrderByDescending(t => t.Title) : query.OrderBy(t => t.Title),
                "description" => descending ? query.OrderByDescending(t => t.Description) : query.OrderBy(t => t.Description),
                "iscompleted" => descending ? query.OrderByDescending(t => t.IsCompleted) : query.OrderBy(t => t.IsCompleted),
                "createdat" => descending ? query.OrderByDescending(t => t.CreatedAt) : query.OrderBy(t => t.CreatedAt),
                _ => query.OrderBy(t => t.Id)
            };
        }
        else
        {
            query = query.OrderBy(t => t.Id);
        }

        // Pagination
        query = query.Skip((page - 1) * pageSize).Take(pageSize);

        return query.ToList();
    }

    public Todo? GetById(int id)
    {
        return _dbContext.Todos.Find(id);
    }

    public bool Update(int id, Todo todo)
    {
        var existing = _dbContext.Todos.Find(id);
        if (existing == null) return false;
        existing.Title = todo.Title;
        existing.Description = todo.Description;
        existing.IsCompleted = todo.IsCompleted;
        _dbContext.SaveChanges();
        return true;
    }

    public bool Delete(int id)
    {
        var todo = _dbContext.Todos.Find(id);
        if (todo == null) return false;
        _dbContext.Todos.Remove(todo);
        _dbContext.SaveChanges();
        return true;
    }
}
