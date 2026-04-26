using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs.Requests.Accounts
{
    public class CreateDepositDto
    {
        [Required]
        public Guid AccountId { get; set; }

        [Required]
        public Guid CustomerId { get; set; }

        [Required]
        public int CurrencyId { get; set; }
        public decimal Amount { get; set; } = 0;


    }

}
