using System;
using Io.Github.NaviCloud.Shared;
using Io.Github.NaviCloud.Shared.Authentication;
using NaviAuthService.Model;
using NaviAuthServiceTest.TestHelper;
using Xunit;
using Xunit.Abstractions;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace NaviAuthServiceTest.Services
{
    public class UserServiceTest: IDisposable
    {
        private readonly Authentication.AuthenticationClient _authenticationClient;
        private readonly TestServerFixture _fixture;
        private readonly ITestOutputHelper _testOutputHelper;

        private readonly RegisterRequest _mockRegisterRequest = new()
        {
            UserEmail = "kangdroid@asdfasdfasdfasdfwhatever.com",
            UserPassword = "testPassword@"
        };

        private readonly LoginRequest _mockLoginRequest = new()
        {
            UserEmail = "kangdroid@asdfasdfasdfasdfwhatever.com",
            UserPassword = "testPassword@"
        };
        
        public UserServiceTest(ITestOutputHelper helper)
        {
            _testOutputHelper = helper;
            _fixture = new TestServerFixture();
            _authenticationClient = new Authentication.AuthenticationClient(_fixture.Channel);
        }

        public void Dispose()
        {
            _fixture.Dispose();
        }

        [Fact(DisplayName = "RegisterUser: RegisterUser should register OK when no duplicated errors are found.")]
        public async void Is_RegisterUser_Returns_Success_When_Everything_Is_Ok()
        {
            var result = await _authenticationClient.RegisterUserAsync(_mockRegisterRequest);
            Assert.Equal(ResultType.Success, result.ResultType);
        }

        [Fact(DisplayName =
            "RegisterUser: RegisterUser should return Duplicated error when duplicated users are found.")]
        public async void Is_RegisterUser_Returns_Duplicated()
        {
            // Register Once
            await _authenticationClient.RegisterUserAsync(_mockRegisterRequest);

            var result = await _authenticationClient.RegisterUserAsync(_mockRegisterRequest);
            Assert.Equal(ResultType.Duplicate, result.ResultType);
        }

        [Fact(DisplayName = "LoginUser: LoginUser should return correct access token if field is correct.")]
        public async void Is_LoginUser_Returns_AccessToken_When_Correct_Entity()
        {
            // Register Once
            await _authenticationClient.RegisterUserAsync(_mockRegisterRequest);
            
            // Try to login
            var result = _authenticationClient.LoginUser(_mockLoginRequest);
            
            // Check
            Assert.Equal(ResultType.Success, result.ResultType);
            Assert.NotEmpty(result.Object);
            
            // Debug
            var targetSerialized = JsonSerializer.Deserialize<AccessToken>(result.Object);
            Assert.NotNull(targetSerialized);
            _testOutputHelper.WriteLine(result.Object);
        }

        [Fact(DisplayName = "LoginUser: LoginUser should return Forbidden when incorrect id or password was input.")]
        public async void Is_LoginUser_Returns_Forbidden_Incorrect_Id_Or_Password()
        {
            // Register Once
            await _authenticationClient.RegisterUserAsync(_mockRegisterRequest);
            
            // Try to login
            var result = _authenticationClient.LoginUser(new LoginRequest()
            {
                UserEmail = "SomewhatNot",
                UserPassword = "testPasswordNotCorrect"
            });
            
            // Check
            Assert.Equal(ResultType.Forbidden, result.ResultType);
            
            // ID Correct, but password wrong - case
            result = _authenticationClient.LoginUser(new LoginRequest
            {
                UserEmail = _mockRegisterRequest.UserEmail,
                UserPassword = "testPasswordNotCorrect"
            });
            
            Assert.Equal(ResultType.Forbidden, result.ResultType);
        }

        [Fact(DisplayName =
            "AuthenticateUser: AuthenticateUser should return user email when authentication succeeds.")]
        public async void Is_AuthenticateUser_Returns_UserEmail_When_Succeeds()
        {
            // Register Once
            await _authenticationClient.RegisterUserAsync(_mockRegisterRequest);

            // Try to login
            var token = JsonSerializer.Deserialize<AccessToken>(_authenticationClient.LoginUser(_mockLoginRequest).Object);
            
            // Do
            Assert.NotNull(token);
            var authenticationRequest = new AuthenticationRequest { UserAccessToken = token.Token };
            var result = await _authenticationClient.AuthenticateUserAsync(authenticationRequest);
            
            // Check
            Assert.NotNull(result);
            Assert.Equal(ResultType.Success, result.ResultType);
            Assert.Equal(_mockLoginRequest.UserEmail, result.Object);
        }

        [Fact(DisplayName = "AuthenticateUser: AuthenticateUser should return forbidden when authentication fails.")]
        public async void Is_AuthenticateUser_Returns_Forbidden()
        {

            // Do
            var authenticationRequest = new AuthenticationRequest { UserAccessToken = "token.Token" };
            var result = await _authenticationClient.AuthenticateUserAsync(authenticationRequest);
            
            // Check
            Assert.NotNull(result);
            Assert.Equal(ResultType.Forbidden, result.ResultType);
        }

        [Fact(DisplayName = "RemoveUser: RemoveUser should remove user well.")]
        public async void Is_RemoveUser_Works_Well()
        {
            // Register Once
            await _authenticationClient.RegisterUserAsync(_mockRegisterRequest);
            
            // DO Remove
            var result = await _authenticationClient.RemoveUserAsync(
                new AccountRemovalRequest { UserEmail = _mockRegisterRequest.UserEmail });
            
            // Check
            Assert.NotNull(result);
            Assert.Equal(ResultType.Success, result.ResultType);
        }
    }
}