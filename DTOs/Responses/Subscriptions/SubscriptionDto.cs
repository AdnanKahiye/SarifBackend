namespace Backend.DTOs.Responses.Subscriptions
{
    public class SubscriptionDto
    {
        public string AgencyName { get; set; } = string.Empty;
        public Guid AgencyId { get; set; }

        public string PlanName { get; set; } = string.Empty;
        public Guid PlanId { get; set; }

        public int Status { get; set; } = 1; 
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }


        public string UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
    }
}
