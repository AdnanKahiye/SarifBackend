using Backend.Models.Setup;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models.Accounts
{
    public class Expense : BaseEntity
    {
  

        [Required]
        [MaxLength(150)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(255)]
        public string? Description { get; set; }

        [Required]
        public decimal Amount { get; set; }


        [Required]
        public Guid TransactionId { get; set; }

        [ForeignKey(nameof(TransactionId))]
        public Transaction Transaction { get; set; } = null!;

        [Required]
        public Guid AccountId { get; set; } // Expense account

        [ForeignKey(nameof(AccountId))]
        public Account Account { get; set; } = null!;

        public DateTime ExpenseDate { get; set; } = DateTime.UtcNow;




        public Guid AgencyId { get; set; }

        [ForeignKey(nameof(AgencyId))]
        public Agency Agency { get; set; } = null!;





        public Guid? BranchId { get; set; }

        [ForeignKey(nameof(BranchId))]
        public Branch branchs { get; set; } = null!;
    }
}
