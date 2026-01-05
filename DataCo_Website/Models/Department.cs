using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataCo_Website.Models;

public partial class Department
{
    public int DepartmentId { get; set; }

    public string? DepartmentName { get; set; }
    public string? AddressLine { get; set; }
    public bool IsActive { get; set; } = true;

    [NotMapped]
    public int OrderCount { get; set; }

    public virtual ICollection<Category> Categories { get; set; } = new List<Category>();
    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    [NotMapped]
    public string Slug => DepartmentName?.ToLowerInvariant()
        .Trim()
        .Replace(" ", "%20")
        .Replace("&", "%26") ?? "";
}
