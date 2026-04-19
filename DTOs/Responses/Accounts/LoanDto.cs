namespace Backend.DTOs.Responses.Accounts
{
    public class LoanDto
    {
        public Guid Id { get; set; }

        public string? LoanNo { get; set; }

        public decimal PrincipalAmount { get; set; }

        public decimal? InterestRate { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? DueDate { get; set; }

        public int Status { get; set; }

        public Guid AccountId { get; set; }
        public string? AccountName { get; set; }

        public Guid CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public Guid TransactionId { get; set; }
        public string TransactionDescription { get; set; }


        public string UserId { get; set; }
        public string UserName { get; set; } = string.Empty;


        public Guid AgencyId { get; set; }
        public string? AgencyName { get; set; }
        public Guid? BranchId { get; set; }
        public string? BranchName { get; set; }
    }
}
