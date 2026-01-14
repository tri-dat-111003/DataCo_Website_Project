using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataCo_Website.Models;

public partial class Product
{
    [Display(Name = "Product ID")]
    public int ProductId { get; set; }

    [Display(Name = "Product Name")]
    public string? ProductName { get; set; }

    [Display(Name = "Category")]
    public byte? CategoryId { get; set; }

    [Display(Name = "Product Price ($)")]
    public double? ProductPrice { get; set; }

    [Display(Name = "Cost Price ($)")]  
    public double? Cost { get; set; }

    [Display(Name = "Description")]
    [DataType(DataType.Html)]
    public string? Description { get; set; }

    [Display(Name = "Image URL")]
    public string? Image { get; set; }
    public bool IsActive { get; set; } = true;

    [Display(Name = "Stock Quantity")]
    [Required(ErrorMessage = "Vui lòng nhập số lượng tồn kho")]
    [Range(0, int.MaxValue, ErrorMessage = "Số lượng phải >= 0")]
    public int Stock { get; set; } = 0;

    [NotMapped]
    [Display(Name = "In Stock")]
    public bool InStock => Stock > 0;
    
    [NotMapped]
    [Display(Name = "Upload Image")]
    public IFormFile? ImageFile { get; set; }

    [NotMapped]
    [Display(Name = "Profit Margin (%)")]
    public double? ProfitMarginPercent
    {
        get
        {
            if (ProductPrice.HasValue && Cost.HasValue && ProductPrice > 0)
            {
                return ((ProductPrice.Value - Cost.Value) / ProductPrice.Value) * 100;
            }
            return null;
        }
    }

    public virtual Category? Category { get; set; }

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();

    [NotMapped]
    public string Slug =>
    string.IsNullOrWhiteSpace(ProductName)
        ? ""
        : Uri.EscapeDataString(ProductName.Trim());

    [NotMapped]
    public string FullUrl
    {
        get
        {
            if (Category?.Department == null || Category == null)
                return $"/Products/Details/{ProductId}"; // Fallback to old URL

            return $"/department/{Category.Department.Slug}/category/{Category.Slug}/product/{Slug}";
        }
    }
}
