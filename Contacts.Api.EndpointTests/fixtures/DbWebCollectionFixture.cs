using Contacts.Infrastructure.testing;

namespace Contacts.Api.EndpointTests.fixtures;

[CollectionDefinition("EndpointsCollection", DisableParallelization = true)]
public class DbWebCollectionFixture : ICollectionFixture<LocalServerFixture>
{
   
}