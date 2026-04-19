using AutoMapper;
using Backend.DTOs.Requests.Identity;
using Backend.DTOs.Responses;
using Backend.DTOs.Responses.Identity;
using Backend.Interfaces;
using Backend.Models;
using Backend.Models.Identity;
using Backend.Persistence;
using Backend.Utiliy;
using Backend.Wrapper;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Claims;

namespace Backend.Services.Implementations
{
    public class AuthService : CacheService,IAuthService
    {
        private readonly AppDbContext _context;
        private readonly IJwtService _jwtService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly ICurrentUserService _currentUser;
        private readonly R2Service _r2Service;
         private readonly IMapper _mapper;



        private readonly IConfiguration _config;

        public AuthService(
            AppDbContext context,
            IJwtService jwtService,
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            IConfiguration config,
               IMemoryCache cache,
            ICurrentUserService currentUser,
            R2Service r2Service,
              IMapper mapper
            ) : base(cache)
        {
            _context = context;
            _jwtService = jwtService;
            _userManager = userManager;
            _roleManager = roleManager;
            _config = config;
            _currentUser = currentUser;
            _r2Service = r2Service;
            _mapper = mapper;

        }











        public async Task<ResponseWrapper<LoginResponse>> GoogleLoginAsync(string idToken)
        {
            try
            {
                var googleClientId = Environment.GetEnvironmentVariable("GOOGLE_CLIENT_ID");

                var payload = await GoogleJsonWebSignature.ValidateAsync(
                    idToken,
                    new GoogleJsonWebSignature.ValidationSettings
                    {
                        Audience = new[] { googleClientId }
                    });

                var email = payload.Email;

                var user = await _userManager.FindByNameAsync(email);

                if (user == null)
                {
                    Console.WriteLine("USER NOT FOUND, CREATING USER");

                    user = new ApplicationUser
                    {
                        UserName = email,
                        Email = email,
                        EmailConfirmed = true
                    };

                    var createResult = await _userManager.CreateAsync(user);

                    if (!createResult.Succeeded)
                    {
                        foreach (var error in createResult.Errors)
                        {
                            Console.WriteLine(error.Description);
                        }

                        return await ResponseWrapper<LoginResponse>.FailureAsync("User creation failed.");
                    }

                    // ADD DEFAULT ROLE
                    await _userManager.AddToRoleAsync(user, "User");

                    Console.WriteLine("DEFAULT ROLE USER ASSIGNED");
                }

                // GET USER ROLES
                var roles = await _userManager.GetRolesAsync(user);

                var token = await _jwtService.GenerateTokenAsync(user.Id, user.Email, roles);

                Console.WriteLine("JWT TOKEN GENERATED");

                return await ResponseWrapper<LoginResponse>.SuccessAsync(
                    new LoginResponse { Token = token },
                    "Google login successful"
                );
            }
            catch (InvalidJwtException ex)
            {
                Console.WriteLine("INVALID JWT ERROR");
                Console.WriteLine(ex.Message);

                return await ResponseWrapper<LoginResponse>.FailureAsync(
                    "Invalid Google token: " + ex.Message
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine("GENERAL ERROR");
                Console.WriteLine(ex.Message);

                return await ResponseWrapper<LoginResponse>.FailureAsync(
                    "Google login failed: " + ex.Message
                );
            }
        }
        public async Task<ResponseWrapper<LoginResponse>> LoginAsync(string usernameOrEmail, string password)
        {
            // 🔹 Find user
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == usernameOrEmail
                                      || u.UserName == usernameOrEmail);

            if (user == null)
                return await ResponseWrapper<LoginResponse>.FailureAsync("Invalid username/email or password.");

            // 🔹 Check password
            var valid = await _userManager.CheckPasswordAsync(user, password);
            if (!valid)
                return await ResponseWrapper<LoginResponse>.FailureAsync("Invalid username/email or password.");

            // 🔹 Get roles
            var roles = await _userManager.GetRolesAsync(user);
            var roleName = roles.FirstOrDefault();

            // 🔹 Get RoleId
            var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == roleName);

            if (role == null)
                return await ResponseWrapper<LoginResponse>.FailureAsync("User role not found.");

            // 🔹 Get Permissions
            var permissions = await _context.RolePermissions
                .Where(rp => rp.RoleId == role.Id)
                .Select(rp => rp.Permission.KeyName)
                .ToListAsync();

            // 🔹 Create Claims
            var claims = new List<Claim>
{
    new Claim(ClaimTypes.NameIdentifier, user.Id),
    new Claim(ClaimTypes.Email, user.Email ?? ""),
    new Claim(ClaimTypes.Role, roleName ?? ""),

    new Claim("AgencyId", user.AgencyId.ToString()),
    new Claim("BranchId", user.BranchId?.ToString() ?? "")
};

            // 🔥 ADD PERMISSIONS HERE
            claims.AddRange(permissions.Select(p => new Claim("permission", p)));

            // 🔹 Generate Token
            var token = await _jwtService.GenerateTokenWithClaimsAsync(claims);

            // 🔹 Return Response
            return await ResponseWrapper<LoginResponse>.SuccessAsync(
                new LoginResponse { Token = token },
                "Login successfully"
            );
        }

        public async Task<ResponseWrapper<LoginResponse>> GetRefreshTokenAsync(RefreshTokenRequest request)
        {
            // TODO: Implement refresh token logic
            return await ResponseWrapper<LoginResponse>.SuccessAsync(new LoginResponse { Token = "newToken" });
        }
    }
}