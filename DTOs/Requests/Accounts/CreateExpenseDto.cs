using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs.Requests.Accounts
{
    public class CreateExpenseDto
    {
        [Required]
        [MaxLength(150)]
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required]
        public decimal Amount { get; set; }

        [Required]
        public Guid AccountId { get; set; }   // Expense account

        public DateTime? ExpenseDate { get; set; }
    }
}
