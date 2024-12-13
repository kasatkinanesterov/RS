using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
namespace RS.Models
{
    public class Employee : IdentityUser
    {
        public string FullName { get; set; } = "New User";
        public string Department { get; set; } = "General";
    }

}
