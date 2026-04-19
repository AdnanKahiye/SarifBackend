using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs.Requests.Accounts
{
    public class CreateDepositDto
    {
        [Required]
        public Guid AgencyId { get; set; }

        [Required]
        public Guid BranchId { get; set; }

        public string? DepositNo { get; set; }

        [Required]
        public Guid AccountId { get; set; }

        [Required]
        public Guid CustomerId { get; set; }

        public int CurrencyId { get; set; }
    }

}
