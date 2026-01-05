using System;
using System.Collections.Generic;

namespace DataCo_Website.Models;

public partial class Location
{
    public string Zipcode { get; set; } = null!;

    public string? City { get; set; }

    public string? Country { get; set; }

    public string? State { get; set; }

    public virtual ICollection<Customer> Customers { get; set; } = new List<Customer>();
}
