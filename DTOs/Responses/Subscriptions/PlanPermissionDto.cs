namespace Backend.DTOs.Responses.Subscriptions
{
    public class PlanPermissionDto
    {
        public Guid Id { get; set; }

        public Guid PlanId { get; set; }
        public string PlanName { get; set; } = string.Empty;

        public int PermissionId { get; set; }
        public string PermissionName { get; set; } = string.Empty;
        public string PermissionKey { get; set; } = string.Empty;


        public string UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
    }
}
