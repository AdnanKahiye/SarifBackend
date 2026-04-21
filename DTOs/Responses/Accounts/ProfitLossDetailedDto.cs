using Backend.Wrapper;

namespace Backend.DTOs.Responses.Accounts
{
    public class ProfitLossDetailedDto
    {
        public decimal TotalRevenue { get; set; }
        public decimal TotalExpense { get; set; }
        public decimal Profit { get; set; }

        public PagedResponse<ProfitLossItemDto> Details { get; set; } = null!;
    }
}
