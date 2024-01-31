using Contacts.api.Models;
using Contacts.domain;

namespace Contacts.Api.Tests.models;

public class ContactUpdateModelTests
{
    [Fact]
    public void ToContact_WhenValidModel_ShouldReturnContact()
    {
        var model = new ContactUpdateModel
        {
            Name = "John Doe",
            Address = "123 Main St",
            BirthDate = "1983-05-02",
        };
        var expected = new Contact()
        {
            Name = "John Doe",
            Address = "123 Main St",
            BirthDate = new DateOnly(1983, 5, 2),
        };
        
        var result = model.ToContact();
        
        Assert.Equivalent(expected, result);
    }
    
    [Fact]
    public void ToContact_InvalidDate_ShouldThrowException()
    {
        var model = new ContactUpdateModel
        {
            Name = "John Doe",
            Address = "123 Main St",
            BirthDate = "not a date",
        };
        
        var exception = Assert.Throws<FormatException>(() => model.ToContact());
        Assert.Equal("String 'not a date' was not recognized as a valid DateOnly.", exception.Message);
    }
    
    [Fact]
    public void UpdateModel_InvalidModel_ShouldHaveValidationErrors()
    {
        var model = new ContactUpdateModel
        {
            Name = "",
            Address = "123 Main St",
            BirthDate = "not a date",
            Email = "not an email",
        };
        
        var validator = new ContactUpdateModelValidator();
        var valResult = validator.Validate(model);
        Assert.False(valResult.IsValid);
        Assert.Equal(3, valResult.Errors.Count);
        Assert.Equal("'Name' must not be empty.", valResult.Errors[0].ErrorMessage);
        Assert.Equal("The specified condition was not met for 'Birth Date'.", valResult.Errors[1].ErrorMessage);
        Assert.Equal("'Email' is not a valid email address.", valResult.Errors[2].ErrorMessage);
    }
}