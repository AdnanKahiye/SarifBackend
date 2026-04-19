using Backend.Models.Setup;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models.Accounts
{
    public class ExchangeRate : BaseStaticEntity
    {
        public decimal Rate { get; set; }
        [Required]
        public int CurrencyId { get; set; }

        [ForeignKey(nameof(CurrencyId))]
        public Currency Currency { get; set; } = null!;

        public Guid? AgencyId { get; set; }

        [ForeignKey(nameof(AgencyId))]
        public Agency? Agency { get; set; }

        public Guid? BranchId { get; set; }

        [ForeignKey(nameof(BranchId))]
        public Branch? Branch { get; set; }
    }
}
