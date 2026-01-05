using System;
using System.Collections.Generic;

namespace DataCo_Website.Models;

public partial class OrderItem
{
    public int OrderItemId { get; set; }

    public int OrderId { get; set; }

    public int? ProductId { get; set; }

    public int? Quantity { get; set; }

    public double? Discount { get; set; }

    public double? DiscountRate { get; set; }

    public double? ProfitRatio { get; set; }

    public double? Total { get; set; }

    public double? Sales { get; set; }
    public int DepartmentId { get; set; }
    public virtual Order Order { get; set; } = null!;

    public virtual Product? Product { get; set; }
    public virtual Department Department { get; set; } = null!;
}
