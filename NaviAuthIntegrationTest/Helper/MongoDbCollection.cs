using Xunit;

namespace NaviAuthIntegrationTest.Helper;

[CollectionDefinition("MongoDb")]
public class MongoDbCollection : ICollectionFixture<MongoDbFixture>
{
}