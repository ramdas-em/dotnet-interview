using TodoApi.Models;
using TodoApi.Models;

namespace TodoApi.Repositories;

public interface ITodoRepository : IRepository<Todo>
{
    bool ExistsByTitle(string title);
}
