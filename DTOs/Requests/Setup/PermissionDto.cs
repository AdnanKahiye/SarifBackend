namespace Backend.DTOs.Requests.Setup
{
    public class PermissionDto
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;
        public string KeyName { get; set; } = string.Empty;


        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; } = string.Empty;



    }
}
