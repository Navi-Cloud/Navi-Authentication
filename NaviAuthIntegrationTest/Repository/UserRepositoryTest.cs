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
public class UserRepositoryTest
{
    private readonly IUserRepository _userRepository;
    private readonly IMongoCollection<User> _testCollection;

    public UserRepositoryTest(MongoDbFixture fixture)
    {
        var mongoContext = fixture.MongoContext;
        _testCollection = mongoContext.UserCollection;
        _userRepository = new UserRepository(mongoContext);
    }

    [Fact(DisplayName = "InsertUserAsync: InsertUserAsync should create one user data.")]
    public async Task Is_InsertUserAsync_Creates_One_User_Data()
    {
        // Let
        var user = new User()
        {
            UserEmail = "test@test.com",
            UserPassword = "testPassword"
        };

        // Do
        await _userRepository.InsertUserAsync(user);

        // Check
        var list = await _testCollection.AsQueryable().ToListAsync();
        Assert.Single(list);
        Assert.Equal(user.UserEmail, list.First().UserEmail);
        Assert.Equal(user.UserPassword, list.First().UserPassword);
    }

    [Fact(DisplayName =
        "GetUserByEmailOrDefaultAsync: GetUserByEmailOrDefaultAsync should return null when data does not exists.")]
    public async Task Is_GetUserByEmailOrDefaultAsync_Should_Returns_Null_When_Data_Not_Exists()
    {
        // Let(N/A)

        // Do
        var result = await _userRepository.GetUserByEmailOrDefaultAsync("testEmail");

        // Check
        Assert.Null(result);
    }

    [Fact(DisplayName =
        "GetUserByEmailOrDefaultAsync: GetUserByEmailOrDefaultAsync should return corresponding data when data exists.")]
    public async Task Is_GetUserByEmailOrDefaultAsync_Returns_Corresponding_Data_When_Data_Exists()
    {
        // Let
        var user = new User()
        {
            UserEmail = "test@test.com",
            UserPassword = "testPassword"
        };
        await _testCollection.InsertOneAsync(user);

        // Do
        var result = await _userRepository.GetUserByEmailOrDefaultAsync(user.UserEmail);

        // Check
        Assert.NotNull(result);
        Assert.Equal(user.UserEmail, result.UserEmail);
        Assert.Equal(user.UserPassword, result.UserPassword);
    }

    [Fact(DisplayName = "GetUserByIdAsync: GetUserByIdAsync should return corresponding data if exists.")]
    public async Task Is_GetUserByIdAsync_Returns_Data_If_Exists()
    {
        // Let
        var user = new User
        {
            Id = ObjectId.Empty.ToString(),
            UserEmail = "testEmail",
            UserPassword = "test"
        };
        await _testCollection.InsertOneAsync(user);

        // Do
        var result = await _userRepository.GetUserByIdAsync(user.Id);

        // Check
        Assert.NotNull(result);
        Assert.Equal(user.Id, result.Id);
        Assert.Equal(user.UserEmail, result.UserEmail);
        Assert.Equal(user.UserPassword, result.UserPassword);
    }

    [Fact(DisplayName = "GetUserByIdAsync: GetUserByIdAsync should return null if data does not exists.")]
    public async Task Is_GetUserByIdAsync_Returns_Null_When_Data_Null()
    {
        // Let
        var userId = ObjectId.Empty.ToString();

        // Do
        var result = await _userRepository.GetUserByIdAsync(userId);

        // Check
        Assert.Null(result);
    }
}