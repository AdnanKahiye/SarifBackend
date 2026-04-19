namespace Backend.DTOs.Responses.Setup
{
    public class ModuleDto
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }

        public string UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
    }
}
