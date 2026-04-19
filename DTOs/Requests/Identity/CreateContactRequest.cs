namespace Backend.DTOs.Requests.Identity
{
    public class CreateContactRequest
    {
        public Guid? Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string ?Status { get; set; } = "Pending";
    }
}
