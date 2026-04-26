namespace Backend.DTOs.Responses.Accounts
{
    public class ExchangeSettingsDto
    {
        public int Id { get; set; }
        public int CurrencyId { get; set; }
        public string CurrencyNmane { get; set; }

        public decimal FeeRate { get; set; }
        public decimal ProfitRate { get; set; }

        public bool IsActive { get; set; }
    }
}
