namespace Backend.DTOs.Responses.Accounts
{
    public class DashboardCardsDto
    {
        public decimal TotalPayableAccountBase { get; set; }
        public decimal TotalReceivableAccountBase { get; set; }

        public decimal CurrentBalanceBase { get; set; }
        public decimal DailyProfitBase { get; set; }
        public int TodayTransactions { get; set; }

        public List<DashboardCurrencyCardDto> BalancesByCurrency { get; set; } = new();
        public List<DashboardCurrencyCashFlowDto> CashFlowTodayByCurrency { get; set; } = new();
    }

    public class DashboardCurrencyCardDto
    {
        public int CurrencyId { get; set; }
        public string CurrencyCode { get; set; } = string.Empty;
        public decimal Balance { get; set; }
    }

    public class DashboardCurrencyCashFlowDto
    {
        public int CurrencyId { get; set; }
        public string CurrencyCode { get; set; } = string.Empty;
        public decimal CashInToday { get; set; }
        public decimal CashOutToday { get; set; }
        public decimal NetToday { get; set; }
    }
}