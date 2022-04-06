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

    public UserController(IUserService userService)
    {
        _userService = userService;
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
}