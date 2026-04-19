using Backend.DTOs.Requests.Identity;
using Backend.DTOs.Responses;
using Backend.DTOs.Responses.Identity;
using Backend.Models;
using Backend.Wrapper;

namespace Backend.Interfaces
{
    public interface IAuthService
    {
        Task<ResponseWrapper<LoginResponse>> LoginAsync(string usernameOrEmail, string password);
        Task<ResponseWrapper<LoginResponse>> GetRefreshTokenAsync(RefreshTokenRequest request);
        public Task<ResponseWrapper<LoginResponse>> GoogleLoginAsync(string idToken);







       

    }
}