using Backend.Models.Accounts;
using Backend.Models.Setup;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models.Customers
{
    public class Customer : BaseEntity
    {


        [Required, MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required, MaxLength(100)]

        public byte? Gender { get; set; }

        [MaxLength(150)]
        public string? Email { get; set; }

        [Required, MaxLength(50)]
        public string PhoneNumber { get; set; } = string.Empty;

        public string? AltPhoneNumber { get; set; }

        public string? Address { get; set; }


        public Guid? AgencyId { get; set; }   // Admin = null or assigned
        public Guid? BranchId { get; set; }   // Staff = must have, Admin = null


        [ForeignKey(nameof(AgencyId))]
        public Agency? Agency { get; set; }

        [ForeignKey(nameof(BranchId))]
        public Branch? Branch { get; set; }
        public ICollection<Deposit> Deposits { get; set; } = new List<Deposit>();


    }
}
