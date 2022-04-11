using System.Diagnostics.CodeAnalysis;
using NaviAuth.Model.Data;
using NaviAuth.Model.Internal;
using NaviAuth.Model.Request;
using NaviAuth.Model.Response;
using NaviAuth.Repository;

namespace NaviAuth.Service;

public interface IUserService
{
    Task<InternalCommunication<object>> CreateUserAsync(RegisterRequest registerRequest);
    Task<InternalCommunication<User>> ValidateCredential(LoginRequest loginRequest);
    Task<UserProjection?> GetUserProjectionAsync(string userId);
}

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<InternalCommunication<object>> CreateUserAsync(RegisterRequest registerRequest)
    {
        var previousUser = await _userRepository.GetUserByEmailOrDefaultAsync(registerRequest.UserEmail);
        if (previousUser != null)
        {
            return new InternalCommunication<object>
            {
                ResultType = ResultType.DataConflicts,
                Message = $"User Email {registerRequest.UserEmail} already exists!"
            };
        }

        await _userRepository.InsertUserAsync(registerRequest.ToUserAccount());

        return InternalCommunication<object>.Success(default);
    }

    public async Task<InternalCommunication<User>> ValidateCredential(LoginRequest loginRequest)
    {
        var user = await _userRepository.GetUserByEmailOrDefaultAsync(loginRequest.UserEmail);
        if (user == null)
        {
            return new InternalCommunication<User>
            {
                ResultType = ResultType.DataNotFound,
                Message = "Login failed! Please check email or id."
            };
        }

        if (!CheckPasswordCorrect(loginRequest.UserPassword, user.UserPassword))
        {
            return new InternalCommunication<User>
            {
                ResultType = ResultType.DataNotFound,
                Message = "Login failed! Please check email or id."
            };
        }

        return new InternalCommunication<User>
        {
            ResultType = ResultType.Success,
            TargetObject = user
        };
    }

    public async Task<UserProjection?> GetUserProjectionAsync(string userId)
    {
        return (await _userRepository.GetUserByIdAsync(userId)).ToUserProjection();
    }

    [ExcludeFromCodeCoverage]
    private bool CheckPasswordCorrect(string plainPassword, string hashedPassword)
    {
        bool correct = false;
        try
        {
            correct = BCrypt.Net.BCrypt.Verify(plainPassword, hashedPassword);
        }
        catch
        {
        }

        return correct;
    }
}