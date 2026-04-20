using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs.Requests.Accounts
{
    public class CreateExchangeDto
    {
        [Required]
        public decimal Rate { get; set; }

        [Required]
        public decimal FromAmount { get; set; }

        [Required]
        public decimal ToAmount { get; set; }

        public decimal Fee { get; set; } = 0;

        public Guid FromAccountId { get; set; }

        public Guid ToAccountId { get; set; }
    }
}
