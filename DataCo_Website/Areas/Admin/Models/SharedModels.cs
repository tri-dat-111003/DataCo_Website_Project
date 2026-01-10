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

    // ✅ THÊM MỚI: Cross-filter request model
    public class CrossFilterRequest
    {
        public string DataType { get; set; } = "";
        public Dictionary<string, string?> Filters { get; set; } = new();

        // ✅ FIX: Thêm form filters từ panel
        public FormFiltersData? FormFilters { get; set; }
    }

    // ✅ FIX: Form filters data structure
    public class FormFiltersData
    {
        public List<int>? Years { get; set; }
        public List<int>? Quarters { get; set; }
        public List<string>? Markets { get; set; }
        public List<string>? Departments { get; set; }
    }

    // ✅ THÊM MỚI: Cross-filter response cho KPIs
    public class KpiResponse
    {
        public decimal TotalProfit { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal ProfitMargin { get; set; }
    }

    public class DashboardViewModel
    {
        public decimal TotalProfit { get; set; }
        public decimal TotalRevenue { get; set; }

        // ✅ CẢI TIẾN #1: Profit Margin % - Chỉ số đo lường hiệu quả kinh doanh
        public decimal ProfitMargin { get; set; }

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

        // ✅ CẢI TIẾN #1: Profit Margin cho từng Market (dùng cho line overlay)
        public decimal ProfitMargin => Revenue > 0 ? Math.Round((Profit / Revenue) * 100, 2) : 0;
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

        // ✅ CẢI TIẾN #1: ProfitMargin cho Scatter Plot tooltip
        public decimal ProfitMargin => Revenue > 0 ? Math.Round((Profit / Revenue) * 100, 2) : 0;
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
        
        public List<string>? Departments { get; set; }
                
        public List<string>? DeliveryStatus { get; set; }
        public List<string>? Segments { get; set; }
        public List<string>? ShippingModes { get; set; }
    }

    public class ShippingDashboardViewModel
    {
        // 1. Cards (Giữ lại Total, bỏ các cái Days/Late)
        public int TotalShipments { get; set; }
        public string TopSegment { get; set; } = "N/A"; // Logic mới: Segment mua nhiều nhất
        public string TopShippingMode { get; set; } = "N/A"; // Logic mới: Mode phổ biến nhất

        // 2. Charts
        public List<ChartDataPoint> ShipmentsByDeliveryStatus { get; set; } = new();
        public List<ChartDataPoint> ShipmentsBySegment { get; set; } = new();
        public List<ChartDataPoint> ShipmentsByShippingMode { get; set; } = new();

        // Chart thay thế 1: Xu hướng theo Quý (Line Chart)
        public List<ChartDataPoint> ShipmentsByQuarterTrend { get; set; } = new();

        // Chart thay thế 2: Mode theo Segment (Stacked Bar)
        public List<StackedBarData> ShippingModeBySegment { get; set; } = new();

        public List<YearShippingData> ShipmentsByYear { get; set; } = new();
        public List<ChartDataPoint> TopCategoriesByShipments { get; set; } = new();
        public List<ChartDataPoint> TopStatesByShipments { get; set; } = new();
        public List<ChartDataPoint> ShipmentsByDepartment { get; set; } = new();
    }

    public class YearShippingData
    {
        public string Year { get; set; } = "";
        public int ShipmentCount { get; set; }
    }

    public class StackedBarData
    {
        public string Segment { get; set; } = "";
        public string ShippingMode { get; set; } = "";
        public int Count { get; set; }
    }

    public class DeliveryPerformanceData
    {
        public string Year { get; set; } = "";
        public decimal DaysActual { get; set; }
        public decimal DaysScheduled { get; set; }

        // ✅ THÊM: Delivery Variance (Độ lệch giao hàng)
        public decimal DeliveryVariance => DaysActual - DaysScheduled;
    }

    // ✅ THÊM MỚI: Late Rate by Shipping Mode
    public class ShippingModeLateRateData
    {
        public string ShippingMode { get; set; } = "";
        public decimal LateDeliveryRate { get; set; }
        public int TotalShipments { get; set; }
        public int LateShipments { get; set; }
    }

    // ✅ THÊM MỚI: Cross-filter response cho Shipping KPIs
    public class ShippingKpiResponse
    {
        public int TotalShipments { get; set; }
        public decimal AverageDaysActual { get; set; }
        public decimal AverageDaysScheduled { get; set; }
        public decimal LateDeliveryRate { get; set; }
    }
    public class ShippingDashboardResponse
    {
        public int TotalShipments { get; set; }
        public string TopSegment { get; set; }
        public string TopShippingMode { get; set; }

        public List<ChartDataPoint> ShipmentsByDeliveryStatus { get; set; }
        public List<ChartDataPoint> ShipmentsBySegment { get; set; }
        public List<ChartDataPoint> ShipmentsByShippingMode { get; set; }
        public List<ChartDataPoint> ShipmentsByQuarterTrend { get; set; }
        public List<StackedBarData> ShippingModeBySegment { get; set; }
        public List<YearShippingData> ShipmentsByYear { get; set; }
        public List<ChartDataPoint> TopCategoriesByShipments { get; set; }
        public List<ChartDataPoint> TopStatesByShipments { get; set; }
        public List<ChartDataPoint> ShipmentsByDepartment { get; set; }
    }
}