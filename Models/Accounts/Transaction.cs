using Backend.Models.Setup;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models.Accounts
{
    public class Transaction : BaseEntity
    {
        public TransactionTypeEnum TransactionType { get; set; }
        public TransactionStatusEnum Status { get; set; } = TransactionStatusEnum.Completed;

        public string? ReferenceNo { get; set; }
        public string? Description { get; set; }
        public decimal? TotalAmount { get; set; }

        public Guid? AgencyId { get; set; }   // Admin = null or assigned
        public Guid? BranchId { get; set; }   // Staff = must have, Admin = null

        [ForeignKey(nameof(AgencyId))]
        public Agency? Agency { get; set; }

        [ForeignKey(nameof(BranchId))]
        public Branch? Branch { get; set; }
        public ICollection<TransactionDetail> Details { get; set; } = new List<TransactionDetail>();
    }

    public enum TransactionTypeEnum
    {
        Deposit = 1,
        Withdraw = 2,
        Transfer = 3,
        Exchange = 4,
        Loan = 5,
        Repayment = 6,
        Expense = 7,
        Revenue =8
    }

    public enum TransactionStatusEnum
    {
        Pending = 1,
        Completed = 2,
        Cancelled = 3
    }
}
