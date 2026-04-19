namespace Backend.DTOs.Requests.Subscriptions
{
    public class UpdatePlanDto
    {
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int DurationInDays { get; set; }
    }
}
