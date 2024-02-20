using System.Diagnostics;
using Contacts.data;
using Contacts.Infrastructure.testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Respawn;

namespace Contacts.Data.Respawn.Tests.fixtures;

public class LocalServerRespawnFixture : IAsyncLifetime
{
    private readonly IConfiguration _config;
    private Respawner _spawner;
    private readonly string _connString;
    public ContactsDbContext Context { get; set; }
    public DbContextOptions<ContactsDbContext> DbOptions { get; set; }

    public LocalServerRespawnFixture()
    {
        // starts the localDB process
        using var process = Process.Start("sqllocaldb", "start MSSQLLocalDB");
        process.WaitForExit();

        var configFixture = new LocalConfigFixture();
        _config = configFixture.Configuration;
        _connString = _config.GetConnectionString("ContactsTestDb");

        var dbOptions = new DbContextOptionsBuilder<ContactsDbContext>();
        dbOptions.UseSqlServer(_connString);
        DbOptions = dbOptions.Options;
        Context = new ContactsDbContext(DbOptions);
    }

    public async Task InitializeAsync()
    {
        // delete and recreate the database only once for the entire collection of test classes
        await Context.Database.EnsureDeletedAsync();
        await Context.Database.EnsureCreatedAsync();

        // create the respawner instance. It will read the db metadata and construct the graph of dependencies
        _spawner = await Respawner.CreateAsync(_connString,
            new RespawnerOptions
            {
                // TablesToIgnore = new Table[] {"someTable"},
                SchemasToInclude = new[] { "dbo" }
            });
    }

    public async Task DisposeAsync()
    {
        await Context.Database.EnsureDeletedAsync();
        // Don't leave LocalDB process running (fix test runner warning)
        using var process = Process.Start("sqllocaldb", "stop MSSQLLocalDB");
        await process.WaitForExitAsync();
    }

    public async Task ResetDbAsync()
    {
        // delete data in all tables included in the configuration
        await _spawner.ResetAsync(_connString);
    }
}