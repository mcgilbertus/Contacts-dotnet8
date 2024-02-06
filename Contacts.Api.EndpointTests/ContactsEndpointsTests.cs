using System.Net;
using Contacts.Api.EndpointTests.fixtures;
using Contacts.api.Models;
using Contacts.data;
using Contacts.domain;
using Contacts.Infrastructure.ReturnCodes;
using Contacts.Infrastructure.testing;
using FluentAssertions;

namespace Contacts.Api.EndpointTests;

[Collection("EndpointsCollection")]
public class ContactsEndpointsTests : IClassFixture<TestWebApplicationFactory<Program>>, IAsyncLifetime
{
    private readonly TestWebApplicationFactory<Program> _webAppFactory;

    public ContactsEndpointsTests(TestWebApplicationFactory<Program> webAppFactory)
    {
        _webAppFactory = webAppFactory;
    }

    public async Task InitializeAsync()
    {
        await _webAppFactory.ResetDatabase();
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    #region GetAll

    [Fact]
    public async Task GetAll_ReturnsContacts()
    {
        var client = _webAppFactory.CreateClient();

        var response = await client.GetAsync("/api/contacts");

        response.EnsureSuccessStatusCode();
        var contacts = await response.Content.ReadFromJsonAsync<ICollection<Contact>>();
        contacts.Should().NotBeNullOrEmpty();
        contacts.Count.Should().Be(3);
    }

    [Fact]
    public async Task GetAll_NoDatabase_ReturnsException()
    {
        using var scope = _webAppFactory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ContactsDbContext>();
        await dbContext.Database.EnsureDeletedAsync();

        var client = _webAppFactory.CreateClient();

        var response = await client.GetAsync("/api/contacts");

        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
    }

    #endregion

    #region GetContact

    [Fact]
    public async Task GetContact_ExistingId_ReturnsContact()
    {
        var client = _webAppFactory.CreateClient();

        var response = await client.GetAsync("/api/contacts/1");

        response.EnsureSuccessStatusCode();
        var contact = await response.Content.ReadFromJsonAsync<Contact>();
        contact.Should().NotBeNull();
        contact.Id.Should().Be(1);
    }

    [Fact]
    public async Task GetContact_NonExistingId_ReturnsNotFound()
    {
        var client = _webAppFactory.CreateClient();

        var response = await client.GetAsync("/api/contacts/100");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetContact_NoDatabase_ReturnsException()
    {
        using var scope = _webAppFactory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ContactsDbContext>();
        await dbContext.Database.EnsureDeletedAsync();

        var client = _webAppFactory.CreateClient();

        var response = await client.GetAsync("/api/contacts/1");

        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
    }

    #endregion

    #region AddContact

    [Fact]
    public async Task AddContact_ValidModel_ReturnsCreated()
    {
        var client = _webAppFactory.CreateClient();
        var newContact = new ContactCreateModel() { Name = "Test Contact", Email = "test.contact@contacts.app", Kind = ContactKind.Family };

        var response = await client.PostAsJsonAsync("/api/contacts", newContact);

        response.EnsureSuccessStatusCode();
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();
        var contactDetail = await response.Content.ReadFromJsonAsync<ContactDetailModel>();
        contactDetail.Should().NotBeNull();
        contactDetail.Id.Should().BeGreaterThan(0);
        contactDetail.Name.Should().Be("Test Contact");
    }

    [Fact]
    public async Task AddContact_InvalidModel_ReturnsBadRequest()
    {
        var client = _webAppFactory.CreateClient();
        var newContact = new ContactCreateModel() { Name = "", Kind = ContactKind.Family };

        var response = await client.PostAsJsonAsync("/api/contacts", newContact);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task AddContact_NoDatabase_ReturnsException()
    {
        using var scope = _webAppFactory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ContactsDbContext>();
        await dbContext.Database.EnsureDeletedAsync();

        var client = _webAppFactory.CreateClient();
        var newContact = new ContactCreateModel() { Name = "Test Contact", Kind = ContactKind.Family };

        var response = await client.PostAsJsonAsync("/api/contacts", newContact);

        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
    }

    #endregion
    
    #region UpdateContact

    [Fact]
    public async Task UpdateContact_ExistingId_ReturnsUpdatedContact()
    {
        var client = _webAppFactory.CreateClient();
        var updatedContact = new ContactUpdateModel() { Name = "Updated Contact", Kind = ContactKind.Family };

        var response = await client.PutAsJsonAsync("/api/contacts/1", updatedContact);

        response.EnsureSuccessStatusCode();
        var contactDetail = await response.Content.ReadFromJsonAsync<ContactDetailModel>();
        contactDetail.Should().NotBeNull();
        contactDetail.Id.Should().Be(1);
        contactDetail.Address.Should().BeNull();
        contactDetail.Name.Should().Be("Updated Contact");
    }
    
    [Fact]
    public async Task UpdateContact_NonExistingId_ReturnsNotFound()
    {
        var client = _webAppFactory.CreateClient();
        var updatedContact = new ContactUpdateModel() { Name = "Updated Contact", Kind = ContactKind.Family };

        var response = await client.PutAsJsonAsync("/api/contacts/100", updatedContact);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
    
    [Fact]
    public async Task UpdateContact_InvalidModel_ReturnsBadRequest()
    {
        var client = _webAppFactory.CreateClient();
        var updatedContact = new ContactUpdateModel() { Name = "", Kind = ContactKind.Family };

        var response = await client.PutAsJsonAsync("/api/contacts/1", updatedContact);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Fact]
    public async Task UpdateContact_NoDatabase_ReturnsException()
    {
        using var scope = _webAppFactory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ContactsDbContext>();
        await dbContext.Database.EnsureDeletedAsync();

        var client = _webAppFactory.CreateClient();
        var updatedContact = new ContactUpdateModel() { Name = "Updated Contact", Kind = ContactKind.Family };

        var response = await client.PutAsJsonAsync("/api/contacts/1", updatedContact);

        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
    }
    
    #endregion
    
    #region DeleteContact
    
    [Fact]
    public async Task DeleteContact_ExistingId_ReturnsNoContent()
    {
        var client = _webAppFactory.CreateClient();

        var response = await client.DeleteAsync("/api/contacts/1");

        response.EnsureSuccessStatusCode();
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
    
    [Fact]
    public async Task DeleteContact_NonExistingId_ReturnsNotFound()
    {
        var client = _webAppFactory.CreateClient();

        var response = await client.DeleteAsync("/api/contacts/100");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
    
    [Fact]
    public async Task DeleteContact_NoDatabase_ReturnsException()
    {
        using var scope = _webAppFactory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ContactsDbContext>();
        await dbContext.Database.EnsureDeletedAsync();

        var client = _webAppFactory.CreateClient();

        var response = await client.DeleteAsync("/api/contacts/1");

        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
    }
    
    #endregion
}