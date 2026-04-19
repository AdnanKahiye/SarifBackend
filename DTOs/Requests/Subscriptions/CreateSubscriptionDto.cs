namespace Backend.DTOs.Requests.Subscriptions
{
    public class CreateSubscriptionDto
    {
        public Guid AgencyId { get; set; }
        public Guid PlanId { get; set; }
        public int Status { get; set; } = 1;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
