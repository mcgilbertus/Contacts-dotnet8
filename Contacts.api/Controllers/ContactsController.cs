using System.Net;
using Contacts.api.Models;
using Contacts.data.Repositories;
using Contacts.domain;
using Contacts.Infrastructure.ReturnCodes;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace Contacts.api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ContactsController : ControllerBase
{
    private readonly IRepository<Contact> _repository;

    public ContactsController(IRepository<Contact> repository)
    {
        _repository = repository;
    }

    [HttpGet]
    public async Task<ActionResult<ICollection<ContactListModel>>> GetContacts()
    {
        var returnCode = await _repository.GetAllAsync();
        return returnCode switch
        {
            ReturnCodeSuccess<ICollection<Contact>> result => Ok(result.Value.Select(ContactListModel.FromContact).ToList()),
            ReturnCodeException result => StatusCode((int)HttpStatusCode.InternalServerError, result.Exception.Message),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ContactDetailModel>> GetContact(int id)
    {
        var returnCode = await _repository.GetByIdAsync(id);
        return returnCode switch
        {
            ReturnCodeSuccess<Contact> result => Ok(ContactDetailModel.FromContact(result.Value)),
            ReturnCodeNotFound result => NotFound(result.Message ?? "Not found"),
            ReturnCodeFailureDetails result => BadRequest(result.Message ?? "Bad request"),
            ReturnCodeException result => StatusCode((int)HttpStatusCode.InternalServerError, result.Exception.Message),
            ReturnCodeFailure => BadRequest(),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    [HttpPost]
    public async Task<ActionResult<ContactDetailModel>> AddContact(ContactCreateModel model)
    {
        if (!model.IsValid())
            return BadRequest("Invalid model");

        var contact = model.ToContact();
        var returnCode = await _repository.AddAsync(contact);
        return returnCode switch
        {
            ReturnCodeSuccess<Contact> result => CreatedAtAction(nameof(GetContact), new { result.Value.Id }, ContactDetailModel.FromContact(result.Value)),
            ReturnCodeException result => StatusCode((int)HttpStatusCode.InternalServerError, result.Exception.Message),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ContactDetailModel>> UpdateContact(
        [FromServices] IValidator<ContactUpdateModel> validation,
        int id,
        ContactUpdateModel model)
    {
        var valResult = await validation.ValidateAsync(model);
        if (!valResult.IsValid)
            return BadRequest("Invalid model");

        var returnCode = await _repository.UpdateAsync(id, model.ToContact());
        return returnCode switch
        {
            ReturnCodeSuccess<Contact> result => Ok(ContactDetailModel.FromContact(result.Value)),
            ReturnCodeNotFound result => NotFound(result.Message ?? "Not found"),
            ReturnCodeException result => StatusCode((int)HttpStatusCode.InternalServerError, result.Exception.Message),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteContact(int id)
    {
        var returnCode = await _repository.DeleteAsync(id);
        return returnCode switch
        {
            ReturnCodeSuccess result => NoContent(),
            ReturnCodeNotFound result => NotFound(result.Message ?? "Not found"),
            ReturnCodeException result => StatusCode((int)HttpStatusCode.InternalServerError, result.Exception.Message),
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}