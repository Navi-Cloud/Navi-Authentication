using System;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using NaviAuth.Model.Data;
using NaviAuth.Repository;
using NaviAuthIntegrationTest.Helper;
using Xunit;

namespace NaviAuthIntegrationTest.Repository;

[Collection("MongoDb")]
public class AccessTokenRepositoryTest
{
    private readonly IAccessTokenRepository _accessTokenRepository;
    private readonly IMongoCollection<AccessToken> _testCollection;

    public AccessTokenRepositoryTest(MongoDbFixture fixture)
    {
        var mongoContext = fixture.MongoContext;
        _testCollection = mongoContext.AccessTokenCollection;
        _accessTokenRepository = new AccessTokenRepository(mongoContext);
    }

    [Fact(DisplayName = "FindAccessTokenByUserId: FindAccessTokenByUserId should return null if data does not exists.")]
    public async Task Is_FindAccessTokenByUserId_Returns_Null_When_Data_Does_Not_Exists()
    {
        // Let(N/A)
        var userId = "testUserId";

        // Do
        var result = await _accessTokenRepository.FindAccessTokenByUserId(userId);

        // Check
        Assert.Null(result);
    }

    [Fact(DisplayName =
        "FindAccessTokenByUserId: FindAccessTokenByUserId should return data if corresponding data exists.")]
    public async Task Is_FindAccessTokenByUserId_Return_Data_If_Data_Exists()
    {
        // Let
        var accessToken = new AccessToken
        {
            UserId = "testUserId",
            CreatedAt = DateTimeOffset.UtcNow,
            Id = "testId"
        };
        await _testCollection.InsertOneAsync(accessToken);

        // Do
        var result = await _accessTokenRepository.FindAccessTokenByUserId(accessToken.UserId);

        // Check
        Assert.NotNull(result);
        Assert.Equal(accessToken.UserId, result.UserId);
        Assert.Equal(accessToken.Id, result.Id);
    }

    [Fact(DisplayName =
        "InsertAccessTokenAsync: InsertAccessTokenAsync should insert corresponding access token correctly.")]
    public async Task Is_InsertAccessTokenAsync_Insert_Works_Well()
    {
        // Let
        var accessToken = new AccessToken
        {
            UserId = "testUserId",
            CreatedAt = DateTimeOffset.UtcNow,
            Id = "testId"
        };

        // Do
        await _accessTokenRepository.InsertAccessTokenAsync(accessToken);

        // Check
        var list = await _testCollection.AsQueryable().ToListAsync();
        Assert.Single(list);
        Assert.Equal(accessToken.UserId, list.First().UserId);
        Assert.Equal(accessToken.Id, list.First().Id);
    }

    [Fact(DisplayName =
        "FindAccessTokenByTokenAsync: FindAccessTokenByTokenAsync should return null if there is no corresponding data.")]
    public async Task Is_FindAccessTokenByTokenAsync_Returns_Null_If_No_Data()
    {
        // Let
        var token = "randomId";

        // Do
        var result = await _accessTokenRepository.FindAccessTokenByTokenAsync(token);

        // Check
        Assert.Null(result);
    }

    [Fact(DisplayName =
        "FindAccessTokenByTokenAsync: FindAccessTokenByTokenAsync should return corresponding data if data exists.")]
    public async Task Is_FindAccessTokenByTokenAsync_Returns_Corresponding_Data_If_Data_Exists()
    {
        // Let
        var accessToken = new AccessToken
        {
            Id = ObjectId.Empty.ToString(),
            CreatedAt = DateTimeOffset.Now,
            UserId = "testUserId"
        };
        await _testCollection.InsertOneAsync(accessToken);

        // Do
        var result = await _accessTokenRepository.FindAccessTokenByTokenAsync(accessToken.Id);

        // Check
        Assert.NotNull(result);
        Assert.Equal(accessToken.Id, result.Id);
        Assert.Equal(accessToken.UserId, result.UserId);
    }
}