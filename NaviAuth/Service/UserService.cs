using NaviAuth.Model.Data;
using NaviAuth.Model.Internal;
using NaviAuth.Model.Request;
using NaviAuth.Repository;

namespace NaviAuth.Service;

public interface IUserService
{
    Task<InternalCommunication<object>> CreateUserAsync(RegisterRequest registerRequest);
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
}