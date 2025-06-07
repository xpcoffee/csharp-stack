namespace Api.Controllers;

using Microsoft.AspNetCore.Mvc;
using Api.Service;
using Api.Model;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly GoogleAuthService _googleAuthService;
    private readonly JwtService _jwtService;

    public AuthController(GoogleAuthService googleAuthService, JwtService jwtService)
    {
        _googleAuthService = googleAuthService;
        _jwtService = jwtService;
    }

    [HttpPost("google")]
    public async Task<IActionResult> GoogleAuth([FromBody] GoogleAuthRequest request)
    {
        if (string.IsNullOrEmpty(request.IdToken))
        {
            return BadRequest("ID token is required");
        }

        var userInfo = await _googleAuthService.ValidateGoogleTokenAsync(request.IdToken);

        if (userInfo == null)
        {
            return Unauthorized("Invalid Google token");
        }

        var jwtToken = _jwtService.GenerateToken(userInfo);

        return Ok(new AuthResponse
        {
            Token = jwtToken,
            Email = userInfo.Email,
            Name = userInfo.Name
        });
    }
}
