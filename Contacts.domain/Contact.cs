namespace Contacts.domain;

public class Contact
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string? Direccion { get; set; }
    public DateOnly? BirthDate { get; set; }
    public string? Email { get; set; }
    public ContactKind Type { get; set; }
}