using Backend.Interfaces;
using System.Security.Claims;

namespace Backend.Services.Implementations
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string? UserId =>
            _httpContextAccessor.HttpContext?.User?
                .FindFirstValue(ClaimTypes.NameIdentifier);

        public string? Email =>
            _httpContextAccessor.HttpContext?.User?
                .FindFirstValue(ClaimTypes.Email);

        public bool IsInRole(string role)
        {
            var user = _httpContextAccessor.HttpContext?.User;

            if (user == null) return false;

            // ✅ method 1 (default)
            if (user.IsInRole(role)) return true;

            // ✅ method 2 (fallback haddii claim type kala duwan yahay)
            var roles = user.Claims
                .Where(c => c.Type == ClaimTypes.Role || c.Type == "role")
                .Select(c => c.Value);

            return roles.Contains(role);
        }


        public Guid AgencyId =>
    Guid.Parse(
        _httpContextAccessor.HttpContext?.User?
            .FindFirst("AgencyId")?.Value
        ?? Guid.Empty.ToString()
    );

        public Guid? BranchId
        {
            get
            {
                var value = _httpContextAccessor.HttpContext?.User?
                    .FindFirst("BranchId")?.Value;

                if (Guid.TryParse(value, out var branchId))
                {
                    return branchId;
                }

                return null; // ama Guid.Empty haddii aad rabto
            }
        }

        public bool IsAuthenticated =>
            _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
    }

}
