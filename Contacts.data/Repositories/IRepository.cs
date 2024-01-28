using Contacts.Infrastructure.ReturnCodes;

namespace Contacts.data.Repositories;

public interface IRepository<T>
{
    /// <summary>
    /// Add an entity to the repository
    /// </summary>
    /// <param name="entity"></param>
    /// <returns>ReturnCodeSuccess&lt;T&gt;,</returns>
    /// <returns>ReturnCodeException</returns>
    Task<ReturnCode> AddAsync(T entity);
    
    /// <summary>
    /// Update an existing entity (all fields but the id)
    /// </summary>
    /// <param name="id"></param>
    /// <param name="entity"></param>
    /// <returns>ReturnCodeSuccess&lt;T&gt;,</returns>
    /// <returns>ReturnCodeNotFound,</returns>
    /// <returns>ReturnCodeException</returns>
    Task<ReturnCode> UpdateAsync(int id, T entity);

    /// <summary>
    /// Removes an entity
    /// </summary>
    /// <param name="id"></param>
    /// <returns>ReturnCodeSuccess,</returns>
    /// <returns>ReturnCodeNotFound,</returns>
    /// <returns>ReturnCodeException</returns>
    Task<ReturnCode> DeleteAsync(int id);
    
    /// <summary>
    /// Returns a single entity by id
    /// </summary>
    /// <param name="id"></param>
    /// <returns>ReturnCodeSuccess&lt;T&gt;,</returns>
    /// <returns>ReturnCodeNotFound,</returns>
    /// <returns>ReturnCodeException</returns>
    Task<ReturnCode> GetByIdAsync(int id);
    
    /// <summary>
    /// Returns a list with all entities
    /// </summary>
    /// <returns>ReturnCodeSuccess&lt;ICollection&lt;T&gt;&gt;,</returns>
    /// <returns>ReturnCodeException</returns>
    Task<ReturnCode> GetAllAsync();
}