namespace Backend.DTOs.Responses.Customers
{
    public class CustomerDto
    {
        public Guid Id { get; set; }

        public string FullName { get; set; } = string.Empty;

        public byte? Gender { get; set; }

        public DateTime? DateOfBirth { get; set; }

        public string? Email { get; set; }

        public string PhoneNumber { get; set; } = string.Empty;

        public string? AltPhoneNumber { get; set; }

        public string? Address { get; set; }

        public Guid? AgencyId { get; set; }

        public Guid? BranchId { get; set; }

        // Optional display fields
        public string? AgencyName { get; set; }
        public string? BranchName { get; set; }


        public string UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
    }
}
