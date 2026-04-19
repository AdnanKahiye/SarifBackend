namespace Backend.DTOs.Responses.Accounts
{
    public class AccountDto
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public int AccountType { get; set; }   // or use enum

        public Guid? ReferenceId { get; set; }

        public int CurrencyId { get; set; }

        public string? CurrencyName { get; set; }

        public Guid? AgencyId { get; set; }

        public Guid? BranchId { get; set; }

        public string? AgencyName { get; set; }

        public string? BranchName { get; set; }

        public string UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
    }
}
