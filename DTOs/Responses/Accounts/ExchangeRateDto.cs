namespace Backend.DTOs.Responses.Accounts
{
    public class ExchangeRateDto
    {
        public int Id { get; set; }

        public decimal Rate { get; set; }

        public int CurrencyId { get; set; }

        public string? CurrencyCode { get; set; }

        public string? CurrencyName { get; set; }

        public Guid? BranchId { get; set; }
        public string? BranchName { get; set; }


        public Guid? AgencyId { get; set; }
        public string? AgencyName { get; set; }


        public string? UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
    }
}
