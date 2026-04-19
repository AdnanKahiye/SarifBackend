using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs.Requests.Accounts
{
    public class CreateAccountDto
    {
        [Required]
        [MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        [Required]

//wAXAA KUSO DAREY FADLAN LASOO DEG

        public int AccountType { get; set; }

        public Guid? ReferenceId { get; set; }

        [Required]
        public int CurrencyId { get; set; }

    }
}
