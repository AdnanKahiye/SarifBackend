using Backend.Models.Accounts;
using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs.Requests.Accounts
{
    public class CreateTransactionRequest
    {
        [Required]
        public TransactionTypeEnum TransactionType { get; set; }

        public string? Description { get; set; }

        // 🔥 Only one of these should be filled
        public CreateExchangeDto? Exchange { get; set; }
        public CreateTransferDto? Transfer { get; set; }
        public CreateLoanDto? Loan { get; set; }
        public CreateExpenseDto? Expense { get; set; }
        public CreateDepositDto? Deposit { get; set; }
        public CreateWithdrawDto? Withdraw { get; set; }
        public CreateRepaymentDto? Repayment { get; set; }
        public RevenueDetailsDto? Revenue { get; set; }
    }
}