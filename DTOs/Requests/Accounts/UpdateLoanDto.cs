using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs.Requests.Accounts
{
    public class UpdateLoanDto
    {

        public string? LoanNo { get; set; }

        [Required]
        public decimal PrincipalAmount { get; set; }
        public decimal PaidAmount { get; set; } = 0;

        public decimal? InterestRate { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? DueDate { get; set; }

        public int Status { get; set; }

        [Required]
        public Guid AccountId { get; set; }

        [Required]
        public Guid CustomerId { get; set; }


        public Guid AgencyId { get; set; }
        public Guid? BranchId { get; set; }
    }
}
