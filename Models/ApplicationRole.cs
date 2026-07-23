using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace YunChenShipping.Models
{
    public class ApplicationRole : IdentityRole
    {
        [StringLength(200)]
        [Display(Name = "描述")]
        public string? Description { get; set; }

        [StringLength(50)]
        [Display(Name = "角色編碼")]
        public string? RoleCode { get; set; }

        [Display(Name = "是否啟用")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "是否系統角色")]
        public bool IsSystem { get; set; }

        [Display(Name = "排序")]
        public int SortOrder { get; set; }

        [Display(Name = "建立時間")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Display(Name = "更新時間")]
        public DateTime? UpdatedAt { get; set; }
    }
}
