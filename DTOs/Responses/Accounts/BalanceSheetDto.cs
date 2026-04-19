using Backend.DTOs.Responses.Accounts;
using Backend.Models.Accounts;
using Microsoft.EntityFrameworkCore;

namespace Backend.DTOs.Responses.Accounts
{
    public class BalanceSheetDto
    {
        public decimal TotalAssets { get; set; }
        public decimal TotalLiabilities { get; set; }
        public decimal TotalEquity { get; set; }
    }
}



////public async Task<BalanceSheetDto> GetBalanceSheet()
////{
////    var data = await _context.TransactionDetails
////        .Include(x => x.Account)
////        .ToListAsync();

////    decimal assets = data
////        .Where(x => x.Account.AccountType == AccountTypeEnum.Cash
////                 || x.Account.AccountType == AccountTypeEnum.Bank
////                 || x.Account.AccountType == AccountTypeEnum.Customer)
////        .Sum(x => x.EntryType == 1 ? x.Amount : -x.Amount);

////    decimal liabilities = data
////        .Where(x => x.Account.AccountType == AccountTypeEnum.Loan)
////        .Sum(x => x.EntryType == 2 ? x.Amount : -x.Amount);

////    decimal equity = assets - liabilities;

////    return new BalanceSheetDto
////    {
////        TotalAssets = assets,
////        TotalLiabilities = liabilities,
////        TotalEquity = equity
////    };
////}
