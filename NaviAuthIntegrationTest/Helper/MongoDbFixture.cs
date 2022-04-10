using System;
using NaviAuth.Configuration;
using NaviAuth.Repository;

namespace NaviAuthIntegrationTest.Helper;

public class MongoDbFixture
{
    public MongoContext MongoContext => new MongoContext(TestMongoConfiguration);

    // Container
    private readonly string _connectionString;

    public MongoConfiguration TestMongoConfiguration => new()
    {
        ConnectionString = _connectionString,
        DatabaseName = Guid.NewGuid().ToString()
    };

    public MongoDbFixture()
    {
        _connectionString = "mongodb://root:testPassword@localhost:27018";
    }
}