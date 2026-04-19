using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs.Requests.Accounts
{
    public class UpdateExchangeRateDto
    {

        [Required]
        public decimal Rate { get; set; }

        [Required]
        public int CurrencyId { get; set; }


    }
}