using Backend.Models.Accounts;
using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs.Requests.Accounts
{
    public class RevenueDetailsDto
    {
        [Required]
        public string Title { get; set; } = string.Empty; // Tusaale: "Khidmadda Adeegga"

        public string? Description { get; set; }

       public RevenueSourceEnum SourceType { get; set; } // Halkan u beddel Enum

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Lacagtu waa inay ka badnaataa 0")]
        public decimal Amount { get; set; }

        [Required]
        public Guid RevenueAccountId { get; set; } // Account-ka dakhliga (Type: Revenue)

        [Required]
        public Guid CashAccountId { get; set; } // Khasnadda lacagtu ku dhacday (Type: Cash/Bank)

    }
}
