namespace Backend.DTOs.Responses.Accounts
{
    public class TransferDto
    {
        public Guid Id { get; set; }

        public string? SenderName { get; set; }
        public string? ReceiverName { get; set; }
        public decimal Amount { get; set; }


        public int Status { get; set; }

        public Guid TransactionId { get; set; }

        public Guid FromAccountId { get; set; }
        public string? FromAccountName { get; set; }

        public Guid ToAccountId { get; set; }
        public string? ToAccountName { get; set; }

        public Guid? FromBranchId { get; set; }
        public string? FromBranchName { get; set; }

        public Guid? ToBranchId { get; set; }
        public string? ToBranchName { get; set; }
        public DateTime CreatedAt { get; set; }



        public string? UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
    }
}
