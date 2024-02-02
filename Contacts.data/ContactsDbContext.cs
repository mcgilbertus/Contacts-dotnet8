using Contacts.domain;
using Microsoft.EntityFrameworkCore;

namespace Contacts.data;

public class ContactsDbContext: DbContext
{
    public ContactsDbContext(DbContextOptions options) : base(options)
    {
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // applies configuration for all types that implement IEntityTypeConfiguration interface
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ContactsDbContext).Assembly);
    }

    public DbSet<Contact> Contacts { get; set; }
}