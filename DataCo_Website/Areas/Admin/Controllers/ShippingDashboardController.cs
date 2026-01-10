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
                // Load Dropdown Data
                ViewBag.AvailableYears = await GetAvailableYears();
                ViewBag.AvailableQuarters = new List<int> { 1, 2, 3, 4 };
                ViewBag.AvailableDepartments = await GetAvailableDepartments();
                ViewBag.Filters = filters;

                // Load Data
                var dashboardData = await LoadDashboardData(filters);
                return View(dashboardData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading shipping dashboard");
                return View(new ShippingDashboardViewModel());
            }
        }

        // API: Handle Cross-Filtering (Cập nhật logic cộng dồn filter)
        [HttpPost]
        public async Task<IActionResult> GetCrossFilterData([FromBody] CrossFilterRequest request)
        {
            try
            {
                // 1. Khởi tạo filter từ Form chính (Năm, Quý)
                var filters = new ShippingFilters();
                if (request.FormFilters != null)
                {
                    filters.Years = request.FormFilters.Years;
                    filters.Quarters = request.FormFilters.Quarters;
                    filters.Departments = request.FormFilters.Departments;
                }

                // 2. Áp dụng CỘNG DỒN các cross-filters từ dictionary
                if (request.Filters.ContainsKey("deliveryStatus") && !string.IsNullOrEmpty(request.Filters["deliveryStatus"]))
                    filters.DeliveryStatus = new List<string> { request.Filters["deliveryStatus"]! };

                if (request.Filters.ContainsKey("segment") && !string.IsNullOrEmpty(request.Filters["segment"]))
                    filters.Segments = new List<string> { request.Filters["segment"]! };

                if (request.Filters.ContainsKey("shippingMode") && !string.IsNullOrEmpty(request.Filters["shippingMode"]))
                    filters.ShippingModes = new List<string> { request.Filters["shippingMode"]! };

                // 3. Load lại toàn bộ data với bộ filter mới
                var data = await LoadDashboardData(filters);

                // 4. Map sang Response Object
                var response = new ShippingDashboardResponse
                {
                    TotalShipments = data.TotalShipments,
                    TopSegment = data.TopSegment,
                    TopShippingMode = data.TopShippingMode,

                    ShipmentsByDeliveryStatus = data.ShipmentsByDeliveryStatus,
                    ShipmentsBySegment = data.ShipmentsBySegment,
                    ShipmentsByShippingMode = data.ShipmentsByShippingMode,
                    ShipmentsByQuarterTrend = data.ShipmentsByQuarterTrend,
                    ShippingModeBySegment = data.ShippingModeBySegment,
                    ShipmentsByYear = data.ShipmentsByYear,
                    TopCategoriesByShipments = data.TopCategoriesByShipments,
                    TopStatesByShipments = data.TopStatesByShipments,
                    ShipmentsByDepartment = data.ShipmentsByDepartment
                };

                return Json(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in cross-filter");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // Helper function để load data (dùng chung cho Index và API)
        private async Task<ShippingDashboardViewModel> LoadDashboardData(ShippingFilters filters)
        {
            var segmentsData = await GetShipmentsBySegment(filters);
            var modesData = await GetShipmentsByShippingMode(filters);

            return new ShippingDashboardViewModel
            {
                TotalShipments = await GetTotalShipments(filters),
                // Logic tìm Top Segment/Mode: lấy phần tử có value cao nhất
                TopSegment = segmentsData.OrderByDescending(x => x.Value).FirstOrDefault()?.Label ?? "N/A",
                TopShippingMode = modesData.OrderByDescending(x => x.Value).FirstOrDefault()?.Label ?? "N/A",

                ShipmentsByDeliveryStatus = await GetShipmentsByDeliveryStatus(filters),
                ShipmentsBySegment = segmentsData,
                ShipmentsByShippingMode = modesData,
                ShipmentsByQuarterTrend = await GetShipmentsByQuarterTrend(filters),
                ShippingModeBySegment = await GetShippingModeBySegment(filters),
                ShipmentsByYear = await GetShipmentsByYear(filters),
                TopCategoriesByShipments = await GetTopCategoriesByShipments(filters),
                TopStatesByShipments = await GetTopStatesByShipments(filters),
                ShipmentsByDepartment = await GetShipmentsByDepartment(filters)
            };
        }

        // ================== MDX QUERIES ==================

        // 1. Total Shipments
        private async Task<int> GetTotalShipments(ShippingFilters filters)
        {
            string mdxQuery = $"SELECT [Measures].[Fact Shipping Count] ON COLUMNS FROM [DataCo Shipping] {BuildWhereClause(filters)}";
            var table = await ExecuteMDXQuery(mdxQuery);
            return table.Rows.Count > 0 && table.Rows[0][0] != DBNull.Value ? Convert.ToInt32(table.Rows[0][0]) : 0;
        }

        // 2. Status (Donut)
        private async Task<List<ChartDataPoint>> GetShipmentsByDeliveryStatus(ShippingFilters filters)
        {
            // Truyền excludeDeliveryStatus: true
            string mdxQuery = $@"
            SELECT [Measures].[Fact Shipping Count] ON COLUMNS,
            NON EMPTY ORDER([Fact Shipping].[Delivery Status].[Delivery Status].MEMBERS, [Measures].[Fact Shipping Count], BDESC) ON ROWS
            FROM [DataCo Shipping] {BuildWhereClause(filters, excludeDeliveryStatus: true)}";

            return ParseChartData(await ExecuteMDXQuery(mdxQuery), "Delivery Status", "Fact Shipping Count");
        }

        // 3. Segment (Pie)
        private async Task<List<ChartDataPoint>> GetShipmentsBySegment(ShippingFilters filters)
        {
            // Truyền excludeSegment: true
            string mdxQuery = $@"
            SELECT [Measures].[Fact Shipping Count] ON COLUMNS,
            NON EMPTY ORDER([Dim Customer 1].[Segment].[Segment].MEMBERS, [Measures].[Fact Shipping Count], BDESC) ON ROWS
            FROM [DataCo Shipping] {BuildWhereClause(filters, excludeSegment: true)}";

            return ParseChartData(await ExecuteMDXQuery(mdxQuery), "Segment", "Fact Shipping Count");
        }

        // 4. Shipping Mode (Donut)
        private async Task<List<ChartDataPoint>> GetShipmentsByShippingMode(ShippingFilters filters)
        {
            // Truyền excludeShippingMode: true
            string mdxQuery = $@"
            SELECT [Measures].[Fact Shipping Count] ON COLUMNS,
            NON EMPTY ORDER([Fact Shipping].[Shipping Mode].[Shipping Mode].MEMBERS, [Measures].[Fact Shipping Count], BDESC) ON ROWS
            FROM [DataCo Shipping] {BuildWhereClause(filters, excludeShippingMode: true)}";

            return ParseChartData(await ExecuteMDXQuery(mdxQuery), "Shipping Mode", "Fact Shipping Count");
        }

        // 5. NEW: Quarter Trend (Line Chart)
        private async Task<List<ChartDataPoint>> GetShipmentsByQuarterTrend(ShippingFilters filters)
        {
            // Truyền excludeQuarter: true
            string mdxQuery = $@"
            SELECT [Measures].[Fact Shipping Count] ON COLUMNS,
            NON EMPTY [Dim Date 1].[Quarter].[Quarter].MEMBERS ON ROWS
            FROM [DataCo Shipping] {BuildWhereClause(filters, excludeQuarter: true)}";

            return ParseChartData(await ExecuteMDXQuery(mdxQuery), "Quarter", "Fact Shipping Count");
        }

        // 6. NEW: Mode by Segment (Stacked Bar)
        private async Task<List<StackedBarData>> GetShippingModeBySegment(ShippingFilters filters)
        {
            string mdxQuery = $@"
            SELECT [Measures].[Fact Shipping Count] ON COLUMNS,
            NON EMPTY CROSSJOIN(
                [Dim Customer 1].[Segment].[Segment].MEMBERS,
                [Fact Shipping].[Shipping Mode].[Shipping Mode].MEMBERS
            ) ON ROWS
            FROM [DataCo Shipping] {BuildWhereClause(filters, excludeSegment: true, excludeShippingMode: true)}";
                        
            var table = await ExecuteMDXQuery(mdxQuery);
            var result = new List<StackedBarData>();
            foreach (DataRow row in table.Rows)
            {
                result.Add(new StackedBarData
                {
                    Segment = row["Segment"]?.ToString() ?? "Unknown",
                    ShippingMode = row["Shipping Mode"]?.ToString() ?? "Unknown",
                    Count = row["Fact Shipping Count"] != DBNull.Value ? Convert.ToInt32(row["Fact Shipping Count"]) : 0
                });
            }
            return result;
        }

        // 7. Year (Bar)
        private async Task<List<YearShippingData>> GetShipmentsByYear(ShippingFilters filters)
        {
            string mdxQuery = $@"
                SELECT [Measures].[Fact Shipping Count] ON COLUMNS,
                NON EMPTY ORDER([Dim Date 1].[Year].[Year].MEMBERS, [Dim Date 1].[Year].CURRENTMEMBER.MEMBER_CAPTION, ASC) ON ROWS
                FROM [DataCo Shipping] {BuildWhereClause(filters, excludeYear: true)}";

            var table = await ExecuteMDXQuery(mdxQuery);
            var result = new List<YearShippingData>();
            foreach (DataRow row in table.Rows)
            {
                result.Add(new YearShippingData
                {
                    Year = row["Year"]?.ToString() ?? "",
                    ShipmentCount = row["Fact Shipping Count"] != DBNull.Value ? Convert.ToInt32(row["Fact Shipping Count"]) : 0
                });
            }
            return result;
        }

        // 8. Categories (Top 20)
        private async Task<List<ChartDataPoint>> GetTopCategoriesByShipments(ShippingFilters filters)
        {
            string mdxQuery = $"SELECT [Measures].[Fact Shipping Count] ON COLUMNS, NON EMPTY TOPCOUNT([Dim Product 1].[Category Name].[Category Name].MEMBERS, 20, [Measures].[Fact Shipping Count]) ON ROWS FROM [DataCo Shipping] {BuildWhereClause(filters)}";
            return ParseChartData(await ExecuteMDXQuery(mdxQuery), "Category Name", "Fact Shipping Count");
        }

        // 9. States (Top 20)
        private async Task<List<ChartDataPoint>> GetTopStatesByShipments(ShippingFilters filters)
        {
            string mdxQuery = $"SELECT [Measures].[Fact Shipping Count] ON COLUMNS, NON EMPTY TOPCOUNT([Dim Customer 1].[State].[State].MEMBERS, 20, [Measures].[Fact Shipping Count]) ON ROWS FROM [DataCo Shipping] {BuildWhereClause(filters)}";
            return ParseChartData(await ExecuteMDXQuery(mdxQuery), "State", "Fact Shipping Count");
        }

        // 10. Department
        private async Task<List<ChartDataPoint>> GetShipmentsByDepartment(ShippingFilters filters)
        {
            // Truyền excludeDepartment: true để tránh lỗi xung đột trục
            string mdxQuery = $"SELECT [Measures].[Fact Shipping Count] ON COLUMNS, NON EMPTY ORDER([Dim Department 1].[Department Name].[Department Name].MEMBERS, [Measures].[Fact Shipping Count], BDESC) ON ROWS FROM [DataCo Shipping] {BuildWhereClause(filters, excludeDepartment: true)}";
            return ParseChartData(await ExecuteMDXQuery(mdxQuery), "Department Name", "Fact Shipping Count");
        }

        // ================== BUILD WHERE CLAUSE ==================
        private string BuildWhereClause(
        ShippingFilters filters,
        bool excludeYear = false,
        bool excludeQuarter = false,
        bool excludeDeliveryStatus = false, // Thêm
        bool excludeSegment = false,        // Thêm
        bool excludeShippingMode = false,
        bool excludeDepartment = false)   // Thêm
        {
            var conditions = new List<string>();

            // Year
            if (!excludeYear && filters.Years != null && filters.Years.Any())
                conditions.Add($"{{{string.Join(", ", filters.Years.Select(y => $"[Dim Date 1].[Year].&[{y}]"))}}}");

            // Quarter (Thêm logic exclude)
            if (!excludeQuarter && filters.Quarters != null && filters.Quarters.Any())
                conditions.Add($"{{{string.Join(", ", filters.Quarters.Select(q => $"[Dim Date 1].[Quarter].&[{q}]"))}}}");

            // Department
            if (!excludeDepartment && filters.Departments != null && filters.Departments.Any())
                conditions.Add($"{{{string.Join(", ", filters.Departments.Select(d => $"[Dim Department 1].[Department Name].[{d}]"))}}}");

            // Delivery Status (Thêm logic exclude)
            if (!excludeDeliveryStatus && filters.DeliveryStatus != null && filters.DeliveryStatus.Any())
                conditions.Add($"{{{string.Join(", ", filters.DeliveryStatus.Select(s => $"[Fact Shipping].[Delivery Status].&[{s}]"))}}}");

            // Segment (Thêm logic exclude) -> ĐÂY LÀ CHỖ FIX LỖI CỦA BẠN
            if (!excludeSegment && filters.Segments != null && filters.Segments.Any())
                conditions.Add($"{{{string.Join(", ", filters.Segments.Select(s => $"[Dim Customer 1].[Segment].&[{s}]"))}}}");

            // Shipping Mode (Thêm logic exclude)
            if (!excludeShippingMode && filters.ShippingModes != null && filters.ShippingModes.Any())
                conditions.Add($"{{{string.Join(", ", filters.ShippingModes.Select(s => $"[Fact Shipping].[Shipping Mode].&[{s}]"))}}}");

            if (!conditions.Any()) return "";
            if (conditions.Count == 1) return $"WHERE {conditions[0]}";

            // CrossJoin tất cả các điều kiện lại
            string result = conditions[0];
            for (int i = 1; i < conditions.Count; i++) result = $"CROSSJOIN({result}, {conditions[i]})";
            return $"WHERE ({result})";
        }

        // ================== HELPERS ==================
        private List<ChartDataPoint> ParseChartData(DataTable table, string labelCol, string valCol)
        {
            var list = new List<ChartDataPoint>();
            foreach (DataRow r in table.Rows) list.Add(new ChartDataPoint { Label = r[labelCol]?.ToString() ?? "", Value = r[valCol] != DBNull.Value ? Convert.ToDecimal(r[valCol]) : 0 });
            return list;
        }

        private async Task<List<int>> GetAvailableYears()
        {
            string mdxQuery = @"SELECT [Measures].[Fact Shipping Count] ON COLUMNS, NON EMPTY [Dim Date 1].[Year].[Year].MEMBERS ON ROWS FROM [DataCo Shipping]";
            var table = await ExecuteMDXQuery(mdxQuery);
            var years = new List<int>();
            foreach (DataRow row in table.Rows) { if (row["Year"] != DBNull.Value && int.TryParse(row["Year"].ToString(), out int year)) years.Add(year); }
            return years.OrderByDescending(y => y).ToList();
        }

        private async Task<List<string>> GetAvailableDeliveryStatus()
        {
            return await GetDimensionMembers("[Fact Shipping].[Delivery Status].[Delivery Status]");
        }
        private async Task<List<string>> GetAvailableSegments()
        {
            return await GetDimensionMembers("[Dim Customer 1].[Segment].[Segment]");
        }

        private async Task<List<string>> GetDimensionMembers(string hierarchy)
        {
            string mdxQuery = $"SELECT [Measures].[Fact Shipping Count] ON COLUMNS, NON EMPTY {hierarchy}.MEMBERS ON ROWS FROM [DataCo Shipping]";
            var table = await ExecuteMDXQuery(mdxQuery);
            var list = new List<string>();
            string colName = hierarchy.Split('.').Last().Replace("]", "").Replace("[", ""); // Hacky parse
            foreach (DataColumn col in table.Columns) if (col.ColumnName != "Fact Shipping Count") { colName = col.ColumnName; break; }

            foreach (DataRow row in table.Rows)
            {
                if (row[colName] != DBNull.Value) list.Add(row[colName].ToString()!);
            }
            return list;
        }

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

        private void CleanColumnNames(DataTable table)
        {
            foreach (DataColumn col in table.Columns)
            {
                string originalName = col.ColumnName;
                if (originalName.Contains("MEMBER_CAPTION"))
                {
                    var match = System.Text.RegularExpressions.Regex.Match(originalName, @"\[([^\]]+)\]\.\[([^\]]+)\]");
                    if (match.Success && match.Groups.Count >= 3) col.ColumnName = match.Groups[2].Value;
                    else col.ColumnName = "Name";
                }
                else if (originalName.Contains("[Measures]"))
                {
                    col.ColumnName = originalName.Replace("[Measures].[", "").Replace("]", "").Replace("[", "");
                }
            }
        }
        private async Task<List<string>> GetAvailableDepartments()
        {
            return await GetDimensionMembers("[Dim Department 1].[Department Name].[Department Name]");
        }
    }
}