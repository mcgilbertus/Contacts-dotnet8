using Contacts.domain;

namespace Contacts.api.Models;

public class ContactDetailModel
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string? Address { get; set; }
    public DateOnly? BirthDate { get; set; }
    public string? Email { get; set; }
    public ContactKind Kind { get; set; }

    public static ContactDetailModel FromContact(Contact contact)
    {
        return new ContactDetailModel()
        {
            Id = contact.Id,
            Name = contact.Name,
            Address = contact.Address,
            BirthDate = contact.BirthDate,
            Email = contact.Email,
            Kind = contact.Kind
        };
    }
}