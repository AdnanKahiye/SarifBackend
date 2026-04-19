namespace Backend.DTOs.Responses.Setup
{
    public class RolePermissionDto
    {
        public int Id { get; set; }

        public string RoleId { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;

        public int PermissionId { get; set; }
        public string PermissionName { get; set; } = string.Empty;
        public string PermissionKey { get; set; } = string.Empty;


        public string UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
    }
}
