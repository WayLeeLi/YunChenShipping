using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace YunChenShipping.Models
{
    public class ApplicationUser : IdentityUser
    {
        [StringLength(50)]
        [Display(Name = "中文名")]
        public string? ChineseName { get; set; }
    }
}
