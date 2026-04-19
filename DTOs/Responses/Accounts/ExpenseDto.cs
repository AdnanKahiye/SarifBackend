using Backend.Models.Setup;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.DTOs.Responses.Accounts
{
    public class ExpenseDto
    {
        public Guid Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        public decimal Amount { get; set; }

        public Guid TransactionId { get; set; }
        public string TransactionDescription { get; set; }

        public Guid AccountId { get; set; }
        public string? AccountName { get; set; }

        public DateTime ExpenseDate { get; set; }

        public string? UserId { get; set; }
        public string UserName { get; set; } = string.Empty;



        //public Guid? AgencyId { get; set; }
        //public string? AgencyName { get; set; }


        //public Guid? BranchId { get; set; }
        //public string? BranchName { get; set; }

    }
}
