using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs.Requests.Setup
{
    public class CreateMenuDto
    {
        [Required]
        [MaxLength(100)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? Href { get; set; }

        [MaxLength(100)]
        public string? Icon { get; set; }

        public int? ParentId { get; set; }

        public int OrderNo { get; set; } = 0;

        public int ModuleId { get; set; }   // ✅ ADD THIS

    }
}

