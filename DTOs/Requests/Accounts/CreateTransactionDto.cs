using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs.Requests.Accounts
{
    public class CreateTransactionDto
    {
        [Required]
        public int TransactionType { get; set; }

        public string? Description { get; set; }

        public decimal? TotalAmount { get; set; }


        public decimal? ExchangeRate { get; set; }
        public decimal? Fee { get; set; }
        public decimal? Profit { get; set; }



        [Required]
        public List<CreateTransactionDetailDto> Details { get; set; } = new();



        public CreateTransferDto? TransferDetails { get; set; }
        public CreateLoanDto? LoanDetails { get; set; }
        public CreateExpenseDto? ExpenseDetails { get; set; }
        public CreateDepositDto? DepositDetails { get; set; }
        public CreateWithdrawDto? WithdrawDetails { get; set; }
        public CreateRepaymentDto? RepaymentDetails { get; set; }
        public RevenueDetailsDto? RevenueDetails { get; set; }
    }
}
