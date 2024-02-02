using Microsoft.Extensions.Configuration;

namespace Contacts.Data.Tests.fixtures;

public class LocalConfigFixture
{
    public readonly IConfiguration Configuration;

    public LocalConfigFixture()
    {
        var configBuilder = new ConfigurationBuilder();
        configBuilder.AddJsonFile("testsettings.json", optional: false, reloadOnChange: true);
        // Add any configuration settings here
        // configBuilder.AddInMemoryCollection(new Dictionary<string, string>()
            // {{"Section:ConfigKey", "ConfigValue"}});
        Configuration = configBuilder.Build();
    }
}