namespace Backend.DTOs.Responses.Accounts
{
    public class LoanPaymentDto
    {
        public Guid Id { get; set; }
        public Guid LoanId { get; set; }
        public string? LoanNo { get; set; } // Tixraaca deynta asalka ah

        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }
        public string? Note { get; set; }

        public Guid TransactionId { get; set; }
        public string? TransactionDescription { get; set; }

        // Account-yada ku lug leh
        public string? CashAccountName { get; set; }
        public string? LoanAccountName { get; set; }



        public string? UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
    }
}
