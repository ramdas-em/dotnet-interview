using TodoApi.Domain.Entities;

namespace TodoApi.Domain.Repositories;

public interface IRepository<T> where T : class, IEntity
{
    T Create(T entity);
    T? GetById(int id);
    bool Update(int id, T entity);
    bool Delete(int id);
}
