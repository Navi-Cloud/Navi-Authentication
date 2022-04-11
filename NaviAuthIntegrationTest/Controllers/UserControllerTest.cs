using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using NaviAuth.Configuration;
using NaviAuth.Model.Request;
using NaviAuth.Model.Response;
using NaviAuthIntegrationTest.Helper;
using Xunit;

namespace NaviAuthIntegrationTest.Controllers;

[Collection("MongoDb")]
public class UserControllerTest
{
    private readonly WebApplicationFactory<Program> _applicationFactory;
    private readonly HttpClient _httpClient;
    private readonly MongoConfiguration _mongoConfiguration;

    public UserControllerTest(MongoDbFixture mongoDbFixture)
    {
        _mongoConfiguration = mongoDbFixture.TestMongoConfiguration;
        _applicationFactory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(service => { service.AddSingleton(_mongoConfiguration); });
            });
        _httpClient = _applicationFactory.CreateClient();
    }

    public async Task<LoginRequest> RegisterSampleUser()
    {
        // Let
        var registerRequest = new RegisterRequest
        {
            UserEmail = "kangdroid@testwhatever.com",
            UserPassword = "helloworld"
        };

        // Do
        var response = await _httpClient.PostAsJsonAsync("/api/user/register", registerRequest);

        // Check
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        return new LoginRequest
        {
            UserEmail = registerRequest.UserEmail,
            UserPassword = registerRequest.UserPassword
        };
    }

    private async Task<LoginResponse> RegisterAndLoginAsync()
    {
        var loginRequest = await RegisterSampleUser();
        
        // Do
        var response = await _httpClient.PostAsJsonAsync("/api/user/login", loginRequest);
        
        // Check
        Assert.True(response.IsSuccessStatusCode);
        
        // Deserialize
        return await response.Content.ReadFromJsonAsync<LoginResponse>();
    }

    [Fact(DisplayName = "POST /api/user/register should return 200 Ok when normal user registered.")]
    public async Task Is_RegisterUser_Returns_200_Ok_When_Registered_Successfully()
    {
        // Let
        var registerRequest = new RegisterRequest
        {
            UserEmail = "kangdroid@testwhatever.com",
            UserPassword = "helloworld"
        };

        // Do
        var response = await _httpClient.PostAsJsonAsync("/api/user/register", registerRequest);

        // Check
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact(DisplayName = "POST /api/user/register should return 409 conflict when same email already registered.")]
    public async Task Is_RegisterUser_Returns_409_Conflict_When_UserEmail_Exists()
    {
        // Let
        await Is_RegisterUser_Returns_200_Ok_When_Registered_Successfully();
        var registerRequest = new RegisterRequest
        {
            UserEmail = "kangdroid@testwhatever.com",
            UserPassword = "helloworld"
        };

        // Do
        var response = await _httpClient.PostAsJsonAsync("/api/user/register", registerRequest);

        // Check
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact(DisplayName = "POST /api/user/login should return 401 when login fails")]
    public async Task Is_LoginUser_Returns_401_When_Login_Fails()
    {
        // Let
        var loginRequest = new LoginRequest
        {
            UserEmail = "adsfadsfasdf@asfddasfadsf.com",
            UserPassword = "testasdf"
        };

        // Do
        var response = await _httpClient.PostAsJsonAsync("/api/user/login", loginRequest);

        // Check
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact(DisplayName = "POST /api/user/login should return 200 OK with access token")]
    public async Task Is_LoginUser_Returns_200_When_Login_Succeed()
    {
        // Let
        var loginRequest = await RegisterSampleUser();
        
        // Do
        var response = await _httpClient.PostAsJsonAsync("/api/user/login", loginRequest);
        
        // Check
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact(DisplayName = "GET /api/user should return 401 unauthorized when access token is not declared.")]
    public async Task Is_GetUserAsync_Returns_Unauthorized_When_AccessToken_Is_Empty()
    {
        // do
        var response = await _httpClient.GetAsync("/api/user");
        
        // Check
        Assert.False(response.IsSuccessStatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact(DisplayName = "GET /api/user should return 200 OK when everything looks normal.")]
    public async Task Is_GetUserAsync_Works_Well()
    {
        // Let
        var loginResponse = await RegisterAndLoginAsync();
        
        // Do
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.Token);
        var response = await _httpClient.GetAsync("/api/user");
        _httpClient.DefaultRequestHeaders.Clear();
        
        // Check
        Assert.True(response.IsSuccessStatusCode);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}