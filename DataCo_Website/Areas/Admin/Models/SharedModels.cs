namespace DataCo_Website.Areas.Admin.Models
{
    // ============= SHARED MODELS (dùng chung) =============
    public class ChartDataPoint
    {
        public string Label { get; set; } = "";
        public decimal Value { get; set; }
    }

    // ============= DASHBOARD MODELS =============
    public class DashboardFilters
    {
        public List<int>? Years { get; set; }
        public List<int>? Quarters { get; set; }
        public List<string>? Markets { get; set; }
        public List<string>? Departments { get; set; }
    }

    public class DashboardViewModel
    {
        public decimal TotalProfit { get; set; }
        public decimal TotalRevenue { get; set; }
        public List<ChartDataPoint> ProfitBySegment { get; set; } = new();
        public List<ChartDataPoint> OrderCountByStatus { get; set; } = new();
        public List<ChartDataPoint> TransactionCountByType { get; set; } = new();
        public List<MarketChartData> RevenueByMarket { get; set; } = new();
        public List<YearChartData> RevenueByYear { get; set; } = new();
        public List<DepartmentChartData> RevenueByDepartment { get; set; } = new();
        public List<CategoryChartData> RevenueByCategory { get; set; } = new();
        public List<StateChartData> ProfitByState { get; set; } = new();
    }

    public class MarketChartData
    {
        public string Market { get; set; } = "";
        public decimal Revenue { get; set; }
        public decimal Profit { get; set; }
    }

    public class YearChartData
    {
        public string Year { get; set; } = "";
        public decimal Revenue { get; set; }
        public decimal Profit { get; set; }
    }

    public class DepartmentChartData
    {
        public string Department { get; set; } = "";
        public decimal Revenue { get; set; }
        public decimal Profit { get; set; }
    }

    public class CategoryChartData
    {
        public string Category { get; set; } = "";
        public decimal Revenue { get; set; }
        public decimal Profit { get; set; }
    }

    public class StateChartData
    {
        public string State { get; set; } = "";
        public decimal Revenue { get; set; }
        public decimal Profit { get; set; }
    }

    // ============= SHIPPING DASHBOARD MODELS =============
    public class ShippingFilters
    {
        public List<int>? Years { get; set; }
        public List<int>? Quarters { get; set; }
        public List<string>? DeliveryStatus { get; set; }
        public List<string>? Segments { get; set; }
    }

    public class ShippingDashboardViewModel
    {
        public int TotalShipments { get; set; }
        public decimal AverageDaysActual { get; set; }
        public decimal AverageDaysScheduled { get; set; }
        public decimal LateDeliveryRate { get; set; }
        public List<ChartDataPoint> ShipmentsByDeliveryStatus { get; set; } = new();
        public List<YearShippingData> ShipmentsByYear { get; set; } = new();
        public List<ChartDataPoint> ShipmentsBySegment { get; set; } = new();
        public List<ChartDataPoint> ShipmentsByShippingMode { get; set; } = new();
        public List<DeliveryPerformanceData> DeliveryPerformanceByYear { get; set; } = new();
        public List<ChartDataPoint> TopCategoriesByShipments { get; set; } = new();
        public List<ChartDataPoint> TopStatesByShipments { get; set; } = new();
        public List<ChartDataPoint> ShipmentsByDepartment { get; set; } = new();
    }

    public class YearShippingData
    {
        public string Year { get; set; } = "";
        public int ShipmentCount { get; set; }
        public decimal DaysActual { get; set; }
        public decimal DaysScheduled { get; set; }
    }

    public class DeliveryPerformanceData
    {
        public string Year { get; set; } = "";
        public decimal DaysActual { get; set; }
        public decimal DaysScheduled { get; set; }
    }
}