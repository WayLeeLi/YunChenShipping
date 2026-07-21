using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YunChenShipping.Models
{
    public class SystemSetting
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string SettingKey { get; set; } = string.Empty;

        [StringLength(500)]
        public string SettingValue { get; set; } = string.Empty;

        [StringLength(200)]
        public string? Description { get; set; }

        [StringLength(50)]
        public string Category { get; set; } = string.Empty;
    }
}
