using MongoDB.Driver;
using MongoDB.Driver.Linq;
using NaviAuth.Model.Data;

namespace NaviAuth.Repository;

public interface IUserRepository
{
    Task InsertUserAsync(User user);
    Task<User?> GetUserByEmailOrDefaultAsync(string email);
}

public class UserRepository : IUserRepository
{
    private readonly IMongoCollection<User> _userCollection;
    private IMongoQueryable<User> UserQueryable => _userCollection.AsQueryable();

    public UserRepository(MongoContext mongoContext)
    {
        _userCollection = mongoContext.UserCollection;
    }

    public async Task InsertUserAsync(User user)
    {
        await _userCollection.InsertOneAsync(user);
    }

    public async Task<User?> GetUserByEmailOrDefaultAsync(string email)
    {
        return await UserQueryable.FirstOrDefaultAsync(a => a.UserEmail == email);
    }
}