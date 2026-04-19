namespace Backend.DTOs.Responses.Subscriptions
{
    public class PlanDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int DurationInDays { get; set; }

        public string UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
    }
}
