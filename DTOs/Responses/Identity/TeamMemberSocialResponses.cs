namespace Backend.DTOs.Responses.Identity
{
    public class TeamMemberSocialResponses
    {
        public Guid TeamMemberId { get; set; }

        public string Platform { get; set; } = string.Empty;

        public string Url { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }
}
