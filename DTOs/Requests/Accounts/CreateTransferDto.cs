using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs.Requests.Accounts
{
    public class CreateTransferDto
    {
        [Required]
        public Guid FromAccountId { get; set; }

        [Required]
        public Guid ToAccountId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }

        public string? SenderName { get; set; }
        public string? ReceiverName { get; set; }
    }
}
