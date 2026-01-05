using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AnalysisServices.AdomdClient;
using System.Data;
using DataCo_Website.Areas.Admin.Models;

namespace DataCo_Website.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ShippingDashboardController : Controller
    {
        private readonly string _connectionString;
        private readonly ILogger<ShippingDashboardController> _logger;

        public ShippingDashboardController(IConfiguration configuration, ILogger<ShippingDashboardController> logger)
        {
            _connectionString = configuration.GetConnectionString("SSASConnection");
            _logger = logger;
        }

        // GET: Admin/ShippingDashboard
        public async Task<IActionResult> Index(ShippingFilters filters)
        {
            try
            {
                // Load available filter options
                ViewBag.AvailableYears = await GetAvailableYears();
                ViewBag.AvailableQuarters = new List<int> { 1, 2, 3, 4 };
                ViewBag.AvailableDeliveryStatus = await GetAvailableDeliveryStatus();
                ViewBag.AvailableSegments = await GetAvailableSegments();

                // Current filters
                ViewBag.Filters = filters;

                // Load dashboard data
                var dashboardData = new ShippingDashboardViewModel
                {
                    // Summary metrics
                    TotalShipments = await GetTotalShipments(filters),
                    AverageDaysActual = await GetAverageDaysActual(filters),
                    AverageDaysScheduled = await GetAverageDaysScheduled(filters),
                    LateDeliveryRate = await GetLateDeliveryRate(filters),

                    // Charts data
                    ShipmentsByDeliveryStatus = await GetShipmentsByDeliveryStatus(filters),
                    ShipmentsByYear = await GetShipmentsByYear(filters),
                    ShipmentsBySegment = await GetShipmentsBySegment(filters),
                    ShipmentsByShippingMode = await GetShipmentsByShippingMode(filters),
                    DeliveryPerformanceByYear = await GetDeliveryPerformanceByYear(filters),
                    TopCategoriesByShipments = await GetTopCategoriesByShipments(filters),
                    TopStatesByShipments = await GetTopStatesByShipments(filters),
                    ShipmentsByDepartment = await GetShipmentsByDepartment(filters)
                };

                return View(dashboardData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading shipping dashboard");
                TempData["Error"] = $"Không thể tải Shipping Dashboard: {ex.Message}";
                return View(new ShippingDashboardViewModel());
            }
        }

        // Get Available Years
        private async Task<List<int>> GetAvailableYears()
        {
            string mdxQuery = @"
                SELECT 
                    [Measures].[Fact Shipping Count] ON COLUMNS,
                    NON EMPTY [Dim Date 1].[Year].[Year].MEMBERS ON ROWS
                FROM [DataCo Shipping]
            ";

            var table = await ExecuteMDXQuery(mdxQuery);
            var years = new List<int>();

            foreach (DataRow row in table.Rows)
            {
                if (row["Year"] != DBNull.Value && int.TryParse(row["Year"].ToString(), out int year))
                {
                    years.Add(year);
                }
            }

            return years.OrderByDescending(y => y).ToList();
        }

        // Get Available Delivery Status
        private async Task<List<string>> GetAvailableDeliveryStatus()
        {
            string mdxQuery = @"
                SELECT 
                    [Measures].[Fact Shipping Count] ON COLUMNS,
                    NON EMPTY [Fact Shipping].[Delivery Status].[Delivery Status].MEMBERS ON ROWS
                FROM [DataCo Shipping]
            ";

            var table = await ExecuteMDXQuery(mdxQuery);
            var statuses = new List<string>();

            foreach (DataRow row in table.Rows)
            {
                if (row["Delivery Status"] != DBNull.Value && !string.IsNullOrEmpty(row["Delivery Status"].ToString()))
                {
                    statuses.Add(row["Delivery Status"].ToString()!);
                }
            }

            return statuses;
        }

        // Get Available Segments
        private async Task<List<string>> GetAvailableSegments()
        {
            string mdxQuery = @"
                SELECT 
                    [Measures].[Fact Shipping Count] ON COLUMNS,
                    NON EMPTY [Dim Customer 1].[Segment].[Segment].MEMBERS ON ROWS
                FROM [DataCo Shipping]
            ";

            var table = await ExecuteMDXQuery(mdxQuery);
            var segments = new List<string>();

            foreach (DataRow row in table.Rows)
            {
                if (row["Segment"] != DBNull.Value && !string.IsNullOrEmpty(row["Segment"].ToString()))
                {
                    segments.Add(row["Segment"].ToString()!);
                }
            }

            return segments;
        }

        // 1. Total Shipments
        private async Task<int> GetTotalShipments(ShippingFilters filters)
        {
            string mdxQuery = $@"
                SELECT 
                    [Measures].[Fact Shipping Count] ON COLUMNS 
                FROM [DataCo Shipping]
                {BuildWhereClause(filters)}
            ";

            var table = await ExecuteMDXQuery(mdxQuery);
            if (table.Rows.Count > 0)
            {
                return table.Rows[0][0] != DBNull.Value ? Convert.ToInt32(table.Rows[0][0]) : 0;
            }
            return 0;
        }

        // 2. Average Days Actual
        private async Task<decimal> GetAverageDaysActual(ShippingFilters filters)
        {
            string mdxQuery = $@"
                SELECT 
                    [Measures].[Days Actual] ON COLUMNS 
                FROM [DataCo Shipping]
                {BuildWhereClause(filters)}
            ";

            var table = await ExecuteMDXQuery(mdxQuery);
            if (table.Rows.Count > 0)
            {
                return table.Rows[0][0] != DBNull.Value ? Convert.ToDecimal(table.Rows[0][0]) : 0;
            }
            return 0;
        }

        // 3. Average Days Scheduled
        private async Task<decimal> GetAverageDaysScheduled(ShippingFilters filters)
        {
            string mdxQuery = $@"
                SELECT 
                    [Measures].[Days Scheduled] ON COLUMNS 
                FROM [DataCo Shipping]
                {BuildWhereClause(filters)}
            ";

            var table = await ExecuteMDXQuery(mdxQuery);
            if (table.Rows.Count > 0)
            {
                return table.Rows[0][0] != DBNull.Value ? Convert.ToDecimal(table.Rows[0][0]) : 0;
            }
            return 0;
        }

        // 4. Late Delivery Rate
        private async Task<decimal> GetLateDeliveryRate(ShippingFilters filters)
        {
            string mdxQuery = $@"
                WITH 
                MEMBER [Measures].[Late Deliveries] AS 
                    ([Fact Shipping].[Late Delivery Risk].[1], [Measures].[Fact Shipping Count])
                MEMBER [Measures].[Late Rate] AS 
                    IIF([Measures].[Fact Shipping Count] = 0, 0, 
                        [Measures].[Late Deliveries] / [Measures].[Fact Shipping Count] * 100)
                SELECT 
                    [Measures].[Late Rate] ON COLUMNS 
                FROM [DataCo Shipping]
                {BuildWhereClause(filters)}
            ";

            var table = await ExecuteMDXQuery(mdxQuery);
            if (table.Rows.Count > 0)
            {
                return table.Rows[0][0] != DBNull.Value ? Convert.ToDecimal(table.Rows[0][0]) : 0;
            }
            return 0;
        }

        // 5. Shipments by Delivery Status
        private async Task<List<ChartDataPoint>> GetShipmentsByDeliveryStatus(ShippingFilters filters)
        {
            string deliveryStatusSet;

            // Nếu có filter Delivery Status, chỉ show những status đó
            if (filters.DeliveryStatus != null && filters.DeliveryStatus.Any())
            {
                deliveryStatusSet = "{" + string.Join(",",
                    filters.DeliveryStatus.Select(s => $"[Fact Shipping].[Delivery Status].&[{s}]")) + "}";
            }
            else
            {
                deliveryStatusSet = "[Fact Shipping].[Delivery Status].[Delivery Status].MEMBERS";
            }

            // Exclude Delivery Status khỏi WHERE vì đã xử lý ở ROWS
            string whereClause = BuildWhereClause(filters, excludeDeliveryStatus: true);

            string mdxQuery = $@"
                SELECT 
                    [Measures].[Fact Shipping Count] ON COLUMNS,
                    NON EMPTY ORDER(
                        {deliveryStatusSet},
                        [Measures].[Fact Shipping Count],
                        BDESC
                    ) ON ROWS
                FROM [DataCo Shipping]
                {whereClause}
            ";

            var data = await ExecuteMDXQuery(mdxQuery);
            return ParseChartData(data, "Delivery Status", "Fact Shipping Count");
        }

        // 6. Shipments by Year
        private async Task<List<YearShippingData>> GetShipmentsByYear(ShippingFilters filters)
        {
            string yearSetOnRows;

            if (filters.Years != null && filters.Years.Any())
            {
                yearSetOnRows = "{" + string.Join(",", filters.Years.Select(y => $"[Dim Date 1].[Year].&[{y}]")) + "}";
            }
            else
            {
                yearSetOnRows = "[Dim Date 1].[Year].[Year].MEMBERS";
            }

            string whereClause = BuildWhereClause(filters, excludeYear: true);

            string mdxQuery = $@"
                SELECT
                    {{[Measures].[Fact Shipping Count], [Measures].[Days Actual], [Measures].[Days Scheduled]}} ON COLUMNS,
                    NON EMPTY ORDER(
                        {yearSetOnRows},
                        [Dim Date 1].[Year].CURRENTMEMBER.MEMBER_CAPTION,
                        ASC
                    ) ON ROWS
                FROM [DataCo Shipping]
                {whereClause}
            ";

            var data = await ExecuteMDXQuery(mdxQuery);
            return ParseYearShippingData(data);
        }

        // 7. Shipments by Segment
        private async Task<List<ChartDataPoint>> GetShipmentsBySegment(ShippingFilters filters)
        {
            string segmentSet;

            // Nếu có filter Segment, chỉ show những segment đó
            if (filters.Segments != null && filters.Segments.Any())
            {
                segmentSet = "{" + string.Join(",",
                    filters.Segments.Select(s => $"[Dim Customer 1].[Segment].&[{s}]")) + "}";
            }
            else
            {
                segmentSet = "[Dim Customer 1].[Segment].[Segment].MEMBERS";
            }

            // Exclude Segment khỏi WHERE vì đã xử lý ở ROWS
            string whereClause = BuildWhereClause(filters, excludeSegment: true);

            string mdxQuery = $@"
                SELECT 
                    [Measures].[Fact Shipping Count] ON COLUMNS,
                    NON EMPTY ORDER(
                        {segmentSet},
                        [Measures].[Fact Shipping Count],
                        BDESC
                    ) ON ROWS
                FROM [DataCo Shipping]
                {whereClause}
            ";

            var data = await ExecuteMDXQuery(mdxQuery);
            return ParseChartData(data, "Segment", "Fact Shipping Count");
        }

        // 8. Shipments by Shipping Mode
        private async Task<List<ChartDataPoint>> GetShipmentsByShippingMode(ShippingFilters filters)
        {
            string mdxQuery = $@"
                SELECT 
                    [Measures].[Fact Shipping Count] ON COLUMNS,
                    NON EMPTY ORDER(
                        [Fact Shipping].[Shipping Mode].[Shipping Mode].MEMBERS,
                        [Measures].[Fact Shipping Count],
                        BDESC
                    ) ON ROWS
                FROM [DataCo Shipping]
                {BuildWhereClause(filters)}
            ";

            var data = await ExecuteMDXQuery(mdxQuery);
            return ParseChartData(data, "Shipping Mode", "Fact Shipping Count");
        }

        // 9. Delivery Performance by Year (Days Actual vs Scheduled)
        private async Task<List<DeliveryPerformanceData>> GetDeliveryPerformanceByYear(ShippingFilters filters)
        {
            // Exclude Year từ WHERE clause vì Year đã ở ROWS
            string whereClause = BuildWhereClause(filters, excludeYear: true);

            string mdxQuery = $@"
                SELECT
                    {{[Measures].[Days Actual], [Measures].[Days Scheduled]}} ON COLUMNS,
                    NON EMPTY ORDER(
                        [Dim Date 1].[Year].[Year].MEMBERS,
                        [Dim Date 1].[Year].CURRENTMEMBER.MEMBER_CAPTION,
                        ASC
                    ) ON ROWS
                FROM [DataCo Shipping]
                {whereClause}
            ";

            var data = await ExecuteMDXQuery(mdxQuery);
            return ParseDeliveryPerformanceData(data);
        }

        // 10. Top Categories by Shipments (Top 20)
        private async Task<List<ChartDataPoint>> GetTopCategoriesByShipments(ShippingFilters filters)
        {
            string mdxQuery = $@"
                SELECT 
                    [Measures].[Fact Shipping Count] ON COLUMNS,
                    NON EMPTY TOPCOUNT(
                        [Dim Product 1].[Category Name].[Category Name].MEMBERS,
                        20,
                        [Measures].[Fact Shipping Count]
                    ) ON ROWS
                FROM [DataCo Shipping]
                {BuildWhereClause(filters)}
            ";

            var data = await ExecuteMDXQuery(mdxQuery);
            return ParseChartData(data, "Category Name", "Fact Shipping Count");
        }

        // 11. Top States by Shipments (Top 20)
        private async Task<List<ChartDataPoint>> GetTopStatesByShipments(ShippingFilters filters)
        {
            string mdxQuery = $@"
                SELECT 
                    [Measures].[Fact Shipping Count] ON COLUMNS,
                    NON EMPTY TOPCOUNT(
                        [Dim Customer 1].[State].[State].MEMBERS,
                        20,
                        [Measures].[Fact Shipping Count]
                    ) ON ROWS
                FROM [DataCo Shipping]
                {BuildWhereClause(filters)}
            ";

            var data = await ExecuteMDXQuery(mdxQuery);
            return ParseChartData(data, "State", "Fact Shipping Count");
        }

        // 12. Shipments by Department
        private async Task<List<ChartDataPoint>> GetShipmentsByDepartment(ShippingFilters filters)
        {
            string mdxQuery = $@"
                SELECT 
                    [Measures].[Fact Shipping Count] ON COLUMNS,
                    NON EMPTY ORDER(
                        [Dim Department 1].[Department Name].[Department Name].MEMBERS,
                        [Measures].[Fact Shipping Count],
                        BDESC
                    ) ON ROWS
                FROM [DataCo Shipping]
                {BuildWhereClause(filters)}
            ";

            var data = await ExecuteMDXQuery(mdxQuery);
            return ParseChartData(data, "Department Name", "Fact Shipping Count");
        }

        // Build WHERE clause from filters
        private string BuildWhereClause(
            ShippingFilters filters,
            bool excludeYear = false,
            bool excludeDeliveryStatus = false,
            bool excludeSegment = false)  // ← THÊM PARAMETER NÀY
        {
            var conditions = new List<string>();

            // 1. Filter Years
            if (!excludeYear && filters.Years != null && filters.Years.Any())
            {
                var members = string.Join(", ", filters.Years.Select(y => $"[Dim Date 1].[Year].&[{y}]"));
                conditions.Add($"{{{members}}}");
            }

            // 2. Filter Quarters
            if (filters.Quarters != null && filters.Quarters.Any())
            {
                var members = string.Join(", ", filters.Quarters.Select(q => $"[Dim Date 1].[Quarter].&[{q}]"));
                conditions.Add($"{{{members}}}");
            }

            // 3. Filter Delivery Status
            if (!excludeDeliveryStatus && filters.DeliveryStatus != null && filters.DeliveryStatus.Any())
            {
                var members = string.Join(", ", filters.DeliveryStatus.Select(s => $"[Fact Shipping].[Delivery Status].&[{s}]"));
                conditions.Add($"{{{members}}}");
            }

            // 4. Filter Segments
            if (!excludeSegment && filters.Segments != null && filters.Segments.Any())  // ← SỬA DÒNG NÀY
            {
                var members = string.Join(", ", filters.Segments.Select(s => $"[Dim Customer 1].[Segment].&[{s}]"));
                conditions.Add($"{{{members}}}");
            }

            if (!conditions.Any()) return "";
            if (conditions.Count == 1) return $"WHERE {conditions[0]}";

            string result = conditions[0];
            for (int i = 1; i < conditions.Count; i++)
            {
                result = $"CROSSJOIN({result}, {conditions[i]})";
            }

            return $"WHERE ({result})";
        }

        // Helper: Execute MDX Query
        private async Task<DataTable> ExecuteMDXQuery(string mdxQuery)
        {
            var table = new DataTable();

            await Task.Run(() =>
            {
                using var conn = new AdomdConnection(_connectionString);
                conn.Open();

                using var cmd = new AdomdCommand(mdxQuery, conn);
                cmd.CommandTimeout = 60;
                using var adapter = new AdomdDataAdapter(cmd);
                adapter.Fill(table);
            });

            CleanColumnNames(table);
            return table;
        }

        // Helper: Clean column names
        private void CleanColumnNames(DataTable table)
        {
            foreach (DataColumn col in table.Columns)
            {
                string originalName = col.ColumnName;

                if (originalName.Contains("MEMBER_CAPTION"))
                {
                    var match = System.Text.RegularExpressions.Regex.Match(
                        originalName,
                        @"\[([^\]]+)\]\.\[([^\]]+)\]"
                    );

                    if (match.Success && match.Groups.Count >= 3)
                    {
                        col.ColumnName = match.Groups[2].Value;
                    }
                    else
                    {
                        col.ColumnName = "Name";
                    }
                }
                else if (originalName.Contains("[Measures]"))
                {
                    col.ColumnName = originalName
                        .Replace("[Measures].[", "")
                        .Replace("]", "")
                        .Replace("[", "");
                }
            }
        }

        // Parsing methods
        private List<ChartDataPoint> ParseChartData(DataTable table, string labelColumn, string valueColumn)
        {
            var result = new List<ChartDataPoint>();
            foreach (DataRow row in table.Rows)
            {
                result.Add(new ChartDataPoint
                {
                    Label = row[labelColumn]?.ToString() ?? "Unknown",
                    Value = row[valueColumn] != DBNull.Value ? Convert.ToDecimal(row[valueColumn]) : 0
                });
            }
            return result;
        }

        private List<YearShippingData> ParseYearShippingData(DataTable table)
        {
            var result = new List<YearShippingData>();
            foreach (DataRow row in table.Rows)
            {
                result.Add(new YearShippingData
                {
                    Year = row["Year"]?.ToString() ?? "Unknown",
                    ShipmentCount = row["Fact Shipping Count"] != DBNull.Value ? Convert.ToInt32(row["Fact Shipping Count"]) : 0,
                    DaysActual = row["Days Actual"] != DBNull.Value ? Convert.ToDecimal(row["Days Actual"]) : 0,
                    DaysScheduled = row["Days Scheduled"] != DBNull.Value ? Convert.ToDecimal(row["Days Scheduled"]) : 0
                });
            }
            return result;
        }

        private List<DeliveryPerformanceData> ParseDeliveryPerformanceData(DataTable table)
        {
            var result = new List<DeliveryPerformanceData>();
            foreach (DataRow row in table.Rows)
            {
                result.Add(new DeliveryPerformanceData
                {
                    Year = row["Year"]?.ToString() ?? "Unknown",
                    DaysActual = row["Days Actual"] != DBNull.Value ? Convert.ToDecimal(row["Days Actual"]) : 0,
                    DaysScheduled = row["Days Scheduled"] != DBNull.Value ? Convert.ToDecimal(row["Days Scheduled"]) : 0
                });
            }
            return result;
        }
    }
    /*
    // Models
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

    public class ChartDataPoint
    {
        public string Label { get; set; } = "";
        public decimal Value { get; set; }
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
    }*/
}