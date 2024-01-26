using Contacts.domain;

namespace Contacts.api.Controllers;

public class ContactListModel
{
    public int Id { get; set; }
    public string Name { get; set; }
    public ContactKind Type { get; set; }
}