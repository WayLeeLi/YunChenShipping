using System.ComponentModel.DataAnnotations;

namespace YunChenShipping.Models
{
    public class DeliveryMethodSetting
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "運送方式名稱為必填")]
        [StringLength(50)]
        [Display(Name = "運送方式")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "排序")]
        public int SortOrder { get; set; }

        [Display(Name = "建立時間")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
