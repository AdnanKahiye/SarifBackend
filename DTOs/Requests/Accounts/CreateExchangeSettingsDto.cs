using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs.Requests.Accounts
{
    public class CreateExchangeSettingsDto
    {
        [Required]
        public int CurrencyId { get; set; }

        [Required]
        [Range(0, 1)]
        public decimal FeeRate { get; set; }

        [Required]
        [Range(0, 1)]
        public decimal ProfitRate { get; set; }
    }
}
