using Contacts.Infrastructure.ReturnCodes;

namespace Contacts.data.Repositories;

public interface IRepository<T>
{
    Task<ReturnCode> AddAsync(T entity);
    Task<ReturnCode> UpdateAsync(int id, T entity);
    Task<ReturnCode> DeleteAsync(int id);
    Task<ReturnCode> GetByIdAsync(int id);
    Task<ReturnCode> GetAllAsync();
}