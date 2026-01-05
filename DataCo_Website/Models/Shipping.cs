using System;
using System.Collections.Generic;

namespace DataCo_Website.Models;

public partial class Shipping
{
    public int OrderId { get; set; }

    public DateTime? OrderDate { get; set; }

    public DateTime? ShippingDate { get; set; }

    public bool? LateDeliveryRisk { get; set; }

    public int? DaysForShipmentScheduled { get; set; }

    public int? DaysForShippingActual { get; set; }

    public string? DeliveryStatus { get; set; }

    public string? ShippingMode { get; set; }

    public virtual Order Order { get; set; } = null!;
}
