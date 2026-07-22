using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YunChenShipping.Models
{
    public class Product
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required(ErrorMessage = "產品名稱為必填")]
        [StringLength(200)]
        [Display(Name = "產品名稱及規格")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "PART NO 為必填")]
        [StringLength(50)]
        [Display(Name = "PART NO")]
        public string PartNo { get; set; } = string.Empty;

        [Required(ErrorMessage = "單位為必填")]
        [StringLength(20)]
        [Display(Name = "單位")]
        public string Unit { get; set; } = string.Empty;

        [Required(ErrorMessage = "標準單價為必填")]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "標準單價")]
        public decimal StandardPrice { get; set; }

        [Display(Name = "課稅別")]
        public TaxType TaxType { get; set; }

        [StringLength(500)]
        [Display(Name = "備註")]
        public string? Remarks { get; set; }

        [Display(Name = "狀態")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "排序")]
        public int SortOrder { get; set; }

        [Display(Name = "建立時間")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [StringLength(100)]
        [Display(Name = "建立人員")]
        public string? CreatedBy { get; set; }

        // 導航屬性
        public ICollection<ProductPriceHistory> PriceHistory { get; set; } = new List<ProductPriceHistory>();
    }

    public class ProductPriceHistory
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int ProductId { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "歷史單價")]
        public decimal Price { get; set; }

        [Display(Name = "變更時間")]
        public DateTime ChangedAt { get; set; } = DateTime.Now;

        [StringLength(100)]
        [Display(Name = "變更人員")]
        public string? ChangedBy { get; set; }

        // 導航屬性
        [ForeignKey("ProductId")]
        public Product? Product { get; set; }
    }

    public enum TaxType
    {
        [Display(Name = "應稅")]
        Taxable = 0,
        [Display(Name = "零稅")]
        ZeroTax = 1,
        [Display(Name = "免稅")]
        TaxFree = 2
    }
}
