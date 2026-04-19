using Backend.DTOs.Responses.Accounts;
using Microsoft.EntityFrameworkCore;

namespace Backend.DTOs.Responses.Accounts
{
    public class TrialBalanceDto
    {
        public Guid AccountId { get; set; }

        public string AccountName { get; set; } = string.Empty;

        public decimal Debit { get; set; }

        public decimal Credit { get; set; }
    }
}


//public async Task<List<TrialBalanceDto>> GetTrialBalance()
//{
//    var data = await _context.TransactionDetails
//        .Include(x => x.Account)
//        .ToListAsync();

//    var result = data
//        .GroupBy(x => new { x.AccountId, x.Account.Name })
//        .Select(g => new TrialBalanceDto
//        {
//            AccountId = g.Key.AccountId,
//            AccountName = g.Key.Name,
//            Debit = g.Where(x => x.EntryType == 1).Sum(x => x.Amount),
//            Credit = g.Where(x => x.EntryType == 2).Sum(x => x.Amount)
//        })
//        .ToList();

//    return result;
//}