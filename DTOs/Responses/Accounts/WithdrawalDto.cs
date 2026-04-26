namespace Backend.DTOs.Responses.Accounts
{
    public class WithdrawalDto
    {
        public Guid Id { get; set; }

        public Guid AgencyId { get; set; }
        public Guid BranchId { get; set; }

        public string? WithdrawNo { get; set; }
        public byte Status { get; set; }
        public decimal? Amount { get; set; } = 0;


        public Guid AccountId { get; set; }
        public string? AccountName { get; set; }

        public Guid CustomerId { get; set; }
        public string? CustomerName { get; set; }

        public DateTime WithdrawnAt { get; set; } // Taariikhda lacagta lala baxay

        public int CurrencyId { get; set; }
        public string? CurrencyCode { get; set; }

        public Guid? TransactionId { get; set; }
        public string? TransactionDescription { get; set; }

        // Metadata dheeraad ah oo muhiim u ah baaritaanka (Audit)
        public string? ReceiverName { get; set; }   // Qofka gacanta looga saaray lacagta
        public string? ReceiverIdCard { get; set; } // Aqoonsiga qofka lacagta qaatay
        
        public string? UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
    }
}
