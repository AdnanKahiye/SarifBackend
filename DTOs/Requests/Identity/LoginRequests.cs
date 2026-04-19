namespace Backend.DTOs.Requests.Identity
{
    public class LoginRequests
    {
        public string UsernameOrEmail { get; set; }=null;
        public string Password { get; set; } = null;
    }
}
