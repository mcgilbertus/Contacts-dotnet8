using System.Reflection;
using Contacts.data;
using Contacts.domain;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;

namespace Contacts.Api.EndpointTests.fixtures;

public class TestWebApplicationFactory<TProgram>: WebApplicationFactory<TProgram> where TProgram : class
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        var assemblyLocation = Assembly.GetExecutingAssembly().Location;
        var assemblyDirectory = Path.GetDirectoryName(assemblyLocation);
        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddJsonFile(Path.Combine(assemblyDirectory,"testsettings.json"));
        });
        builder.UseEnvironment("Development");
        
        builder.ConfigureServices(services =>
        {
            // Replace the app dbContext with one configured for express DB
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<ContactsDbContext>));
            if (descriptor != null) services.Remove(descriptor);

            services.AddDbContext<ContactsDbContext>(options =>
            {
                options.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=ContactsTestDb;Integrated Security=true;TrustServerCertificate=true");
            });
            // the same technique could be used to replace other services like authentication, etc.
        });
    }

    public async Task ResetDatabase()
    {
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ContactsDbContext>();
        await dbContext.Database.EnsureCreatedAsync();

        // clear the table
        await dbContext.Database.ExecuteSqlRawAsync("truncate table Contacts");
        dbContext.ChangeTracker.Clear();

        // Seed the database with test data
        var contacts = new List<Contact>
        {
            new Contact { Id = 1, Name = "John Doe", Email = "john.doe@contacts.app", Address = "123 Main St", Kind = ContactKind.Work },
            new Contact { Id = 2, Name = "Jane Doe", Email = "jane.doe@contacts.app", Kind = ContactKind.Personal },
            new Contact { Id = 3, Name = "Santa Claus", Email = "santa.claus@northpole.com", Kind = ContactKind.Personal, BirthDate = new DateOnly(1901, 12, 1) }
        };
        // insert the test data
        await using var txn = await dbContext.Database.BeginTransactionAsync();
        await dbContext.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT Contacts ON");
        await dbContext.Contacts.AddRangeAsync(contacts);
        await dbContext.SaveChangesAsync();
        await dbContext.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT Contacts OFF");
        await txn.CommitAsync();
    }
}