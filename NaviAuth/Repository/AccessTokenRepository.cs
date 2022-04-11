using MongoDB.Driver;
using MongoDB.Driver.Linq;
using NaviAuth.Model.Data;

namespace NaviAuth.Repository;

public interface IAccessTokenRepository
{
    Task<AccessToken?> FindAccessTokenByUserId(string userId);
    Task InsertAccessTokenAsync(AccessToken accessToken);
    Task<AccessToken?> FindAccessTokenByTokenAsync(string token);
}

public class AccessTokenRepository : IAccessTokenRepository
{
    private readonly IMongoCollection<AccessToken> _accessTokenCollection;
    private IMongoQueryable<AccessToken> AccessTokenQueryable => _accessTokenCollection.AsQueryable();

    public AccessTokenRepository(MongoContext mongoContext)
    {
        _accessTokenCollection = mongoContext.AccessTokenCollection;
    }

    public async Task<AccessToken?> FindAccessTokenByUserId(string userId)
    {
        return await AccessTokenQueryable.Where(a => a.UserId == userId)
            .OrderBy(a => a.CreatedAt)
            .FirstOrDefaultAsync();
    }

    public async Task<AccessToken?> FindAccessTokenByTokenAsync(string token)
    {
        return await AccessTokenQueryable.Where(a => a.Id == token)
            .FirstOrDefaultAsync();
    }

    public async Task InsertAccessTokenAsync(AccessToken accessToken)
    {
        await _accessTokenCollection.InsertOneAsync(accessToken);
    }
}