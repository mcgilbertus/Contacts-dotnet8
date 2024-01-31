using Contacts.api.Controllers;
using Contacts.api.Models;
using Contacts.data.Repositories;
using Contacts.domain;
using Contacts.Infrastructure.ReturnCodes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Contacts.Api.Tests.controllers;

public class ContactsControllerTests
{
    private readonly IRepository<Contact> _repository;
    private readonly ContactsController _controller;

    public ContactsControllerTests()
    {
        _repository = Substitute.For<IRepository<Contact>>();

        // set up an empty httpContext to have access to the Request inside the controller
        var httpContext = new DefaultHttpContext();
        var controllerContext = new ControllerContext()
        {
            HttpContext = httpContext,
        };
        _controller = new ContactsController(_repository) { ControllerContext = controllerContext };
    }

    #region GetContacts

    [Fact]
    public async Task GetContacts_ReturnsOk()
    {
        // Arrange
        var contacts = new List<Contact>()
        {
            new()
            {
                Id = 1, Name = "John Doe", Address = "123 Main St",
                Email = "john.doe@contacts.app", Kind = ContactKind.Work
            },
            new()
            {
                Id = 2, Name = "Jane Doe", Address = "456 Main St",
                Email = "jane.dow@contacts.app", Kind = ContactKind.Personal
            },
        };
        _repository.GetAllAsync().Returns(new ReturnCodeSuccess<ICollection<Contact>>(contacts));

        var expected = new List<ContactListModel>()
        {
            new() { Id = 1, Name = "John Doe", Kind = ContactKind.Work },
            new() { Id = 2, Name = "Jane Doe", Kind = ContactKind.Personal }
        };

        // Act
        var actionResult = await _controller.GetContacts();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var model = Assert.IsAssignableFrom<ICollection<ContactListModel>>(okResult.Value);
        Assert.Equivalent(expected, model);
    }

    [Fact]
    public async Task GetContacts_Exception_ReturnsInternalServerError()
    {
        _repository.GetAllAsync().Returns(new ReturnCodeException(new Exception("Something went wrong")));

        var actionResult = await _controller.GetContacts();

        var statusCodeResult = Assert.IsType<ObjectResult>(actionResult.Result);
        Assert.Equal(StatusCodes.Status500InternalServerError, statusCodeResult.StatusCode);
        Assert.Equal("Something went wrong", statusCodeResult.Value);
    }

    [Fact]
    public async Task GetContacts_UnexpectedResultFromRepository_Throws()
    {
        _repository.GetAllAsync().Returns(new ReturnCodeUnexpected());

        var actionResult = async () => await _controller.GetContacts();

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(actionResult);
    }

    #endregion

    #region GetContact

    [Fact]
    public async Task GetContact_ReturnsOk()
    {
        var contact = new Contact() { Id = 1, Name = "John Doe", Address = "123 Main St", Email = "contact1@contacts.app", Kind = ContactKind.Work };
        _repository.GetByIdAsync(1).Returns(new ReturnCodeSuccess<Contact>(contact));

        var actionResult = await _controller.GetContact(1);

        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var model = Assert.IsAssignableFrom<ContactDetailModel>(okResult.Value);
        Assert.Equal(1, model.Id);
    }

    [Fact]
    public async Task GetContact_NotFound_ReturnsNotFound()
    {
        _repository.GetByIdAsync(1).Returns(new ReturnCodeNotFound("Contact 1 not found"));

        var actionResult = await _controller.GetContact(1);

        var notFoundResult = Assert.IsType<NotFoundObjectResult>(actionResult.Result);
        Assert.Equal("Contact 1 not found", notFoundResult.Value);
    }

    [Fact]
    public async Task GetContact_Exception_ReturnsInternalServerError()
    {
        _repository.GetByIdAsync(1).Returns(new ReturnCodeException(new Exception("Something went wrong")));

        var actionResult = await _controller.GetContact(1);

        var statusCodeResult = Assert.IsType<ObjectResult>(actionResult.Result);
        Assert.Equal(StatusCodes.Status500InternalServerError, statusCodeResult.StatusCode);
        Assert.Equal("Something went wrong", statusCodeResult.Value);
    }

    [Fact]
    public async Task GetContact_FailureDetails_ReturnsBadRequest()
    {
        _repository.GetByIdAsync(1).Returns(new ReturnCodeFailureDetails("Something went wrong", -1));

        var actionResult = await _controller.GetContact(1);

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult.Result);
        Assert.Equal("Something went wrong", badRequestResult.Value);
    }

    [Fact]
    public async Task GetContact_Failure_ReturnsBadRequest()
    {
        _repository.GetByIdAsync(1).Returns(new ReturnCodeFailure());

        var actionResult = await _controller.GetContact(1);

        var badRequestResult = Assert.IsType<BadRequestResult>(actionResult.Result);
        Assert.Equal(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);
    }

    [Fact]
    public async Task GetContact_UnexpectedResultFromRepository_Throws()
    {
        _repository.GetByIdAsync(1).Returns(new ReturnCodeUnexpected());

        var actionResult = async () => await _controller.GetContact(1);

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(actionResult);
    }

    #endregion

    #region AddContact

    [Fact]
    public async Task AddContact_DataOk_ReturnsCreated()
    {
        var contact = new Contact()
            { Id = 1, Name = "John Doe", Address = "123 Main St", Email = "john.doe@contacts.app", Kind = ContactKind.Work, BirthDate = new DateOnly(1969, 1, 19) };
        _repository.AddAsync(Arg.Any<Contact>()).Returns(new ReturnCodeSuccess<Contact>(contact));

        var payload = new ContactCreateModel()
        {
            Name = "John Doe", Address = "123 Main St", Email = "john.doe@contacts.app",
            Kind = ContactKind.Work, BirthDate = "1969-01-19"
        };

        var expected = new ContactDetailModel()
        {
            Id = 1, Name = "John Doe", Address = "123 Main St", Email = "john.doe@contacts.app",
            Kind = ContactKind.Work, BirthDate = new DateOnly(1969, 1, 19)
        };

        var actionResult = await _controller.AddContact(payload);

        var createdResult = Assert.IsType<CreatedAtActionResult>(actionResult.Result);
        var model = Assert.IsAssignableFrom<ContactDetailModel>(createdResult.Value);
        Assert.Equivalent(expected, model);
    }

    [Fact]
    public async Task AddContact_Exception_ReturnsInternalServerError()
    {
        _repository.AddAsync(Arg.Any<Contact>()).Returns(new ReturnCodeException(new Exception("Something went wrong")));

        var payload = new ContactCreateModel()
        {
            Name = "John Doe", Kind = ContactKind.Family
        };

        var actionResult = await _controller.AddContact(payload);

        var statusCodeResult = Assert.IsType<ObjectResult>(actionResult.Result);
        Assert.Equal(StatusCodes.Status500InternalServerError, statusCodeResult.StatusCode);
        Assert.Equal("Something went wrong", statusCodeResult.Value);
    }

    [Fact]
    public async Task AddContact_BadPayload_ReturnsBadRequest()
    {
        // missing Name
        var payload = new ContactCreateModel()
        {
            Kind = ContactKind.Family
        };

        var actionResult = await _controller.AddContact(payload);

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult.Result);
        Assert.Equal("Invalid model", badRequestResult.Value);
    }

    [Fact]
    public async Task AddContact_UnexpectedResultFromRepository_Throws()
    {
        _repository.GetByIdAsync(1).Returns(new ReturnCodeUnexpected());

        var actionResult = async () => await _controller.AddContact(
            new ContactCreateModel() { Name = "John Doe", Kind = ContactKind.Family });

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(actionResult);
    }

    #endregion

    #region UpdateContact

    [Fact]
    public async Task UpdateContact_DataOk_ReturnsOk()
    {
        var contact = new Contact()
        {
            Id = 1, Name = "John Doe", Address = "123 Main St",
            Email = "contact1@contacts.app", Kind = ContactKind.Work
        };
        _repository.UpdateAsync(1, Arg.Any<Contact>()).Returns(new ReturnCodeSuccess<Contact>(contact));

        var payload = new ContactUpdateModel()
        {
            Name = "John Doe", Address = "123 Main St",
            Email = "contact1@contacts.app", Kind = ContactKind.Work
        };

        var actionResult = await _controller.UpdateContact(new ContactUpdateModelValidator(), 1, payload);

        var expected = new ContactDetailModel()
        {
            Id = 1, Name = "John Doe", Address = "123 Main St",
            Email = "contact1@contacts.app", Kind = ContactKind.Work
        };
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var model = Assert.IsAssignableFrom<ContactDetailModel>(okResult.Value);
        Assert.Equivalent(expected, model);
    }

    [Fact]
    public async Task UpdateContact_InvalidModel_ReturnsBadRequest()
    {
        var payload = new ContactUpdateModel()
        {
            Name = "", Address = "123 Main St",
            Email = "contact1@contacts.app", Kind = ContactKind.Work
        };

        var actionResult = await _controller.UpdateContact(new ContactUpdateModelValidator(), 1, payload);

        Assert.IsType<BadRequestObjectResult>(actionResult.Result);
    }

    [Fact]
    public async Task UpdateContact_ContactNotPresent_ReturnsNotFound()
    {
        _repository.UpdateAsync(99, Arg.Any<Contact>()).Returns(new ReturnCodeNotFound("Contact 2 not found"));
        var payload = new ContactUpdateModel()
        {
            Name = "John Doe", Kind = ContactKind.Work
        };

        var actionResult = await _controller.UpdateContact(new ContactUpdateModelValidator(), 99, payload);

        var notFoundResult = Assert.IsType<NotFoundObjectResult>(actionResult.Result);
        Assert.Equal("Contact 2 not found", notFoundResult.Value);
    }
    
    [Fact]
    public async Task UpdateContact_Exception_ReturnsInternalServerError()
    {
        _repository.UpdateAsync(Arg.Any<int>(), Arg.Any<Contact>())
            .Returns(new ReturnCodeException(new Exception("Something went wrong")));

        var payload = new ContactUpdateModel() { Name = "John Doe", Kind = ContactKind.Family };

        var actionResult = await _controller.UpdateContact(new ContactUpdateModelValidator(), 1, payload);

        var statusCodeResult = Assert.IsType<ObjectResult>(actionResult.Result);
        Assert.Equal(StatusCodes.Status500InternalServerError, statusCodeResult.StatusCode);
        Assert.Equal("Something went wrong", statusCodeResult.Value);
    }
    
    [Fact]
    public async Task UpdateContact_UnexpectedResultFromRepository_Throws()
    {
        _repository.UpdateAsync(Arg.Any<int>(), Arg.Any<Contact>()).Returns(new ReturnCodeUnexpected());

        var payload = new ContactUpdateModel() { Name = "John Doe", Kind = ContactKind.Family };

        var actionResult = async () => 
            await _controller.UpdateContact(new ContactUpdateModelValidator(), 1, payload);

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(actionResult);
    }

    #endregion
    
    #region DeleteContact
    
    [Fact]
    public async Task DeleteContact_ExistingId_ReturnsNoContent()
    {
        _repository.DeleteAsync(1).Returns(new ReturnCodeSuccess());

        var actionResult = await _controller.DeleteContact(1);

        Assert.IsType<NoContentResult>(actionResult);
    }
    
    [Fact]
    public async Task DeleteContact_NonExistantContact_ReturnsNotFound()
    {
        _repository.DeleteAsync(99).Returns(new ReturnCodeNotFound("Contact 99 not found"));

        var actionResult = await _controller.DeleteContact(99);

        var notFoundResult = Assert.IsType<NotFoundObjectResult>(actionResult);
        Assert.Equal("Contact 99 not found", notFoundResult.Value);
    }
       
    [Fact]
    public async Task DeleteContact_Exception_ReturnsInternalServerError()
    {
        _repository.DeleteAsync(1).Returns(new ReturnCodeException(new Exception("Something went wrong")));

        var actionResult = await _controller.DeleteContact(1);

        var statusCodeResult = Assert.IsType<ObjectResult>(actionResult);
        Assert.Equal(StatusCodes.Status500InternalServerError, statusCodeResult.StatusCode);
        Assert.Equal("Something went wrong", statusCodeResult.Value);
    }
    
    [Fact]
    public async Task DeleteContact_UnexpectedResultFromRepository_Throws()
    {
        _repository.DeleteAsync(1).Returns(new ReturnCodeUnexpected());

        var actionResult = async () => await _controller.DeleteContact(1);

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(actionResult);
    }
    
    #endregion
}