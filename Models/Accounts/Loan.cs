using Backend.Models.Customers;
using Backend.Models.Setup;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models.Accounts
{
    public class Loan : BaseEntity
    {


        [MaxLength(100)]
        public string? LoanNo { get; set; }

        [Required]
        public decimal PrincipalAmount { get; set; }

        public decimal InterestRate { get; set; } = 0;
        public decimal PaidAmount { get; set; } = 0;

        public DateTime? StartDate { get; set; }
        public DateTime? DueDate { get; set; }

        public LoanStatusEnum Status { get; set; } = LoanStatusEnum.Active;



        [Required]
        public Guid AccountId { get; set; }

        [ForeignKey(nameof(AccountId))]
        public Account Account { get; set; } = null!;

        [Required]
        public Guid CustomerId { get; set; }

        [ForeignKey(nameof(CustomerId))]
        public Customer Customer { get; set; } = null!;



        public Guid AgencyId { get; set; }

        [ForeignKey(nameof(AgencyId))]
        public Agency Agency { get; set; } = null!;





        public Guid? BranchId { get; set; }

        [ForeignKey(nameof(BranchId))]
        public Branch Branch { get; set; } = null!;



        [Required]
        public Guid TransactionId { get; set; }

        [ForeignKey(nameof(TransactionId))]
        public Transaction Transaction { get; set; } = null!;


        public int CurrencyId { get; set; }

        [ForeignKey(nameof(CurrencyId))]
        public Currency Currency { get; set; } = null!;

    }

    public enum LoanStatusEnum
    {
        Active = 1,
        Closed = 2,
        Defaulted = 3,
        Pending = 0
    }
}
