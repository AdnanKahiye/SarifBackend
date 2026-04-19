namespace Backend.DTOs.Requests.Identity
{
    public class CreateRoleRequest
    {
        public string RoleName { get; set; }
        public string? Description { get; set; }
    }
}
