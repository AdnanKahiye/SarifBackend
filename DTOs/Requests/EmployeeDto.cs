using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs.Requests
{
    public class EmployeeDto
    {



        
        [Required(ErrorMessage = "First Name is required.")]
        [StringLength(50, ErrorMessage = "First Name cannot be longer than 50 characters.")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last Name is required.")]
        [StringLength(50, ErrorMessage = "Last Name cannot be longer than 50 characters.")]
        public string LastName { get; set; }
        public string Email { get; set; } = string.Empty;
        public Guid Id { get; set; }


    }
}
