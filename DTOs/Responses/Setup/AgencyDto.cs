namespace Backend.DTOs.Responses.Setup
{
    public class AgencyDto
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;

        public string? Phone { get; set; }
        public string? Address { get; set; }
        public string Country { get; set; } = string.Empty;


        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }

        public string UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
    }
}
