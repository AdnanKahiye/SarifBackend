namespace Backend.DTOs.Responses.Setup
{
    public class MenuPermissionGroupedDto
    {
        public int MenuId { get; set; }
        public string MenuTitle { get; set; } = string.Empty;

        public int? ParentId { get; set; }
        public string? ParentTitle { get; set; }

        public List<string> Permissions { get; set; } = new();
        public List<string> PermissionKeys { get; set; } = new();
    }
}
