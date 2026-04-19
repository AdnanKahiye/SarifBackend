namespace Backend.DTOs.Responses.Identity
{
    public class TeamMemberResponses
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string CoverImageUrl { get; set; } = string.Empty;
        public string? Status { get; set; }

        public string? Linkedin { get; set; } = string.Empty;
        public string? Facebook { get; set; } = string.Empty;
        public string? Website { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } 




    }
}
