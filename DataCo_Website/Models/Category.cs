using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataCo_Website.Models;

public partial class Category
{
    public byte CategoryId { get; set; }

    public string? CategoryName { get; set; }
    public int? DepartmentId { get; set; }
    public bool IsActive { get; set; } = true;
    public virtual Department? Department { get; set; } = null!;
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
    [NotMapped]
    public string Slug => CategoryName?.ToLowerInvariant()
        .Trim()
        .Replace(" ", "%20")
        .Replace("&", "%26") ?? "";
}
