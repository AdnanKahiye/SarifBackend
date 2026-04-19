using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs.Requests.Accounts
{
    public class CreateWithdrawDto
    {


        public string? WithdrawNo { get; set; }

        [Required]
        public Guid AccountId { get; set; } // Account-ka macmiilka laga jarayo

        [Required]
        public Guid CustomerId { get; set; }

        [Required]
        public int CurrencyId { get; set; }

        // Metadata dheeraad ah (Optional laakiin muhiim ah)
        public string? ReceiverName { get; set; }   // Haddii qof kale loo soo wakiishay
        public string? ReceiverIdCard { get; set; } // Aqoonsiga qofka lacagta qaatay
    }
}
