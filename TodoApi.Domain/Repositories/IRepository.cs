using TodoApi.Domain.Entities;

namespace TodoApi.Domain.Repositories;

public interface IRepository<T> where T : class, IEntity
{
    T Create(T entity);
    List<T> GetAll();
    T? GetById(int id);
    bool Update(int id, T entity);
    bool Delete(int id);
}
