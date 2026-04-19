using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs.Requests.Accounts
{
    public class UpdateTransactionDto
    {


        [Required]
        public int TransactionType { get; set; }

        public int Status { get; set; }


        public string? Description { get; set; }

        public decimal? TotalAmount { get; set; }

        public Guid AgencyId { get; set; }

        public Guid? BranchId { get; set; }

        public List<UpdateTransactionDetailDto> Details { get; set; } = new();
    }
}
