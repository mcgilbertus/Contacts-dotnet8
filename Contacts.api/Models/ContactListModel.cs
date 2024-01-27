using Contacts.domain;

namespace Contacts.api.Models;

public class ContactListModel
{
    public int Id { get; set; }
    public string Name { get; set; }
    public ContactKind Kind { get; set; }

    public static ContactListModel FromContact(Contact contact)
    {
        return new ContactListModel()
        {
            Id = contact.Id,
            Name = contact.Name,
            Kind = contact.Kind
        };
    }
}