using System.Threading.Tasks;
using Moq;
using NaviAuth.Model.Data;
using NaviAuth.Model.Internal;
using NaviAuth.Model.Request;
using NaviAuth.Repository;
using NaviAuth.Service;
using Xunit;

namespace NaviAuthUnitTest.Service;

public class UserServiceTest
{
    private IUserService UserService => new UserService(_mockUserRepository.Object);
    private readonly Mock<IUserRepository> _mockUserRepository;

    public UserServiceTest()
    {
        _mockUserRepository = new Mock<IUserRepository>();
    }

    [Fact(DisplayName =
        "CreateUserAsync: CreateUserAsync should return result type of DataConflicts when email already")]
    public async Task Is_CreateUserAsync_Return_DataConflicts_When_Email_Conflicts()
    {
        // Let
        var registerRequest = new RegisterRequest
        {
            UserEmail = "test@test.com",
            UserPassword = "hello"
        };
        _mockUserRepository.Setup(a => a.GetUserByEmailOrDefaultAsync(registerRequest.UserEmail))
            .ReturnsAsync(value: new User());

        // do
        var result = await UserService.CreateUserAsync(registerRequest);

        // Check
        Assert.NotNull(result);
        Assert.Equal(ResultType.DataConflicts, result.ResultType);
    }

    [Fact(DisplayName = "CreateUserAsync: CreateUserAsync should return Success Result when adding data succeeds.")]
    public async Task IS_CreateUserAsync_Return_Success_When_Adding_Data_Succeeds()
    {
        // Let
        var registerRequest = new RegisterRequest
        {
            UserEmail = "test@test.com",
            UserPassword = "hello"
        };
        _mockUserRepository.Setup(a => a.GetUserByEmailOrDefaultAsync(registerRequest.UserEmail))
            .ReturnsAsync(value: null);
        _mockUserRepository.Setup(a => a.InsertUserAsync(It.IsAny<User>()))
            .Callback((User user) =>
            {
                Assert.Equal(registerRequest.UserEmail, user.UserEmail);
                Assert.NotEqual(registerRequest.UserPassword, user.UserPassword);
            });

        // Do
        var result = await UserService.CreateUserAsync(registerRequest);

        // Check
        Assert.NotNull(result);
        Assert.Equal(ResultType.Success, result.ResultType);
    }
}