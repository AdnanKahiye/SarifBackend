using Backend.Models.Setup;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models.Accounts
{
    public class ExchangeSettings : BaseStaticEntity
    {

        public decimal FeeRate { get; set; }
        public decimal ProfitRate { get; set; }

        public int CurrencyId { get; set; }
        [ForeignKey(nameof(CurrencyId))]
        public Currency Currency { get; set; } = null!;


        public Guid AgencyId { get; set; }
        public Guid? BranchId { get; set; }

        [ForeignKey(nameof(AgencyId))]
        public Agency Agency { get; set; } = null!;

        [ForeignKey(nameof(BranchId))]
        public Branch? Branch { get; set; }


    }
}
