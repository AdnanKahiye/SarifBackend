using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

namespace Backend.Interfaces
{
    public interface IJwtService
    {
        /// <summary>
        /// Generates a signed JWT token for a user with claims, roles, and permissions.
        /// </summary>
        /// <param name="userId">The unique ID of the user.</param>
        /// <param name="email">The user's email address.</param>
        /// <param name="roles">List of roles assigned to the user.</param>
        /// <param name="permissions">List of permissions derived from roles.</param>
        /// <returns>A signed JWT token string.</returns>
        Task<string> GenerateTokenAsync(string userId, string email, IList<string> roles);
        TokenValidationParameters GetValidationParametersForExpiredTokens();
        Task<string> GenerateTokenWithClaimsAsync(List<Claim> claims);
    }
}
