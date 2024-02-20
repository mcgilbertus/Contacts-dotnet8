using Contacts.data;
using Contacts.data.Repositories;
using Contacts.Data.Respawn.Tests.fixtures;
using Contacts.domain;
using Contacts.Infrastructure.ReturnCodes;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NSubstitute.Extensions;
using ApplicationException = System.ApplicationException;

namespace Contacts.Data.Respawn.Tests;

[Collection("DataCollectionRespawn")]
public class ContactsRepositoryRespawnTests : IAsyncLifetime
{
    private readonly ContactsRepository _repository;
    private readonly LocalServerRespawnFixture _fixture;

    public ContactsRepositoryRespawnTests(LocalServerRespawnFixture fixture)
    {
        _fixture = fixture;
        _repository = new ContactsRepository(fixture.Context);
    }

    public async Task InitializeAsync()
    {
        // clean the database on each run
        await _fixture.ResetDbAsync();
        _fixture.Context.ChangeTracker.Clear();
        // add test data before each test
        await SeedTestDataAsync();
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    private async Task SeedTestDataAsync()
    {
        // Seed test data
        var contacts = new List<Contact>
        {
            new() { Id = 1, Name = "John Doe", Email = "john.doe@contacts.app", Address = "123 Main St", Kind = ContactKind.Work, BirthDate = DateOnly.Parse("1983-05-23") },
            new() { Id = 2, Name = "Jane Doe", Email = "jane.doe@contacts.app", Address = "234 Great Av", Kind = ContactKind.Work },
            new() { Id = 3, Name = "Contact3", Email = "contact3@contacts.app", Kind = ContactKind.Work, BirthDate = DateOnly.Parse("1990-11-02") },
        };
        await using var txn = await _fixture.Context.Database.BeginTransactionAsync();
        await _fixture.Context.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT Contacts ON");
        await _fixture.Context.Contacts.AddRangeAsync(contacts);
        await _fixture.Context.SaveChangesAsync();
        await _fixture.Context.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT Contacts OFF");
        await txn.CommitAsync();
    }

    #region GetAll

    [Fact]
    public async Task GetAll_ReturnsList()
    {
        var result = await _repository.GetAllAsync();
        result.Should().BeOfType<ReturnCodeSuccess<ICollection<Contact>>>();
        var contacts = (result as ReturnCodeSuccess<ICollection<Contact>>)?.Value.ToList();
        contacts.Should().NotBeNull();
        contacts.Should().HaveCount(3);
        contacts[0].Id.Should().Be(1);
        contacts[1].Id.Should().Be(2);
        contacts[2].Id.Should().Be(3);
    }

    [Fact]
    public async Task GetAll_NoDbSet_ReturnsException()
    {
        var ctxMock = new ContactsDbContext(_fixture.DbOptions);
        ctxMock.Contacts = null;
        var newRepo = new ContactsRepository(ctxMock);

        var result = await newRepo.GetAllAsync();

        result.Should().BeOfType<ReturnCodeException>();
    }

    #endregion

    #region GetById

    [Fact]
    public async Task GetById_ExistentId_ReturnsContact()
    {
        var result = await _repository.GetByIdAsync(1);
        result.Should().BeOfType<ReturnCodeSuccess<Contact>>();
        var contact = (result as ReturnCodeSuccess<Contact>)?.Value;
        contact.Should().NotBeNull();
        contact.Id.Should().Be(1);
        contact.Name.Should().Be("John Doe");
    }

    [Fact]
    public async Task GetById_NonExistentId_ReturnsNotFound()
    {
        var result = await _repository.GetByIdAsync(99);
        result.Should().BeOfType<ReturnCodeNotFound>();
    }

    [Fact]
    public async Task GetById_NoDbSet_ReturnsException()
    {
        var ctxMock = new ContactsDbContext(_fixture.DbOptions);
        ctxMock.Contacts = null;
        var newRepo = new ContactsRepository(ctxMock);

        var result = await newRepo.GetByIdAsync(1);

        result.Should().BeOfType<ReturnCodeException>();
    }

    #endregion

    #region Add

    [Fact]
    public async Task Create_ValidContact_ReturnsSuccess()
    {
        var newContact = new Contact { Name = "New Contact", Email = "new.contact@contacts.app", Kind = ContactKind.Work };

        var result = await _repository.AddAsync(newContact);

        result.Should().BeOfType<ReturnCodeSuccess<Contact>>();
        var contact = (result as ReturnCodeSuccess<Contact>)?.Value;
        contact.Should().NotBeNull();
        contact.Id.Should().BeGreaterThan(3);
        contact.Name.Should().Be("New Contact");

        // verify on the database directly
        var name = await _fixture.Context.Database
            .SqlQuery<string>($"SELECT Name as Value FROM Contacts WHERE Id = {contact.Id}")
            .FirstOrDefaultAsync();
        name.Should().Be("New Contact");
    }

    [Fact]
    public async Task Add_ErrorOnSave_ReturnsException()
    {
        var ctxMock = Substitute.ForPartsOf<ContactsDbContext>(_fixture.DbOptions);
        ctxMock.Configure().SaveChangesAsync().ThrowsAsyncForAnyArgs(new ApplicationException("Test Exception"));
        var newRepo = new ContactsRepository(ctxMock);
        var newContact = new Contact { Name = "New Contact", Kind = ContactKind.Family };

        var result = await newRepo.AddAsync(newContact);

        result.Should().BeOfType<ReturnCodeException>();
    }

    #endregion

    #region Update

    [Fact]
    public async Task Update_ValidContact_ReturnsSuccess()
    {
        var updates = new Contact()
        {
            Name = "Updated Name",
            BirthDate = DateOnly.Parse("1999-01-01")
        };

        var updateResult = await _repository.UpdateAsync(1, updates);

        updateResult.Should().BeOfType<ReturnCodeSuccess<Contact>>();
        var updatedContact = (updateResult as ReturnCodeSuccess<Contact>)?.Value;
        updatedContact.Should().NotBeNull();
        updatedContact.Id.Should().Be(1);
        updatedContact.Name.Should().Be("Updated Name");
        updatedContact.BirthDate.Should().Be(DateOnly.Parse("1999-01-01"));
        updatedContact.Address.Should().BeNull();

        // verify on the database directly
        var name = await _fixture.Context.Database
            .SqlQuery<string>($"SELECT Name as Value FROM Contacts WHERE Id = {updatedContact.Id}")
            .FirstOrDefaultAsync();
        name.Should().Be("Updated Name");
    }

    [Fact]
    public async Task Update_ErrorOnSave_ReturnsException()
    {
        var c1 = await _fixture.Context.Contacts.FindAsync(1);
        var ctxMock = Substitute.ForPartsOf<ContactsDbContext>(_fixture.DbOptions);
        ctxMock.Configure().SaveChangesAsync()
            .ThrowsAsyncForAnyArgs(new ApplicationException("Test Exception"));

        var repoMock = Substitute.ForPartsOf<ContactsRepository>(ctxMock);
        repoMock.Configure().GetByIdAsync(default)
            .ReturnsForAnyArgs(new ReturnCodeSuccess<Contact>(c1));

        var updates = new Contact()
        {
            Name = "Updated Name",
            BirthDate = DateOnly.Parse("1999-01-01")
        };

        var result = await repoMock.UpdateAsync(1, updates);

        result.Should().BeOfType<ReturnCodeException>()
            .Which.Exception.Should().BeOfType<ApplicationException>()
            .Which.Message.Should().Be("Test Exception");
    }

    [Fact]
    public async Task Update_NonExistentId_ReturnsNotFound()
    {
        var updates = new Contact()
        {
            Name = "Updated Name",
            BirthDate = DateOnly.Parse("1999-01-01")
        };

        var result = await _repository.UpdateAsync(99, updates);

        result.Should().BeOfType<ReturnCodeNotFound>();
    }

    #endregion

    #region Delete

    [Fact]
    public async Task Delete_ExistentId_ReturnsSuccess()
    {
        var result = await _repository.DeleteAsync(1);
        result.Should().BeOfType<ReturnCodeSuccess>();
        var contacts = (await _repository.GetAllAsync() as ReturnCodeSuccess<ICollection<Contact>>)?.Value.ToList();
        contacts.Should().NotBeNull();
        contacts.Should().HaveCount(2);
        contacts[0].Id.Should().Be(2);
        contacts[1].Id.Should().Be(3);

        // verify on the database directly
        var name = await _fixture.Context.Database.SqlQuery<string>($"SELECT Id as Value FROM Contacts WHERE Id = 1").FirstOrDefaultAsync();
        name.Should().BeNull();
    }

    [Fact]
    public async Task Delete_ErrorOnSave_ReturnsException()
    {
        var c1 = await _fixture.Context.Contacts.FindAsync(1);
        var ctxMock = Substitute.ForPartsOf<ContactsDbContext>(_fixture.DbOptions);
        ctxMock.Configure().SaveChangesAsync().ThrowsAsyncForAnyArgs(new ApplicationException("Test Exception"));

        var repoMock = Substitute.ForPartsOf<ContactsRepository>(ctxMock);
        repoMock.Configure().GetByIdAsync(default).ReturnsForAnyArgs(new ReturnCodeSuccess<Contact>(c1));

        var result = await repoMock.DeleteAsync(1);

        result.Should().BeOfType<ReturnCodeException>();
    }

    [Fact]
    public async Task Delete_NonExistentId_ReturnsNotFound()
    {
        var result = await _repository.DeleteAsync(99);
        result.Should().BeOfType<ReturnCodeNotFound>();
    }

    #endregion
}