using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs.Requests.Accounts
{
    public class CreateRepaymentDto
    {
        [Required]
        public Guid LoanId { get; set; }

        [Required]
        public decimal Amount { get; set; }

        public string? Note { get; set; }

        // Account-ka lacagta lagu shubayo (Main Cash)
        [Required]
        public Guid CashAccountId { get; set; }

        // Account-ka deynta (Loan Receivable Account)
        [Required]
        public Guid LoanAccountId { get; set; }
    }
}
