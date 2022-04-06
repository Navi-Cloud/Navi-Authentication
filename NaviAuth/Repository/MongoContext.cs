using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using NaviAuth.Configuration;
using NaviAuth.Model.Data;

namespace NaviAuth.Repository;

public class MongoContext
{
    private readonly IMongoClient _mongoClient;
    private readonly IMongoDatabase _mongoDatabase;
    private readonly MongoConfiguration _mongoConfiguration;

    // Public Collection
    public IMongoCollection<User> UserCollection => _mongoDatabase.GetCollection<User>("Users");

    public IMongoCollection<AccessToken> AccessTokenCollection =>
        _mongoDatabase.GetCollection<AccessToken>("AccessTokens");

    public MongoContext(MongoConfiguration mongoConfiguration)
    {
        // Setup MongoDB Naming
        var camelCase = new ConventionPack {new CamelCaseElementNameConvention()};
        ConventionRegistry.Register("CamelCase", camelCase, a => true);

        // Setup Client/Database
        _mongoConfiguration = mongoConfiguration;
        _mongoClient = new MongoClient(_mongoConfiguration.ConnectionString);
        _mongoDatabase = _mongoClient.GetDatabase(_mongoConfiguration.DatabaseName);

        // Create Indexes
        CreateAccountIndexesAsync().Wait();
        CreateAccessTokenIndexesAsync().Wait();
    }

    private async Task CreateAccessTokenIndexesAsync()
    {
        // TTL
        var timeToLiveKey = Builders<AccessToken>.IndexKeys.Ascending("createdAt");
        var ttlIndexModel = new CreateIndexModel<AccessToken>(timeToLiveKey, new CreateIndexOptions
        {
            ExpireAfter = TimeSpan.FromMinutes(30)
        });
        await AccessTokenCollection.Indexes.CreateOneAsync(ttlIndexModel);

        // UserId
        var userIdKey = Builders<AccessToken>.IndexKeys.Ascending("userId");
        await AccessTokenCollection.Indexes.CreateOneAsync(new CreateIndexModel<AccessToken>(userIdKey));
    }

    private async Task CreateAccountIndexesAsync()
    {
        // User Email Index
        var userEmailKey = Builders<User>.IndexKeys.Ascending("userEmail");
        var userEmailIndexModel = new CreateIndexModel<User>(userEmailKey, new CreateIndexOptions
        {
            Unique = true
        });
        await UserCollection.Indexes.CreateOneAsync(userEmailIndexModel);
    }
}