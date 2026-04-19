namespace Backend.DTOs.Responses.Accounts
{
    public class RevenueDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Amount { get; set; }

        // Taariikhda
        public DateTime CreatedAt { get; set; }

        // Source-ka (Enum-ka oo loo beddelay String ama Magac)
        public string SourceType { get; set; } = string.Empty; // Tusaale: "Exchange", "Direct", "Transfer"

        // Account-yada (Magacyadooda)
        public string RevenueAccountName { get; set; } = string.Empty; // Account-ka dakhliga (Type 7)
        public string CashAccountName { get; set; } = string.Empty;    // Khasnadda (Type 1)

        // Tixraacyada (Link-yada)
        public Guid TransactionId { get; set; }
        public string? ReferenceNo { get; set; } // Reference-ka Transaction-ka weyn

        // User-ka diiwaangeliyey
        public string UserName { get; set; } = string.Empty;
    }
}
