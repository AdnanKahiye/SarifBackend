namespace Backend.DTOs.Requests.Identity
{
    public class UpdateUserRequest
    {
        public string? UserId { get; set; }
        public string ? Email { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public string? UserName { get; set; }
        public bool? Isactive { get; set; } = true;
        public string ?FullName { get; set; }
        public string ?Gender { get; set; }
        public string ?Role { get; set; } = "User";
        public string? NewPassword { get; set; }
    }
}
