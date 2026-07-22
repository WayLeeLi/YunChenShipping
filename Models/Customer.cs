using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YunChenShipping.Models
{
    public class Customer
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required(ErrorMessage = "客戶名稱為必填")]
        [StringLength(100)]
        [Display(Name = "客戶名稱")]
        public string Name { get; set; } = string.Empty;

        [StringLength(20)]
        [Display(Name = "電話")]
        public string? Phone { get; set; }

        [StringLength(20)]
        [Display(Name = "傳真")]
        public string? Fax { get; set; }

        [StringLength(50)]
        [Display(Name = "聯絡人")]
        public string? ContactPerson { get; set; } // 主聯絡人

        // 導航屬性 - 多筆聯絡人
        public ICollection<CustomerContact> Contacts { get; set; } = new List<CustomerContact>();

        [StringLength(200)]
        [Display(Name = "統一編號")]
        public string? TaxId { get; set; }

        [Display(Name = "付款方式")]
        public PaymentMethod PaymentMethod { get; set; }

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
        public ICollection<CustomerAddress> Addresses { get; set; } = new List<CustomerAddress>();
        public ICollection<ShippingOrder> ShippingOrders { get; set; } = new List<ShippingOrder>();
    }

    public class CustomerAddress
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int CustomerId { get; set; }

        [Required(ErrorMessage = "地址為必填")]
        [StringLength(300)]
        [Display(Name = "交貨地址")]
        public string Address { get; set; } = string.Empty;

        [Display(Name = "是否為預設地址")]
        public bool IsDefault { get; set; }

        // 導航屬性
        [ForeignKey("CustomerId")]
        public Customer? Customer { get; set; }
    }

    public enum PaymentMethod
    {
        [Display(Name = "現金")]
        Cash = 0,
        [Display(Name = "匯款")]
        Transfer = 1,
        [Display(Name = "月結")]
        Monthly = 2,
        [Display(Name = "支票")]
        Check = 3,
        [Display(Name = "其他")]
        Other = 4
    }

    public class CustomerContact
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int CustomerId { get; set; }

        [Required(ErrorMessage = "聯絡人姓名為必填")]
        [StringLength(50)]
        [Display(Name = "聯絡人姓名")]
        public string Name { get; set; } = string.Empty;

        [StringLength(20)]
        [Display(Name = "電話")]
        public string? Phone { get; set; }

        [StringLength(50)]
        [Display(Name = "職稱")]
        public string? Title { get; set; }

        [Display(Name = "是否為主要聯絡人")]
        public bool IsPrimary { get; set; }

        // 導航屬性
        [ForeignKey("CustomerId")]
        public Customer? Customer { get; set; }
    }
}
