namespace Backend.DTOs.Responses.Setup
{
    public class BranchDto
    {
     public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;

        public string? Location { get; set; }
        public bool IsMain { get; set; }

        public Guid AgencyId { get; set; }
        public string AgencyName { get; set; } = string.Empty;


        public string UserId { get; set; }
        public string UserName { get; set; } = string.Empty;

        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
