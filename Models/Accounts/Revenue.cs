using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models.Accounts
{
    public class Revenue : BaseEntity
    {
        public string Title { get; set; } = string.Empty; // Tusaale: "Service Fee", "Commission"
        public string? Description { get; set; }

        [Required]
        public decimal Amount { get; set; }

        [Required]
        public RevenueSourceEnum SourceType { get; set; } // Halkan u beddel Enum

        [Required]
        public Guid RevenueAccountId { get; set; } // Account-ka Revenue-ka (Type 7)

        [Required]
        public Guid CashAccountId { get; set; } // Khasnadda lacagtu ku dhacday (Type 1)

        [ForeignKey(nameof(RevenueAccountId))]
        public virtual Account RevenueAccount { get; set; } = null!;

        [ForeignKey(nameof(CashAccountId))]
        public virtual Account CashAccount { get; set; } = null!;

        public Guid TransactionId { get; set; }
        [ForeignKey(nameof(TransactionId))]
        public virtual Transaction Transaction { get; set; } = null!;

        public Guid AgencyId { get; set; }
        public Guid? BranchId { get; set; }
    }

    public enum RevenueSourceEnum
    {
        Direct = 1,    // Dakhli toos ah (Service Fees, Commissions)
        Exchange = 2,  // Dakhli ka yimid Profit-ka Sarifka
        Transfer = 3,  // Dakhli ka yimid Khidmadda Xawaaladda
        LoanInterest = 4 // Dakhli ka yimid Ribada ama Khidmadda Deynta
    }
}
