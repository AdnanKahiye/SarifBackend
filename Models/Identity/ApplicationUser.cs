using Backend.Models.Setup;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models.Identity
{
    public class ApplicationUser : IdentityUser
    {
        // 🔹 Personal Information
        [MaxLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [MaxLength(100)]
        public string LastName { get; set; } = string.Empty;

        [MaxLength(20)]
        public string Gender { get; set; } = string.Empty;

        public DateTime? DateOfBirth { get; set; }

        // 🔹 Organization Structure
        public Guid? AgencyId { get; set; }   // Admin = null or assigned
        public Guid? BranchId { get; set; }   // Staff = must have, Admin = null

        [ForeignKey(nameof(AgencyId))]
        public Agency? Agency { get; set; }

        [ForeignKey(nameof(BranchId))]
        public Branch? Branch { get; set; }

        // 🔹 Contact & Address
        [MaxLength(255)]
        public string? Address { get; set; }

        [MaxLength(100)]
        public string? City { get; set; }

        [MaxLength(100)]
        public string? Country { get; set; }

        // 🔹 Security
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime? RefreshTokenExpiryDate { get; set; }

        // 🔹 System / Status
        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public DateTime? LastLoginTime { get; set; }

        public string? CreatedBy { get; set; }
    }
}