using Microsoft.AspNetCore.Mvc.Rendering;

namespace RS.ViewModels.Services
{
    public class ServiceViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string? Description { get; set; }
        public string? Photo { get; set; }
        public string? RoleName { get; set; }
        public string RoleId { get; set; }
        public bool IsActive { get; set; }
    }
}
