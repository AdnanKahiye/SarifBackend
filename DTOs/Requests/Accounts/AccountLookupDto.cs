namespace Backend.DTOs.Requests.Accounts
{
    public class AccountLookupDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;


        public int CurrencyId { get; set; }
        public string CurrencyName { get; set; } = string.Empty;
    }
}
