using MCBank.WebApi.Application.DTOs;
using MCBank.WebApi.Application.Interfaces;
using MCBank.WebApi.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace MCBank.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var result = await authService.RegisterAsync(request.Username, request.Password);
        if (result.IsFailure)
            return result.ToActionResult();

        SetTokenCookies(
            result.Value.AccessToken,
            result.Value.RefreshToken,
            result.Value.AccessTokenExpiresIn,
            result.Value.RefreshTokenExpiresIn);

        return Ok();
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await authService.LoginAsync(request.Username, request.Password);
        if (result.IsFailure)
            return result.ToActionResult();

        SetTokenCookies(
            result.Value.AccessToken,
            result.Value.RefreshToken,
            result.Value.AccessTokenExpiresIn,
            result.Value.RefreshTokenExpiresIn);

        return Ok();
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh()
    {
        var refreshToken = Request.Cookies["refreshToken"];
        if (string.IsNullOrWhiteSpace(refreshToken))
            return Unauthorized();

        var result = await authService.RefreshAsync(refreshToken);
        if (result.IsFailure)
            return result.ToActionResult();

        SetTokenCookies(
            result.Value.AccessToken,
            result.Value.RefreshToken,
            result.Value.AccessTokenExpiresIn,
            result.Value.RefreshTokenExpiresIn);

        return Ok();
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var refreshToken = Request.Cookies["refreshToken"];
        if (!string.IsNullOrWhiteSpace(refreshToken))
        {
            await authService.RevokeTokenAsync(refreshToken);
        }
        
        var options = new CookieOptions
        {
            HttpOnly = true,
            Secure = false,
            SameSite = SameSiteMode.Lax,
            Path = "/"
        };
        
        Response.Cookies.Delete("accessToken", options);
        Response.Cookies.Delete("refreshToken", options);
        
        return Ok();
    }

    private void SetTokenCookies(string accessToken, string refreshToken, int accessExpMinutes, int refreshExpMinutes)
    {
        var baseOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = false,
            SameSite = SameSiteMode.Lax
        };

        Response.Cookies.Append("accessToken", accessToken, new CookieOptions
        {
            HttpOnly = baseOptions.HttpOnly,
            Secure = baseOptions.Secure,
            SameSite = baseOptions.SameSite,
            Expires = DateTime.UtcNow.AddMinutes(accessExpMinutes)
        });

        Response.Cookies.Append("refreshToken", refreshToken, new CookieOptions
        {
            HttpOnly = baseOptions.HttpOnly,
            Secure = baseOptions.Secure,
            SameSite = baseOptions.SameSite,
            Expires = DateTime.UtcNow.AddMinutes(refreshExpMinutes)
        });
    }
}