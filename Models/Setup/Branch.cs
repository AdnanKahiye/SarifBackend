using Backend.DTOs.Responses.Accounts;
using Backend.Models.Accounts;
using Backend.Models.Customers;
using Backend.Models.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models.Setup
{
    public class Branch : BaseEntity
    {
        [Required]
        [MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Code { get; set; } = string.Empty;

        [MaxLength(255)]
        public string? Location { get; set; }

        public bool IsMain { get; set; } = false;

        [Required]
        public Guid AgencyId { get; set; }

        [ForeignKey(nameof(AgencyId))]
        public Agency Agency { get; set; } = null!;
        public ICollection<Customer> Customers { get; set; } = new List<Customer>();
        public ICollection<Account> Accounts { get; set; } = new List<Account>();
        public ICollection<ExchangeDto>  Exchanges { get; set; } = new List<ExchangeDto>();
        public ICollection<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();

    }
}