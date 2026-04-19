namespace Backend.DTOs.Requests.Identity
{
    public class GoogleLoginRequest
    {
        public string IdToken { get; set; } = string.Empty;
    }
}
