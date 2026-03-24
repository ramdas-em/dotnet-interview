using TodoApi.Domain.Entities;

namespace TodoApi.Application.Services;

public interface ITodoService
{
    Todo? Create(Todo todo);
    List<Todo> GetAll(string? search = null, string? sortBy = null, bool descending = false, int page = 1, int pageSize = 10);
    Todo? GetById(int id);
    Todo? Update(int id, Todo todo);
    bool Delete(int id);
}
