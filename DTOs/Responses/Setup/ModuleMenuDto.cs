namespace Backend.DTOs.Responses.Setup
{
    public class ModuleMenuDto
    {
        public int ModuleId { get; set; }
        public string ModuleName { get; set; }

        public List<MenuDto> Menus { get; set; }
    }
}
