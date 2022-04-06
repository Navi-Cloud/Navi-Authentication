using NaviAuth.Model.Data;

namespace NaviAuth.Model.Request;

public class RegisterRequest
{
    public string UserEmail { get; set; }
    public string UserPassword { get; set; }

    public User ToUserAccount() => new User
    {
        UserEmail = this.UserEmail,
        UserPassword = BCrypt.Net.BCrypt.HashPassword(this.UserPassword),
    };
}