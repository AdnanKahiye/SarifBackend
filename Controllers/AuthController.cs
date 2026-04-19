using Azure;
using Backend.DTOs.Requests.Identity;
using Backend.DTOs.Responses;
using Backend.Interfaces;
using Backend.Models.Identity;
using Backend.Utiliy;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly UserManager<ApplicationUser> _userManager;

        public AuthController(IAuthService authService , UserManager<ApplicationUser> userManager)
        {
            _authService = authService;
            _userManager = userManager;
        }












        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> Me()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                return Unauthorized();

                    var permissions = User.Claims
                 .Where(c => c.Type == "permission")
                 .Select(c => c.Value)
                 .ToList();

            return Ok(new
            {
                id = user.Id,
                fullName = user.FirstName,
                email = user.Email,
                phone = user.PhoneNumber,
                gender = user.Gender,
                address = user.Address,
                role = User.FindFirstValue(ClaimTypes.Role),
                permissions = permissions   // ✅ NEW

            });
        }



        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequests request)
        {
            // Call the AuthService to log in the user
            var response = await _authService.LoginAsync(request.UsernameOrEmail, request.Password);

            if (!response.Success)
                return BadRequest(response);  

            // ✅ SET COOKIE HERE for the token (after successful login)
            Response.Cookies.Append("token", response.Data.Token, new CookieOptions
            {
                HttpOnly = true,  
                Secure = true,    
                SameSite = SameSiteMode.None,  
                Path = "/",       
                Expires = DateTimeOffset.UtcNow.AddMinutes(60)  // Expiration time for the cookie
            });

            // ✅ Do NOT return the token to frontend, just a success message
            return Ok(new
            {
                success = true,
                message = "Login successful"
            });
        }


        [HttpPost("logout")]
        public IActionResult Logout()
        {
            Response.Cookies.Append("token", "", new CookieOptions
            {
                HttpOnly = true,
                Secure = true,              // true in production
                SameSite = SameSiteMode.Lax,
                Path = "/",
                Expires = DateTimeOffset.UtcNow.AddMinutes(60)
            });

            return Ok(new { success = true });
        }


        [HttpPost("google-login")]
        public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginRequest request)
        {
            var response = await _authService.GoogleLoginAsync(request.IdToken);

            if (!response.Success)
                return BadRequest(response);

            // ✅ SET COOKIE HERE for the token (after successful login)
            Response.Cookies.Append("token", response.Data.Token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Path = "/",
                Expires = DateTimeOffset.UtcNow.AddMinutes(10)  // Expiration time for the cookie
            });

            // ✅ Do NOT return the token to frontend, just a success message
            return Ok(new
            {
                success = true,
                message = "Login successful"
            });
        }


        [Authorize]
        [Permission("USER.CREATE")]
        [HttpPost("create-user")]
        public IActionResult CreateUser()
        {
            return Ok();
        }



        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            var response = await _authService.GetRefreshTokenAsync(request);

            if (response.Success)
                return Ok(response.Data);

            return BadRequest(response.Message);
        }
    }
}