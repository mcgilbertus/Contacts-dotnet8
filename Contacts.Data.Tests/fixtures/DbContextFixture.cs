using Contacts.data;
using Contacts.Infrastructure.testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Contacts.Data.Tests.fixtures;

public class DbContextFixture: IDisposable
{
    private readonly IConfiguration _config;
    public DbContext Context { get; private set; }

    public DbContextFixture()
    {
        var configFixture = new LocalConfigFixture();
        _config = configFixture.Configuration;
        var dbOptions = new DbContextOptionsBuilder<ContactsDbContext>();
        var conString = _config.GetConnectionString("ContactsTestDb");
        dbOptions.UseSqlServer(conString);
        Context = new ContactsDbContext(dbOptions.Options);
    }

    public void Dispose()
    {
        Context?.Dispose();
    }
}