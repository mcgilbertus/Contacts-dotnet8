using Contacts.Infrastructure.testing;

namespace Contacts.Data.Tests.fixtures;

[CollectionDefinition("DataCollection", DisableParallelization = true)]
public class DbDataCollectionFixture : ICollectionFixture<LocalServerFixture>
{
   
}