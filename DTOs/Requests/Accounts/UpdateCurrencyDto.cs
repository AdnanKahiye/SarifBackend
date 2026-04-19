using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs.Requests.Accounts
{
    public class UpdateCurrencyDto
    {

        [Required]
        [MaxLength(10)]
        public string Code { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(10)]
        public string? Symbol { get; set; }

        public int DecimalPlaces { get; set; }

        public bool IsBase { get; set; }
    }
}
