using TodoApi.Domain.Entities;

namespace TodoApi.Domain.Repositories;

public interface ITodoRepository : IRepository<Todo>
{
    bool ExistsByTitle(string title);
}
