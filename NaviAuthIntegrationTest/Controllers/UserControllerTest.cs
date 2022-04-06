using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using NaviAuth.Configuration;
using NaviAuth.Model.Request;
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
}