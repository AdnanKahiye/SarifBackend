using Backend.DTOs.Responses.Accounts;
using Microsoft.EntityFrameworkCore;

namespace Backend.DTOs.Responses.Accounts
{
    public class LedgerDto
    {
        public DateTime Date { get; set; }

        public string? Description { get; set; }

        public decimal Debit { get; set; }

        public decimal Credit { get; set; }

        public decimal Balance { get; set; }
    }
}



//public async Task<List<LedgerDto>> GetLedger(Guid accountId)
//{
//    var data = await _context.TransactionDetails
//        .Where(x => x.AccountId == accountId)
//        .Include(x => x.Transaction)
//        .OrderBy(x => x.Transaction.CreatedAt)
//        .ToListAsync();

//    decimal balance = 0;

//    var result = new List<LedgerDto>();

//    foreach (var item in data)
//    {
//        decimal debit = 0;
//        decimal credit = 0;

//        if (item.EntryType == 1) // Debit
//        {
//            debit = item.Amount;
//            balance += debit;
//        }
//        else // Credit
//        {
//            credit = item.Amount;
//            balance -= credit;
//        }

//        result.Add(new LedgerDto
//        {
//            Date = item.Transaction.CreatedAt,
//            Description = item.Transaction.Description,
//            Debit = debit,
//            Credit = credit,
//            Balance = balance
//        });
//    }

//    return result;
//}
