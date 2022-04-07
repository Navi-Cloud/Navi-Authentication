using System.Security.Cryptography;
using System.Text;
using NaviAuth.Model.Data;
using NaviAuth.Model.Internal;
using NaviAuth.Model.Response;
using NaviAuth.Repository;

namespace NaviAuth.Service;

public interface IAccessTokenService
{
    Task<LoginResponse?> FindPreviousTokenAsync(string userId);
    Task<LoginResponse> CreateTokenAsync(string userId);
}

public class AccessTokenService : IAccessTokenService
{
    private readonly IAccessTokenRepository _accessTokenRepository;

    public AccessTokenService(IAccessTokenRepository accessTokenRepository)
    {
        _accessTokenRepository = accessTokenRepository;
    }

    public async Task<LoginResponse?> FindPreviousTokenAsync(string userId)
    {
        var accessToken = await _accessTokenRepository.FindAccessTokenByUserId(userId);

        if (accessToken == null)
        {
            return null;
        }

        return new LoginResponse
        {
            Token = accessToken.Id
        };
    }

    public async Task<LoginResponse> CreateTokenAsync(string userId)
    {
        using var shaManaged = new SHA512Managed();
        var targetString = $"{DateTime.UtcNow.Ticks}/{userId}/{Guid.NewGuid().ToString()}";
        var targetByte = Encoding.UTF8.GetBytes(targetString);
        var result = shaManaged.ComputeHash(targetByte);

        var accessToken = new AccessToken
        {
            Id = BitConverter.ToString(result).Replace("-", string.Empty),
            CreatedAt = DateTimeOffset.UtcNow,
            UserId = userId
        };

        await _accessTokenRepository.InsertAccessTokenAsync(accessToken);

        return new LoginResponse
        {
            Token = accessToken.Id
        };
    }
}