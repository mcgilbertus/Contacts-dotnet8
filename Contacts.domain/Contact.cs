namespace Contacts.domain;

public class Contact
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string? Address { get; set; }
    public DateOnly? BirthDate { get; set; }
    public string? Email { get; set; }
    public ContactKind Kind { get; set; }
}