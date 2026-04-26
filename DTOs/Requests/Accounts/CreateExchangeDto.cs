using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs.Requests.Accounts
{
    public class CreateExchangeDto
    {
        [Required]
        public Guid FromAccountId { get; set; }

        [Required]
        public Guid ToAccountId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal FromAmount { get; set; }

        [Required]
        public int FromCurrencyId { get; set; } =0;

        [Required]
        public int ToCurrencyId { get; set; } = 0;
    }
}
