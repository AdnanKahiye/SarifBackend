namespace Backend.DTOs.Requests.Accounts
{
    public class CashOpeningDto
    {
        public Guid CashAccountId { get; set; }
        public Guid CapitalAccountId { get; set; }
        public decimal Amount { get; set; }
        public string? Note { get; set; }
    }
}
