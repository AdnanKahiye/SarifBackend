namespace Backend.DTOs.Responses.Setup
{
    public class MenuPermissionDto
    {
        public int Id { get; set; }

        public int MenuId { get; set; }
        public string MenuTitle { get; set; } = string.Empty;

        public int PermissionId { get; set; }
        public string PermissionName { get; set; } = string.Empty;
        public string? ParentTitle { get; set; }   // ✅ ADD THIS


        public int? ParentId { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
    }
}
