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
        _controller = new ContactsController() { ControllerContext = controllerContext };
    }

    #region GetContacts

    [Fact]
    public async Task GetContacts_ReturnsOk()
    {
        var contacts = new List<Contact>()
        {
            new Contact() { Id = 1, Name = "John Doe", Address = "123 Main St", Email = "john.doe@contacts.app", Kind = ContactKind.Work },
            new Contact() { Id = 2, Name = "Jane Doe", Address = "456 Main St", Email = "jane.dow@contacts.app", Kind = ContactKind.Personal },
        };
        _repository.GetAllAsync().Returns(new ReturnCodeSuccess<ICollection<Contact>>(contacts));

        var actionResult = await _controller.GetContacts(_repository);

        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var model = Assert.IsAssignableFrom<ICollection<ContactListModel>>(okResult.Value);
        Assert.Equal(2, model.Count());
    }

    [Fact]
    public async Task GetContacts_Exception_ReturnsInternalServerError()
    {
        _repository.GetAllAsync().Returns(new ReturnCodeException(new Exception("Something went wrong")));

        var actionResult = await _controller.GetContacts(_repository);

        var statusCodeResult = Assert.IsType<ObjectResult>(actionResult.Result);
        Assert.Equal(StatusCodes.Status500InternalServerError, statusCodeResult.StatusCode);
        Assert.Equal("Something went wrong", statusCodeResult.Value);
    }

    #endregion
    
    #region GetContact

    [Fact]
    public async Task GetContact_ReturnsOk()
    {
        var contact = new Contact() { Id = 1, Name = "John Doe", Address = "123 Main St", Email = "contact1@contacts.app", Kind = ContactKind.Work };
        _repository.GetByIdAsync(1).Returns(new ReturnCodeSuccess<Contact>(contact));

        var actionResult = await _controller.GetContact(_repository, 1);

        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var model = Assert.IsAssignableFrom<ContactDetailModel>(okResult.Value);
        Assert.Equal(1, model.Id);
    }
    
    [Fact]
    public async Task GetContact_NotFound_ReturnsNotFound()
    {
        _repository.GetByIdAsync(1).Returns(new ReturnCodeNotFound("Contact 1 not found"));

        var actionResult = await _controller.GetContact(_repository, 1);

        var notFoundResult = Assert.IsType<NotFoundObjectResult>(actionResult.Result);
        Assert.Equal("Contact 1 not found", notFoundResult.Value);
    }

    [Fact]
    public async Task GetContact_Exception_ReturnsInternalServerError()
    {
        _repository.GetByIdAsync(1).Returns(new ReturnCodeException(new Exception("Something went wrong")));

        var actionResult = await _controller.GetContact(_repository, 1);
        
        var statusCodeResult = Assert.IsType<ObjectResult>(actionResult.Result);
        Assert.Equal(StatusCodes.Status500InternalServerError, statusCodeResult.StatusCode);
        Assert.Equal("Something went wrong", statusCodeResult.Value);
    }
    
    [Fact]
    public async Task GetContact_FailureDetails_ReturnsBadRequest()
    {
        _repository.GetByIdAsync(1).Returns(new ReturnCodeFailureDetails("Something went wrong", -1));

        var actionResult = await _controller.GetContact(_repository, 1);
        
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult.Result);
        Assert.Equal("Something went wrong", badRequestResult.Value);
    }
    
    [Fact]
    public async Task GetContact_Failure_ReturnsBadRequest()
    {
        _repository.GetByIdAsync(1).Returns(new ReturnCodeFailure());

        var actionResult = await _controller.GetContact(_repository, 1);
        
        var badRequestResult = Assert.IsType<BadRequestResult>(actionResult.Result);
        Assert.Equal(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);
    }
    
    #endregion
}