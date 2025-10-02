using AuthApp.Models;
using AuthApp.Services;
using Microsoft.AspNetCore.Mvc;
using BCrypt.Net;
using AuthApp.Config;

namespace AuthApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AuthServices _authServices;

        public AuthController(AuthServices authServices)
        {
            _authServices = authServices;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(Signup user)
        {
            var existing = await _authServices.GetByEmailAsync(user.email);
            if (existing != null) return BadRequest(new { message = "Email already exists" });

            user.password = BCrypt.Net.BCrypt.HashPassword(user.password);

            await _authServices.AddUser(user);
            return Ok(new { message = "User registered successfully" });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(Login loginData)
        {
            var user = await _authServices.GetByEmailAsync(loginData.email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(loginData.password, user.password))
                return Unauthorized(new { message = "Invalid credentials" });

            var jwtToken = _authServices.GenerateToken(user);
            var refreshToken = await _authServices.GenerateRefreshToken(user.Id);

            // Set refresh token in HTTP-only cookie
            Response.Cookies.Append("refreshToken", refreshToken.Token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = refreshToken.Expires
            });

            Response.Cookies.Append("JwtToken", jwtToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = refreshToken.Expires
            });



            // Return JWT in response body
            return Ok(new { token = jwtToken, message = "Login successful" });
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken()
        {
            var oldRefreshToken = Request.Cookies["refreshToken"];
            if (string.IsNullOrEmpty(oldRefreshToken))
                return Unauthorized(new { message = "No refresh token provided" });

            try
            {
                var (newJwt, newRefreshToken) = await _authServices.RefreshJwtTokenAsync(oldRefreshToken);

                // Update refresh token cookie
                Response.Cookies.Append("refreshToken", newRefreshToken.Token, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = newRefreshToken.Expires
                });

                return Ok(new { token = newJwt });
            }
            catch
            {
                return Unauthorized(new { message = "Invalid refresh token" });
            }
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            Response.Cookies.Delete("refreshToken");
            return Ok(new { message = "Logged out successfully" });
        }
    }
}
