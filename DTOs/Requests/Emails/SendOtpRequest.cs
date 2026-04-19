namespace Backend.DTOs.Requests.Emails
{
    public class SendOtpRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Purpose { get; set; } = "EmailVerification";
    }

}
