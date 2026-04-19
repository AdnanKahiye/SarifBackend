namespace Backend.DTOs.Requests.Emails
{
    public class VerifyOtpRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string Purpose { get; set; } = "EmailVerification";
    }
}
