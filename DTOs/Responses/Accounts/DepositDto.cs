namespace Backend.DTOs.Responses.Accounts
{
    public class DepositDto
    {
        public Guid Id { get; set; }

        public Guid AgencyId { get; set; }
        public Guid BranchId { get; set; }

        public string? DepositNo { get; set; }

        public byte Status { get; set; }

        public Guid AccountId { get; set; }
        public string? AccountName { get; set; }
        public decimal? Amount { get; set; } = 0;


        public Guid CustomerId { get; set; }
        public string? CustomerName { get; set; }

        public DateTime OpenedAt { get; set; }

        public int CurrencyId { get; set; }
        public string? CurrencyCode { get; set; }
        public Guid? TransactionId { get; set; }
        public string TransactionDescription { get; set; }


        public string? UserId { get; set; }
        public string UserName { get; set; } = string.Empty;

    }
}