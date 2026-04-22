namespace Backend.DTOs.Responses.Accounts
{
    public class ExchangeDto
    {
        public Guid Id { get; set; }

        public decimal Rate { get; set; }

        public decimal FromAmount { get; set; }
        public decimal ToAmount { get; set; }

        public decimal Fee { get; set; }
        public decimal Profit { get; set; }

        public Guid TransactionId { get; set; }
        public string Reference { get; set; }

        public Guid FromAccountId { get; set; }
        public string? FromAccountName { get; set; }

        public Guid ToAccountId { get; set; }
        public string? ToAccountName { get; set; }
        public DateTime CreatedAt { get; set; }

        public string? UserId { get; set; }
        public string UserName { get; set; } = string.Empty;

    }
}
