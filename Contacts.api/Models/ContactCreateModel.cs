using Contacts.domain;

namespace Contacts.api.Models;

public class ContactCreateModel
{
    public string Name { get; set; }
    public string? Address { get; set; }
    public string? BirthDate { get; set; }
    public string? Email { get; set; }
    public ContactKind Kind { get; set; }

    public Contact ToContact()
    {
        if (!IsValid()) throw new InvalidOperationException("Invalid model");

        return new Contact()
        {
            Name = Name,
            Address = Address,
            BirthDate = BirthDate == null ? null : DateOnly.Parse(BirthDate),
            Email = Email,
            Kind = Kind
        };
    }

    public bool IsValid()
    {
        return !string.IsNullOrEmpty(Name) && (BirthDate == null || DateOnly.TryParse(BirthDate, out _));
    }
}