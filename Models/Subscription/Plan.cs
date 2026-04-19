namespace Backend.Models.Subscription
{
    public class Plan : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }

        public int DurationInDays { get; set; }

        public ICollection<PlanPermission> PlanPermissions { get; set; } = new List<PlanPermission>();
    }
}
