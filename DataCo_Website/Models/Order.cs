using System;
using System.Collections.Generic;

namespace DataCo_Website.Models;

public partial class Order
{
    public int OrderId { get; set; }

    public int? CustomerId { get; set; }

    public string? Status { get; set; }

    public string? OrderRegion { get; set; }

    public string? OrderCountry { get; set; }

    public string? OrderState { get; set; }

    public string? OrderCity { get; set; }

    public string? Market { get; set; }

    public string? TypeTransaction { get; set; }

    public DateTime? OrderDate { get; set; }
    public string? AddressLine { get; set; }

    public virtual Customer? Customer { get; set; }

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public virtual Shipping? Shipping { get; set; }
}
