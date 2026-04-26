using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs.Requests.Accounts
{
    public class UpdateExchangeSettingsDto
    {
        [Required]
        public int Id { get; set; }

        [Required]
        [Range(0, 1)]
        public decimal FeeRate { get; set; }

        [Required]
        [Range(0, 1)]
        public decimal ProfitRate { get; set; }

        public bool IsActive { get; set; }
    }
}
