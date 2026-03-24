using TodoApi.Domain.Entities;

namespace TodoApi.Domain.Repositories;

public interface ITodoRepository : IRepository<Todo>
{
    bool ExistsByTitle(string title);
    List<Todo> GetAll(string? search = null, string? sortBy = null, bool descending = false, int page = 1, int pageSize = 10);

}
