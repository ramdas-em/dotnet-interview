using TodoApi.Domain.Entities;

namespace TodoApi.Application.Services;

public interface ITodoService
{
    Todo? Create(Todo todo);
    List<Todo> GetAll();
    Todo? GetById(int id);
    Todo? Update(int id, Todo todo);
    bool Delete(int id);
}
