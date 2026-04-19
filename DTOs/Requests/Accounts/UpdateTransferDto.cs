using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs.Requests.Accounts
{
    public class UpdateTransferDto
    {


        public string? SenderName { get; set; }
        public string? ReceiverName { get; set; }
        public decimal Amount { get; set; }

        public int Status { get; set; }

        [Required]
        public Guid FromAccountId { get; set; }

        [Required]
        public Guid ToAccountId { get; set; }

        public Guid? FromBranchId { get; set; }
        public Guid? ToBranchId { get; set; }
    }
}
