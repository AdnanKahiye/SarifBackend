using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs.Requests.Accounts
{
    public class UpdateAccountDto
    {
        [Required]
        [MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public int AccountType { get; set; }

        public Guid? ReferenceId { get; set; }

        [Required]
        public int CurrencyId { get; set; }


    }
}
