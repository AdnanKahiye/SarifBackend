namespace Backend.DTOs.Requests.Accounts.Internal
{
    public class TransactionDetailModel
    {
        public Guid AccountId { get; set; }
        public decimal Amount { get; set; }
        public byte EntryType { get; set; }
        public int CurrencyId { get; set; }
    }
}
