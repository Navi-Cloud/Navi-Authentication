using System;
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

    [Fact(DisplayName =
        "ValidateCredential: ValidateCredential should return DataNotFound result when email/id is not found")]
    public async Task Is_ValidateCredential_Returns_DataNotFound_When_Entity_Does_Not_Exists()
    {
        // Let
        var loginRequest = new LoginRequest
        {
            UserEmail = "test@adsfasdfadsf.com",
            UserPassword = "hello"
        };
        _mockUserRepository.Setup(a => a.GetUserByEmailOrDefaultAsync(loginRequest.UserEmail))
            .ReturnsAsync(value: null);

        // Do
        var communicationResult = await UserService.ValidateCredential(loginRequest);

        // Verify
        _mockUserRepository.VerifyAll();

        // Check
        Assert.Equal(ResultType.DataNotFound, communicationResult.ResultType);
    }

    [Fact(DisplayName =
        "ValidateCredential: ValidateCredential should return DataNotFound result when Password does not match.")]
    public async Task Is_ValidateCredential_Returns_DataNotFound_When_Password_Wrong()
    {
        // Let
        var loginRequest = new LoginRequest
        {
            UserEmail = "test@adsfasdfadsf.com",
            UserPassword = "hello"
        };
        _mockUserRepository.Setup(a => a.GetUserByEmailOrDefaultAsync(loginRequest.UserEmail))
            .ReturnsAsync(new User
            {
                Id = Guid.NewGuid().ToString(),
                UserEmail = loginRequest.UserEmail,
                UserPassword = BCrypt.Net.BCrypt.HashPassword("wrong")
            });

        // Do
        var communicationResult = await UserService.ValidateCredential(loginRequest);

        // Verify
        _mockUserRepository.VerifyAll();

        // Check
        Assert.Equal(ResultType.DataNotFound, communicationResult.ResultType);
    }

    [Fact(DisplayName = "ValidateCredential: ValidateCredential should return Success result when login succeed.")]
    public async Task Is_ValidateCredential_Returns_Success_When_Login_Success()
    {
        // Let
        var loginRequest = new LoginRequest
        {
            UserEmail = "test@sadffddsf.com",
            UserPassword = "testhello"
        };
        _mockUserRepository.Setup(a => a.GetUserByEmailOrDefaultAsync(loginRequest.UserEmail))
            .ReturnsAsync(new User
            {
                Id = Guid.NewGuid().ToString(),
                UserEmail = loginRequest.UserEmail,
                UserPassword = BCrypt.Net.BCrypt.HashPassword(loginRequest.UserPassword)
            });

        // Do
        var communicationResult = await UserService.ValidateCredential(loginRequest);

        // Verify
        _mockUserRepository.VerifyAll();

        // Check
        Assert.Equal(ResultType.Success, communicationResult.ResultType);
        Assert.NotNull(communicationResult.TargetObject);
    }

    [Fact(DisplayName = "GetUserProjectionAsync: GetUserProjectionAsync should return user-projection entity.")]
    public async Task Is_GetUserProjectionAsync_Returns_UserProjection_Well()
    {
        // Let
        var user = new User
        {
            Id = "test",
            UserEmail = "test",
            UserPassword = "test"
        };
        _mockUserRepository.Setup(a => a.GetUserByIdAsync(user.Id))
            .ReturnsAsync(value: user);

        // Do
        var result = await UserService.GetUserProjectionAsync(user.Id);

        // Verify
        _mockUserRepository.VerifyAll();

        // Check
        Assert.NotNull(result);
        Assert.Equal(user.Id, result.UserId);
        Assert.Equal(user.UserEmail, result.UserEmail);
    }
}