using Backend.Models.Setup;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models.Accounts
{
    public class LoanPayment : BaseEntity
    {

        [Required]
        public Guid LoanId { get; set; }

        [ForeignKey(nameof(LoanId))]
        public  Loan Loan { get; set; } = null!;

        [Required]
        public decimal Amount { get; set; } // Inta lacag ee hadda la bixiyey

        public DateTime PaymentDate { get; set; } = DateTime.UtcNow;

        public string? Note { get; set; }

        [Required]
        public Guid TransactionId { get; set; } // Link-ga Double-Entry-ga

        [ForeignKey(nameof(TransactionId))]
        public  Transaction Transaction { get; set; } = null!;

        // Ikhtiyaari: Haddii aad rabto inaad raacdo qofka qaaday lacagta
        public Guid AgencyId { get; set; }
        public Guid? BranchId { get; set; }

        [ForeignKey(nameof(AgencyId))]
        public Agency Agency { get; set; } = null!;

        [ForeignKey(nameof(BranchId))]
        public Branch? Branch { get; set; }



        [Required]
        public Guid CashAccountId { get; set; } // Halka lacagtu ku soo dhacday (Debit Account)

        [Required]
        public Guid LoanAccountId { get; set; } // Account-ka deynta ee laga jarayo (Credit Account)

        [ForeignKey(nameof(CashAccountId))]
        public virtual Account CashAccount { get; set; } = null!;

        [ForeignKey(nameof(LoanAccountId))]
        public virtual Account LoanAccount { get; set; } = null!;
    }
}
