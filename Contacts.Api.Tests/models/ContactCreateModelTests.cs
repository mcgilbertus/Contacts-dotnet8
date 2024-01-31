using Contacts.api.Models;
using Contacts.domain;

namespace Contacts.Api.Tests.models;

public class ContactCreateModelTests
{
    [Fact]
    public void ToContact_WhenValidModel_ShouldReturnContact()
    {
        var model = new ContactCreateModel
        {
            Name = "John Doe",
            Address = "123 Main St",
            BirthDate = "1983-05-02",
            Email = "john.doe@contacts.app",
            Kind = ContactKind.Personal
        };
        var expected = new Contact()
        {
            Name = "John Doe",
            Address = "123 Main St",
            BirthDate = new DateOnly(1983, 5, 2),
            Email = "john.doe@contacts.app",
            Kind = ContactKind.Personal
        };

        var result = model.ToContact();

        Assert.Equivalent(expected, result);
    }

    [Fact]
    public void ToContact_WhenInvalidModel_ShouldThrowException()
    {
        var model = new ContactCreateModel
        {
            Name = "John Doe",
            Address = "123 Main St",
            BirthDate = "not a date",
        };

        var exception = Assert.Throws<InvalidOperationException>(() => model.ToContact());
        Assert.Equal("Invalid model", exception.Message);
    }
}