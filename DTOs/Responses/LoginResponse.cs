namespace Backend.DTOs.Responses
{
    public class LoginResponse
    {

        public string Token { get; set; }
        public string RefreshToken { get; set; }
        public DateTime RefreshTokenExpiryDate { get; set; }

    }
}
