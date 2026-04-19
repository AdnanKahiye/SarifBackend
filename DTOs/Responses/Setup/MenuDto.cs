namespace Backend.DTOs.Responses.Setup
{
    public class MenuDto
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;
        public string? Href { get; set; }
        public string? Icon { get; set; }

        public int? ParentId { get; set; }
        public int OrderNo { get; set; }

        public int ModuleId { get; set; }

        // ✅ NEVER NULL
        public List<MenuDto> Children { get; set; } = new();

        // ✅ UI permissions
        public List<string> Permissions { get; set; } = new();
    }
}
