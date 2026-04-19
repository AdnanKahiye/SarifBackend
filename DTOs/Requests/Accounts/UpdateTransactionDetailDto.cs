using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs.Requests.Accounts
{
    public class UpdateTransactionDetailDto
    {
        public Guid? Id { get; set; }   // null = new, has value = update

        [Required]
        public Guid AccountId { get; set; }

        [Required]
        public decimal Amount { get; set; }

        [Required]
        public byte EntryType { get; set; } // 1=Debit, 2=Credit

        [Required]
        public int CurrencyId { get; set; }
    }
}
