namespace Backend.DTOs.Responses.Identity
{
    public class UserResponses
    {
        public string Id { get; set; }
        public string Phone { get; set; }
        public string UserName { get; set; }
        public string Gender { get; set; }
        public bool Isactive { get; set; } = true;
        public string FullName { get; set; }
        public string Address { get; set; }
        public string Email { get; set; }
        public string ProfilePictureUrl { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;

    }
}
