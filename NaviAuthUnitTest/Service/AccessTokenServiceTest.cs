using System;
using System.Threading.Tasks;
using Moq;
using NaviAuth.Model.Data;
using NaviAuth.Repository;
using NaviAuth.Service;
using Xunit;

namespace NaviAuthUnitTest.Service;

public class AccessTokenServiceTest
{
    private IAccessTokenService AccessTokenService => new AccessTokenService(_mockAccessTokenRepository.Object);
    private readonly Mock<IAccessTokenRepository> _mockAccessTokenRepository;

    public AccessTokenServiceTest()
    {
        _mockAccessTokenRepository = new Mock<IAccessTokenRepository>();
    }

    [Fact(DisplayName =
        "FindPreviousTokenAsync: FindPreviousTokenAsync should return null if access token for userId is not found.")]
    public async Task Is_FindPreviousTokenAsync_Returns_Null_When_AccessToken_Not_Found()
    {
        // Let
        var userId = "testUserId";
        _mockAccessTokenRepository.Setup(a => a.FindAccessTokenByUserId(userId))
            .ReturnsAsync(value: null);

        // Do
        var result = await AccessTokenService.FindPreviousTokenAsync(userId);

        // Verify
        _mockAccessTokenRepository.VerifyAll();

        // Check
        Assert.Null(result);
    }

    [Fact(DisplayName =
        "FindPreviousTokenAsync: FindPreviousTokenAsync should return corresponding token when access token data exists.")]
    public async Task Is_FindPreviousTokenAsync_Returns_Data_When_AccessToken_Exists()
    {
        // Let
        var accessToken = new AccessToken
        {
            CreatedAt = DateTimeOffset.Now,
            Id = Guid.NewGuid().ToString(),
            UserId = "testUserId"
        };
        _mockAccessTokenRepository.Setup(a => a.FindAccessTokenByUserId(accessToken.UserId))
            .ReturnsAsync(accessToken);

        // Do
        var result = await AccessTokenService.FindPreviousTokenAsync(accessToken.UserId);

        // Verify
        _mockAccessTokenRepository.VerifyAll();

        // Check
        Assert.NotNull(result);
        Assert.Equal(accessToken.Id, result.Token);
    }

    [Fact(DisplayName = "CreateTokenAsync: CreateTokenAsync should create access token correctly.")]
    public async Task Is_CreateTokenAsync_Creates_AccessToken_Correctly()
    {
        // Let
        var userId = "testUserId";
        _mockAccessTokenRepository.Setup(a => a.InsertAccessTokenAsync(It.IsAny<AccessToken>()))
            .Callback((AccessToken accessToken) =>
            {
                Assert.NotNull(accessToken.Id);
                Assert.Equal(userId, accessToken.UserId);
            });

        // Do
        var accessToken = await AccessTokenService.CreateTokenAsync(userId);

        // Verify
        _mockAccessTokenRepository.VerifyAll();

        // Check
        Assert.NotNull(accessToken);
        Assert.NotNull(accessToken.Token);
    }
}