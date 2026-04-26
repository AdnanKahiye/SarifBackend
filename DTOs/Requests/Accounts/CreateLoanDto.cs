using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs.Requests.Accounts
{
    public class CreateLoanDto
    {
        public string? LoanNo { get; set; }

        [Required]
        public decimal PrincipalAmount { get; set; }

        public decimal InterestRate { get; set; } = 0;

        public DateTime? DueDate { get; set; }

        [Required]
        public Guid AccountId { get; set; }

        [Required]
        public Guid CustomerId { get; set; }
    }
}
