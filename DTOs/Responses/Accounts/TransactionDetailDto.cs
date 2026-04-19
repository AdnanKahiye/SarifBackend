namespace Backend.DTOs.Responses.Accounts
{
    public class TransactionDetailDto
    {
        public Guid Id { get; set; }

        public Guid AccountId { get; set; }
        public string? AccountName { get; set; }

        public string? ReferenceNo { get; set; }

        public string? Description { get; set; }

        public decimal Amount { get; set; }

        public byte EntryType { get; set; }

        public int CurrencyId { get; set; }
        public string? CurrencyCode { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
