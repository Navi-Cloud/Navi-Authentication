using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Mvc;
using NaviAuth.Model.Internal;
using NaviAuth.Model.Request;
using NaviAuth.Model.Response;
using NaviAuth.Service;

namespace NaviAuth.Controllers;

[ExcludeFromCodeCoverage]
[Route("/api/user")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IAccessTokenService _accessTokenService;

    public UserController(IUserService userService, IAccessTokenService accessTokenService)
    {
        _userService = userService;
        _accessTokenService = accessTokenService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> RegisterUserAsync(RegisterRequest registerRequest)
    {
        var result = await _userService.CreateUserAsync(registerRequest);

        return result.ResultType switch
        {
            ResultType.Success => Ok(),
            ResultType.DataConflicts => Conflict(new ErrorResponse
            {
                StatusCode = StatusCodes.Status409Conflict,
                Message = "Email Already Exists!",
                DetailedMessage = result.Message
            }),
            _ => new StatusCodeResult(StatusCodes.Status500InternalServerError)
        };
    }

    [HttpPost("login")]
    public async Task<IActionResult> LoginUserAsync(LoginRequest loginRequest)
    {
        var credentialValidation = await _userService.ValidateCredential(loginRequest);
        if (credentialValidation.ResultType != ResultType.Success)
        {
            return Unauthorized(new ErrorResponse
            {
                StatusCode = StatusCodes.Status401Unauthorized,
                Message = "Login Failed!",
                DetailedMessage = credentialValidation.Message
            });
        }
        
        // Make sure user exists.
        var user = credentialValidation.TargetObject ??
                   throw new NullReferenceException("Validating credential succeed but TargetObject was null.");

        // Token
        var token = (await _accessTokenService.FindPreviousTokenAsync(user.Id))
                    ?? (await _accessTokenService.CreateTokenAsync(user.Id));
        return Ok(token);
    }
}