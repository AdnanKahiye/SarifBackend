using Backend.Models.Accounts;

namespace Backend.DTOs.Responses.Accounts
{
    public class RecentTransactionDto
    {
        public string ReferenceNo { get; set; } = string.Empty;

        public TransactionTypeEnum TransactionType { get; set; }
        public TransactionStatusEnum Status { get; set; }

        public string? Username { get; set; }

        public decimal? TotalAmount { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}