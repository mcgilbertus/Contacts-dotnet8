using Contacts.domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Contacts.data.configuration;

public class ContactConfiguration : IEntityTypeConfiguration<Contact>
{
    public void Configure(EntityTypeBuilder<Contact> builder)
    {
        builder.ToTable("Contacts");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).ValueGeneratedOnAdd();
        builder.Property(c => c.Name).IsRequired()
            .HasMaxLength(50);
        builder.Property(c => c.Direccion).HasMaxLength(100);
        builder.Property(c => c.BirthDate)
            .HasColumnType("date")
            .HasDefaultValue(new DateOnly());
        builder.Property(c => c.Email).HasMaxLength(100);
        builder.Property(c => c.Type)
            .IsRequired()
            .HasConversion<string>()
            .HasDefaultValue(ContactKind.Work);
    }
}