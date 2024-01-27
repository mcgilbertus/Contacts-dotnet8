using Contacts.domain;
using Contacts.Infrastructure.ReturnCodes;
using Microsoft.EntityFrameworkCore;

namespace Contacts.data.Repositories;

public class ContactsRepository : IRepository<Contact>
{
    private readonly ContactsDbContext _ctx;

    public ContactsRepository(ContactsDbContext ctx)
    {
        _ctx = ctx;
    }

    public async Task<ReturnCode> AddAsync(Contact entity)
    {
        try
        {
            await _ctx.Contacts.AddAsync(entity);
            await _ctx.SaveChangesAsync();
            return new ReturnCodeSuccess<Contact>(entity);
        }
        catch (Exception e)
        {
            return new ReturnCodeException(e);
        }
    }

    public async Task<ReturnCode> UpdateAsync(int id, Contact newData)
    {
        var result = await GetByIdAsync(id);
        if (result is not ReturnCodeSuccess<Contact> success)
            return result;

        var contact = success.Value;
        contact.Name = newData.Name;
        contact.Address = newData.Address;
        contact.BirthDate = newData.BirthDate;
        contact.Email = newData.Email;
        contact.Kind = newData.Kind;

        try
        {
            _ctx.Contacts.Update(contact);
            await _ctx.SaveChangesAsync();
            return new ReturnCodeSuccess<Contact>(contact);
        }
        catch (Exception e)
        {
            return new ReturnCodeException(e);
        }
    }

    public async Task<ReturnCode> DeleteAsync(int id)
    {
        var result = await GetByIdAsync(id);
        if (result is not ReturnCodeSuccess<Contact> success)
            return result;

        try
        {
            _ctx.Contacts.Remove(success.Value);
            await _ctx.SaveChangesAsync();
            return new ReturnCodeSuccess();
        }
        catch (Exception e)
        {
            return new ReturnCodeException(e);
        }
    }

    public async Task<ReturnCode> GetByIdAsync(int id)
    {
        try
        {
            var contact = await _ctx.Contacts.FirstOrDefaultAsync(c => c.Id == id);
            if (contact is null)
                return new ReturnCodeNotFound($"Contact {id} not found");
            return new ReturnCodeSuccess<Contact>(contact);
        }
        catch (Exception e)
        {
            return new ReturnCodeException(e);
        }
    }

    public async Task<ReturnCode> GetAllAsync()
    {
        try
        {
            var contacts = await _ctx.Contacts.ToListAsync();
            return new ReturnCodeSuccess<ICollection<Contact>>(contacts);
        }
        catch (Exception e)
        {
            return new ReturnCodeException(e);
        }
    }
}