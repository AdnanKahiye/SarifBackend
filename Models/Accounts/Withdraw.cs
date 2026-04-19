using Backend.Models.Customers;
using Backend.Models.Setup;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models.Accounts
{
    public class Withdraw : BaseEntity
    {
        public Guid AgencyId { get; set; }
        public Guid? BranchId { get; set; }

        [ForeignKey(nameof(AgencyId))]
        public Agency Agency { get; set; } = null!;

        [ForeignKey(nameof(BranchId))]
        public Branch? Branch { get; set; }

        public string? WithdrawNo { get; set; } 

        public byte Status { get; set; } = 1;

        [Required]
        public Guid AccountId { get; set; }
        [ForeignKey(nameof(AccountId))]
        public Account Account { get; set; } = null!;

        [Required]
        public Guid CustomerId { get; set; }
        [ForeignKey(nameof(CustomerId))]
        public Customer Customer { get; set; } = null!;

        // Marka lacagta lala baxay
        public DateTime WithdrawnAt { get; set; } = DateTime.UtcNow;

        public int CurrencyId { get; set; }
        [ForeignKey(nameof(CurrencyId))]
        public Currency Currency { get; set; } = null!;

        [Required]
        public Guid TransactionId { get; set; }
        [ForeignKey(nameof(TransactionId))]
        public Transaction Transaction { get; set; } = null!;

        // Metadata dheeraad ah oo muhiim u ah Withdraw-ga
        public string? ReceiverName { get; set; } // Haddii qof kale loo soo wakiishay
        public string? ReceiverIdCard { get; set; } // Aqoonsiga qofka lacagta qaatay
    }
}
