using Contacts.data;
using Contacts.Infrastructure.testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Contacts.Data.Respawn.Tests.fixtures;

public class LocalServerRespawnFixture : IAsyncLifetime
{
    private readonly IConfiguration _config;
    private readonly string _connString;
    public ContactsDbContext Context { get; set; }
    public DbContextOptions<ContactsDbContext> DbOptions { get; set; }

    public LocalServerRespawnFixture()
    {
        var configFixture = new LocalConfigFixture();
        _config = configFixture.Configuration;
        _connString = _config.GetConnectionString("ContactsTestDb")!;

        var dbOptions = new DbContextOptionsBuilder<ContactsDbContext>();
        dbOptions.UseSqlite(_connString);
        DbOptions = dbOptions.Options;
        Context = new ContactsDbContext(DbOptions);
    }

    public async Task InitializeAsync()
    {
        await Context.Database.EnsureDeletedAsync();
        await Context.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        await Context.Database.EnsureDeletedAsync();
    }

    public async Task ResetDbAsync()
    {
        await Context.Database.ExecuteSqlRawAsync("DELETE FROM Contacts");
        Context.ChangeTracker.Clear();
    }
}
