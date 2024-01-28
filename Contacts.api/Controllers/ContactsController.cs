using System.Net;
using Contacts.api.Models;
using Contacts.data.Repositories;
using Contacts.domain;
using Contacts.Infrastructure.ReturnCodes;
using Microsoft.AspNetCore.Mvc;

namespace Contacts.api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ContactsController : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ICollection<ContactListModel>>> GetContacts([FromServices] IRepository<Contact> repository)
    {
        var returnCode = await repository.GetAllAsync();
        return returnCode switch
        {
            ReturnCodeSuccess<ICollection<Contact>> result => Ok(result.Value.Select(ContactListModel.FromContact).ToList()),
            ReturnCodeException result => StatusCode((int)HttpStatusCode.InternalServerError, result.Exception.Message)
        };
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ContactDetailModel>> GetContact([FromServices] IRepository<Contact> repository, int id)
    {
        var returnCode = await repository.GetByIdAsync(id);
        return returnCode switch
        {
            ReturnCodeSuccess<Contact> result => Ok(ContactDetailModel.FromContact(result.Value)),
            ReturnCodeNotFound result => NotFound(result.Message ?? "Not found"),
            ReturnCodeFailureDetails result => BadRequest(result.Message ?? "Bad request"),
            ReturnCodeException result => StatusCode((int)HttpStatusCode.InternalServerError, result.Exception.Message),
            ReturnCodeFailure => BadRequest()
        };
    }

    [HttpPost]
    public async Task<ActionResult<ContactDetailModel>> AddContact([FromServices] IRepository<Contact> repository, ContactCreateModel model)
    {
        var returnCode = await repository.AddAsync(model.ToContact());
        return returnCode switch
        {
            ReturnCodeSuccess<Contact> result => CreatedAtAction(nameof(GetContact), new { result.Value.Id }, ContactDetailModel.FromContact(result.Value)),
            ReturnCodeException result => BadRequest(result.Exception.Message),
            _ => BadRequest()
        };
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ContactDetailModel>> UpdateContact([FromServices] IRepository<Contact> repository, int id, ContactUpdateModel model)
    {
        var returnCode = await repository.UpdateAsync(id, model.ToContact());
        return returnCode switch
        {
            ReturnCodeSuccess<Contact> result => Ok(ContactDetailModel.FromContact(result.Value)),
            ReturnCodeNotFound result => NotFound(result.Message ?? "Not found"),
            ReturnCodeException result => BadRequest(result.Exception.Message),
            _ => BadRequest()
        };
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteContact([FromServices] IRepository<Contact> repository, int id)
    {
        var returnCode = await repository.DeleteAsync(id);
        return returnCode switch
        {
            ReturnCodeSuccess result => NoContent(),
            ReturnCodeNotFound result => NotFound(result.Message ?? "Not found"),
            ReturnCodeException result => BadRequest(result.Exception.Message),
            _ => BadRequest()
        };
    }
}