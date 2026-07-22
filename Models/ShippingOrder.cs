using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YunChenShipping.Models
{
    public class ShippingOrder
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "出貨單號")]
        public string OrderNo { get; set; } = string.Empty;

        [Required]
        public int CustomerId { get; set; }

        [StringLength(50)]
        [Display(Name = "發票號碼")]
        public string? InvoiceNo { get; set; }

        [Required(ErrorMessage = "出貨日期為必填")]
        [Display(Name = "出貨日期")]
        public DateTime OrderDate { get; set; } = DateTime.Now;

        [StringLength(50)]
        [Display(Name = "訂單號碼")]
        public string? ReferenceNo { get; set; }

        [StringLength(50)]
        [Display(Name = "案號")]
        public string? ProjectNo { get; set; }

        [Display(Name = "付款方式")]
        public PaymentMethod PaymentMethod { get; set; }

        [StringLength(50)]
        [Display(Name = "運送方式")]
        public string? DeliveryMethod { get; set; }

        [Required(ErrorMessage = "交貨地址為必填")]
        [StringLength(300)]
        [Display(Name = "交貨地址")]
        public string DeliveryAddress { get; set; } = string.Empty;

        [StringLength(500)]
        [Display(Name = "備註")]
        public string? Remarks { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "其他費用")]
        public decimal OtherExpenses { get; set; }

        [Display(Name = "營業稅類別")]
        public int TaxCategoryId { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "稅率(%)")]
        public decimal TaxRate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "銷售額")]
        public decimal SubTotal { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "營業稅")]
        public decimal TaxAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "總計")]
        public decimal Total { get; set; }

        [Display(Name = "狀態")]
        public OrderStatus Status { get; set; } = OrderStatus.Draft;

        [StringLength(100)]
        [Display(Name = "承辦人")]
        public string? Handler { get; set; }

        // 簽核欄位
        [Display(Name = "主管簽核")]
        public bool ManagerApproved { get; set; }

        [StringLength(100)]
        [Display(Name = "主管簽核人員")]
        public string? ManagerName { get; set; }

        [Display(Name = "主管簽核時間")]
        public DateTime? ManagerApprovedAt { get; set; }

        [Display(Name = "會計簽核")]
        public bool AccountingApproved { get; set; }

        [StringLength(100)]
        [Display(Name = "會計簽核人員")]
        public string? AccountingName { get; set; }

        [Display(Name = "會計簽核時間")]
        public DateTime? AccountingApprovedAt { get; set; }

        [Display(Name = "承辦簽核")]
        public bool HandlerApproved { get; set; }

        [Display(Name = "承辦簽核時間")]
        public DateTime? HandlerApprovedAt { get; set; }

        [Display(Name = "排序")]
        public int SortOrder { get; set; }

        [Display(Name = "建立時間")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [StringLength(100)]
        [Display(Name = "建立人員")]
        public string? CreatedBy { get; set; }

        // 導航屬性
        [ForeignKey("CustomerId")]
        public Customer? Customer { get; set; }

        public ICollection<ShippingOrderDetail> Details { get; set; } = new List<ShippingOrderDetail>();
    }

    public class ShippingOrderDetail
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int ShippingOrderId { get; set; }

        [Display(Name = "項次")]
        public int LineNo { get; set; }

        [Required]
        public int ProductId { get; set; }

        [StringLength(200)]
        [Display(Name = "產品名稱及規格")]
        public string? ProductName { get; set; } // 允許手動輸入非制式品項

        [StringLength(50)]
        [Display(Name = "PART NO")]
        public string? PartNo { get; set; }

        [Required(ErrorMessage = "數量為必填")]
        [Display(Name = "數量")]
        public int Quantity { get; set; }

        [StringLength(20)]
        [Display(Name = "單位")]
        public string? Unit { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "單價")]
        public decimal UnitPrice { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "小計")]
        public decimal SubTotal => Quantity * UnitPrice;

        // 導航屬性
        [ForeignKey("ShippingOrderId")]
        public ShippingOrder? ShippingOrder { get; set; }

        [ForeignKey("ProductId")]
        public Product? Product { get; set; }
    }

    public enum OrderStatus
    {
        [Display(Name = "草稿")]
        Draft = 0,
        [Display(Name = "待簽核")]
        PendingApproval = 1,
        [Display(Name = "已出貨")]
        Shipped = 2,
        [Display(Name = "已完成")]
        Completed = 3,
        [Display(Name = "作廢")]
        Cancelled = 4
    }
}
