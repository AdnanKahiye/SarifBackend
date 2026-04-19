using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models.Accounts
{
    public class TransactionDetail : BaseEntity
    { 

        public decimal Amount { get; set; }

        public byte EntryType { get; set; } // 1=Debit, 2=Credit



        [Required]
        public Guid TransactionId { get; set; }
        public TransactionTypeEnum TransactionType { get; set; }

        [ForeignKey(nameof(TransactionId))]
        public Transaction Transaction { get; set; } = null!;

        [Required]
        public Guid AccountId { get; set; }

        [ForeignKey(nameof(AccountId))]
        public Account Account { get; set; } = null!;

        public int CurrencyId { get; set; }
        [ForeignKey(nameof(CurrencyId))]
        public Currency Currency { get; set; } = null!;
    }
}
