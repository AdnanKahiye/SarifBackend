namespace Backend.DTOs.Requests.Accounts
{
    public class CashOpeningReportDto
    {
        public Guid TransactionId { get; set; }
        public string? ReferenceNo { get; set; }
        public DateTime Date { get; set; }

        public string CashAccountName { get; set; } = "";
        public string CapitalAccountName { get; set; } = "";
        public string CurrencyCode { get; set; } = "";

        public decimal? Amount { get; set; }
        public string? Description { get; set; }
    }

    public class DailyCashReportDto
    {
        public Guid AccountId { get; set; }
        public string AccountName { get; set; } = "";
        public string CurrencyCode { get; set; } = "";

        public decimal OpeningCash { get; set; }
        public decimal CashIn { get; set; }
        public decimal CashOut { get; set; }
        public decimal SystemClosingCash { get; set; }
    }
}
