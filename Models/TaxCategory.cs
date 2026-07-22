using System.ComponentModel.DataAnnotations;

namespace YunChenShipping.Models
{
    public class TaxCategory
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "營業稅名稱為必填")]
        [StringLength(50)]
        [Display(Name = "營業稅名稱")]
        public string Name { get; set; } = string.Empty;

        [Range(0, 100)]
        [Display(Name = "稅率(%)")]
        public decimal TaxRate { get; set; }

        [Display(Name = "排序")]
        public int SortOrder { get; set; }

        [Display(Name = "建立時間")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
