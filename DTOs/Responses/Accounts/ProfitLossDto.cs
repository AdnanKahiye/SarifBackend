using Backend.DTOs.Responses.Accounts;
using Backend.Models.Accounts;
using Microsoft.EntityFrameworkCore;

namespace Backend.DTOs.Responses.Accounts
{
    public class ProfitLossDto
    {
        public decimal TotalRevenue { get; set; }
        public decimal TotalExpense { get; set; }
        public decimal NetProfit { get; set; }
    }
}

//public async Task<ProfitLossDto> GetProfitLoss()
//{
//    var data = await _context.TransactionDetails
//        .Include(x => x.Account)
//        .ToListAsync();

//    var revenue = data
//        .Where(x => x.Account.AccountType == AccountTypeEnum.Revenue)
//        .Sum(x => x.EntryType == 2 ? x.Amount : -x.Amount);

//    var expense = data
//        .Where(x => x.Account.AccountType == AccountTypeEnum.Expense)
//        .Sum(x => x.EntryType == 1 ? x.Amount : -x.Amount);

//    return new ProfitLossDto
//    {
//        TotalRevenue = revenue,
//        TotalExpense = expense,
//        NetProfit = revenue - expense
//    };
//}
