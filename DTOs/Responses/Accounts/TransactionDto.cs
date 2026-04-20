using Backend.Models.Accounts;

namespace Backend.DTOs.Responses.Accounts
{
    public class TransactionDto
    {
        public Guid Id { get; set; }

        public TransactionTypeEnum TransactionType { get; set; }

        public TransactionStatusEnum Status { get; set; }

        public string? ReferenceNo { get; set; }

        public string? Description { get; set; }

        public decimal? TotalAmount { get; set; }

        public Guid? AgencyId { get; set; }

        public Guid? BranchId { get; set; }

        public string? AgencyName { get; set; }

        public string? BranchName { get; set; }

        public string? UserId { get; set; }
        public string UserName { get; set; } = string.Empty;

        public List<TransactionDetailDto> Details { get; set; } = new();
    }
}
