using System;
using System.Collections.Generic;

namespace DataCo_Website.Models;

public partial class Customer
{
    public string? Zipcode { get; set; }

    public string? Segment { get; set; }

    public int CustomerId { get; set; }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }
    public bool IsActive { get; set; } = true;

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
    public virtual ICollection<Cart> Carts { get; set; } = new List<Cart>();

    public virtual Location? ZipcodeNavigation { get; set; }
    // Thêm navigation property cho ApplicationUser
    public virtual ApplicationUser? ApplicationUser { get; set; }
}
