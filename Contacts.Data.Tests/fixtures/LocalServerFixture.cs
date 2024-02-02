using System.Diagnostics;

namespace Contacts.Data.Tests.fixtures;

public class LocalServerFixture: IDisposable
{
    public LocalServerFixture()
    {
        // starts the localDB process
        using var process = Process.Start("sqllocaldb", "start MSSQLLocalDB");
        process.WaitForExit();
    }

    public void Dispose()
    {
        // Don't leave LocalDB process running (fix test runner warning)
        using var process = Process.Start("sqllocaldb", "stop MSSQLLocalDB");
        process.WaitForExit();
    }
}