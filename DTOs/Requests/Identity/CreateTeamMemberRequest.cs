namespace Backend.DTOs.Requests.Identity
{
    public class CreateTeamMemberRequest
    {
        public Guid? Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Status { get; set; } = "Pending";

        public string? Linkedin { get; set; } = string.Empty;
        public string? Facebook { get; set; } = string.Empty;
        public string? Website { get; set; } = string.Empty;


        public IFormFile? CoverImage { get; set; }
    }
}
