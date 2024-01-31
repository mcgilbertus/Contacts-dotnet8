using Contacts.domain;
using FluentValidation;

namespace Contacts.api.Models;

public class ContactUpdateModel
{
    public string Name { get; set; }
    public string? Address { get; set; }
    public string? BirthDate { get; set; }
    public string? Email { get; set; }
    public ContactKind Kind { get; set; }
    
    public Contact ToContact()
    {
        return new Contact
        {
            Name = Name,
            Address = Address,
            BirthDate = BirthDate != null ? DateOnly.Parse(BirthDate) : null,
            Email = Email,
            Kind = Kind
        };
    }
}

public class ContactUpdateModelValidator : AbstractValidator<ContactUpdateModel>
{
    public ContactUpdateModelValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.BirthDate).Must(x => DateOnly.TryParse(x, out _)).When(x=>x.BirthDate != null);
        RuleFor(x => x.Email).EmailAddress().When(x => x.Email != null);
    }
}
