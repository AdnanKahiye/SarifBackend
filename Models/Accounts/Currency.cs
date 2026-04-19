using Backend.Models.Customers;
using System.ComponentModel.DataAnnotations;

namespace Backend.Models.Accounts
{
    public class Currency : BaseStaticEntity
    {

        [Required]
        [MaxLength(10)]
        public string Code { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(10)]
        public string? Symbol { get; set; }

        public int DecimalPlaces { get; set; } = 2;

        public bool IsBase { get; set; } = false;

        public ICollection<Account> Accounts { get; set; } = new List<Account>();
        public ICollection<ExchangeRate>  ExchangeRates { get; set; } = new List<ExchangeRate>();
        public ICollection<Deposit>  Deposits { get; set; } = new List<Deposit>();


    }
}
