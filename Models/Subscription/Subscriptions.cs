using Backend.Models.Setup;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models.Subscription
{
    public class Subscriptions : BaseEntity
    {


        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }


        [Required]
        public Guid AgencyId { get; set; }

        [ForeignKey(nameof(AgencyId))]
        public Agency Agency { get; set; } = null!;

        public SubscriptionStatus Status { get; set; }

        [Required]
        public Guid PlanId { get; set; }

        [ForeignKey(nameof(PlanId))]
        public Plan Plan { get; set; } = null!;

    }

    public enum SubscriptionStatus
    {
        Active = 1,
        Expired = 2,
        Cancelled = 3
    }
}

