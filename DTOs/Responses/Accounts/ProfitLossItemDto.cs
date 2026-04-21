namespace Backend.DTOs.Responses.Accounts
{
    public class ProfitLossItemDto
    {
        public string AccountName { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty; // Revenue / Expense
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
    }
}
