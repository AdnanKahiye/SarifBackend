namespace Backend.DTOs.Responses.Accounts
{
    public class DailyReportDto
    {
        public DateTime Date { get; set; }
        public decimal TotalIn { get; set; }
        public decimal TotalOut { get; set; }
        public decimal Balance { get; set; }
    }
}
