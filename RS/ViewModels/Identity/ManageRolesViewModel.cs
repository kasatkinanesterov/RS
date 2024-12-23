namespace RS.ViewModels.Identity
{
    public class ManageRolesViewModel
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public List<string> AvailableRoles { get; set; }
        public IEnumerable<string> AssignedRoles { get; set; }
        public List<string> SelectedRoles { get; set; }
    }
}
