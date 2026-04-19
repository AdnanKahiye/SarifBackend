namespace Backend.DTOs.Requests.Identity
{
    public class CreateUserRequest
    {
        public string Password { get; set; }
        public string Phone { get; set; }
        public string? UserName { get; set; }
        public string Gender { get; set; } = string.Empty;
        public bool Isactive { get; set; } = true;
        public string? FullName { get; set; }
        public string? Address { get; set; } = "Somalia";
        public string Email { get; set; }
        public string Role { get; set; } = "User";
    }
}
