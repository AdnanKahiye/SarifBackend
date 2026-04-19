namespace Backend.DTOs.Responses.Setup
{
    public class MenuSummaryDto
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;
        public string? Href { get; set; }
        public string? Icon { get; set; }

        public int? ParentId { get; set; }
        public int OrderNo { get; set; }
        public string ModuleName { get; set; }


        public int ModuleId { get; set; }


    }
}
