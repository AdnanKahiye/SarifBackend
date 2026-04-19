using Backend.Models.Setup;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models.Accounts
{
    public class Transfer : BaseEntity
    {


    

        [MaxLength(150)]
        public string? SenderName { get; set; }

        [MaxLength(150)]
        public string? ReceiverName { get; set; }

        public TransferStatusEnum Status { get; set; } = TransferStatusEnum.Pending;



        [Required]
        public Guid TransactionId { get; set; }

        [ForeignKey(nameof(TransactionId))]
        public Transaction Transaction { get; set; } = null!;

        [Required]
        public Guid ToAccountId { get; set; }

        [ForeignKey(nameof(ToAccountId))]
        public Account ToAccount { get; set; } = null!;


        [Required]
        public Guid FromAccountId { get; set; }

        [ForeignKey(nameof(FromAccountId))]
        public Account FromAccount { get; set; } = null!;

        public decimal Amount { get; set; }

        public Guid? FromBranchId { get; set; }

        [ForeignKey(nameof(FromBranchId))]
        public Branch? FromBranch { get; set; }

        public Guid? ToBranchId { get; set; }

        [ForeignKey(nameof(ToBranchId))]
        public Branch? ToBranch { get; set; }


        public Guid AgencyId { get; set; }
        public Guid? BranchId { get; set; }

        [ForeignKey(nameof(AgencyId))]
        public Agency Agency { get; set; } = null!;

        [ForeignKey(nameof(BranchId))]
        public Branch? Branch { get; set; }

    }

    public enum TransferStatusEnum
    {
        Pending = 1,
        Completed = 2,
        Cancelled = 3
    }
}
