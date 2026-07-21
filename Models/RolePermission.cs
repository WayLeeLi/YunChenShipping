using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YunChenShipping.Models
{
    public class RolePermission
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string RoleName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string MenuKey { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string MenuName { get; set; } = string.Empty;

        [StringLength(50)]
        public string? Controller { get; set; }

        [StringLength(50)]
        public string? Action { get; set; }

        public bool IsVisible { get; set; } = true;

        [StringLength(50)]
        public string Group { get; set; } = string.Empty;
    }
}
