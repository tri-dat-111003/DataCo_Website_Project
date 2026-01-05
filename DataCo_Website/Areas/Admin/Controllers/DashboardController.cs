using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AnalysisServices.AdomdClient;
using System.Data;
using DataCo_Website.Areas.Admin.Models;

namespace DataCo_Website.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class DashboardController : Controller
    {
        private readonly string _connectionString;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(IConfiguration configuration, ILogger<DashboardController> logger)
        {
            _connectionString = configuration.GetConnectionString("SSASConnection");
            _logger = logger;
        }

        // GET: Admin/Dashboard
        public async Task<IActionResult> Index(DashboardFilters filters)
        {
            try
            {
                // Load available filter options
                ViewBag.AvailableYears = await GetAvailableYears();
                ViewBag.AvailableQuarters = new List<int> { 1, 2, 3, 4 };
                ViewBag.AvailableMarkets = await GetAvailableMarkets();
                ViewBag.AvailableDepartments = await GetAvailableDepartments();

                // Current filters
                ViewBag.Filters = filters;

                // Load dashboard data
                var dashboardData = new DashboardViewModel
                {
                    // Summary metrics
                    TotalProfit = await GetTotalProfit(filters),
                    TotalRevenue = await GetTotalRevenue(filters),

                    // Charts data
                    ProfitBySegment = await GetProfitBySegment(filters),
                    OrderCountByStatus = await GetOrderCountByStatus(filters),
                    TransactionCountByType = await GetTransactionCountByType(filters),
                    RevenueByMarket = await GetRevenueByMarket(filters),
                    RevenueByYear = await GetRevenueByYear(filters),
                    RevenueByDepartment = await GetRevenueByDepartment(filters),
                    RevenueByCategory = await GetRevenueByCategory(filters),
                    ProfitByState = await GetProfitByState(filters)
                };

                return View(dashboardData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading dashboard");
                TempData["Error"] = $"Không thể tải Dashboard: {ex.Message}";
                return View(new DashboardViewModel());
            }
        }

        // Get Available Years from Cube
        private async Task<List<int>> GetAvailableYears()
        {
            string mdxQuery = @"
                SELECT 
                    [Measures].[Profit] ON COLUMNS,
                    [Dim Date].[Year].[Year].MEMBERS ON ROWS
                FROM [DataCo Sales]
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

        // Get Available Markets
        private async Task<List<string>> GetAvailableMarkets()
        {
            string mdxQuery = @"
                SELECT 
                    [Measures].[Profit] ON COLUMNS,
                    [Fact Sales].[Market].[Market].MEMBERS ON ROWS
                FROM [DataCo Sales]
            ";

            var table = await ExecuteMDXQuery(mdxQuery);
            var markets = new List<string>();

            foreach (DataRow row in table.Rows)
            {
                if (row["Market"] != DBNull.Value && !string.IsNullOrEmpty(row["Market"].ToString()))
                {
                    markets.Add(row["Market"].ToString()!);
                }
            }

            return markets;
        }

        // Get Available Departments
        private async Task<List<string>> GetAvailableDepartments()
        {
            string mdxQuery = @"
                SELECT 
                    [Measures].[Profit] ON COLUMNS,
                    [Dim Department].[Department Name].[Department Name].MEMBERS ON ROWS
                FROM [DataCo Sales]
            ";

            var table = await ExecuteMDXQuery(mdxQuery);
            var departments = new List<string>();

            foreach (DataRow row in table.Rows)
            {
                if (row["Department Name"] != DBNull.Value && !string.IsNullOrEmpty(row["Department Name"].ToString()))
                {
                    departments.Add(row["Department Name"].ToString()!);
                }
            }

            return departments;
        }

        // 1. Total Profit
        private async Task<decimal> GetTotalProfit(DashboardFilters filters)
        {
            string mdxQuery = $@"
                SELECT 
                    [Measures].[Profit] ON COLUMNS 
                FROM [DataCo Sales]
                {BuildWhereClause(filters)}
            ";

            var table = await ExecuteMDXQuery(mdxQuery);
            if (table.Rows.Count > 0)
            {
                return table.Rows[0][0] != DBNull.Value ? Convert.ToDecimal(table.Rows[0][0]) : 0;
            }
            return 0;
        }

        // 2. Total Revenue
        private async Task<decimal> GetTotalRevenue(DashboardFilters filters)
        {
            string mdxQuery = $@"
                SELECT 
                    [Measures].[Total Price] ON COLUMNS 
                FROM [DataCo Sales]
                {BuildWhereClause(filters)}
            ";

            var table = await ExecuteMDXQuery(mdxQuery);
            if (table.Rows.Count > 0)
            {
                return table.Rows[0][0] != DBNull.Value ? Convert.ToDecimal(table.Rows[0][0]) : 0;
            }
            return 0;
        }

        // 3. Profit by Customer Segment
        private async Task<List<ChartDataPoint>> GetProfitBySegment(DashboardFilters filters)
        {
            string mdxQuery = $@"
                SELECT 
                    [Measures].[Profit] ON COLUMNS,
                    NON EMPTY ORDER(
                        [Dim Customer].[Segment].[Segment].MEMBERS,
                        [Measures].[Profit],
                        BDESC
                    ) ON ROWS
                FROM [DataCo Sales]
                {BuildWhereClause(filters)}
            ";

            var data = await ExecuteMDXQuery(mdxQuery);
            return ParseChartData(data, "Segment", "Profit");
        }

        // 4. Order Count by Status
        private async Task<List<ChartDataPoint>> GetOrderCountByStatus(DashboardFilters filters)
        {
            string mdxQuery = $@"
                SELECT 
                    [Measures].[Fact Sales Count] ON COLUMNS,
                    NON EMPTY ORDER(
                        [Fact Sales].[Status].[Status].MEMBERS,
                        [Measures].[Fact Sales Count],
                        BDESC
                    ) ON ROWS
                FROM [DataCo Sales]
                {BuildWhereClause(filters)}
            ";

            var data = await ExecuteMDXQuery(mdxQuery);
            return ParseChartData(data, "Status", "Fact Sales Count");
        }

        // 5. Transaction Count by Type
        private async Task<List<ChartDataPoint>> GetTransactionCountByType(DashboardFilters filters)
        {
            string mdxQuery = $@"
                SELECT 
                    [Measures].[Fact Sales Count] ON COLUMNS,
                    NON EMPTY ORDER(
                        [Fact Sales].[Type Transaction].[Type Transaction].MEMBERS,
                        [Measures].[Fact Sales Count],
                        BDESC
                    ) ON ROWS
                FROM [DataCo Sales]
                {BuildWhereClause(filters)}
            ";

            var data = await ExecuteMDXQuery(mdxQuery);
            return ParseChartData(data, "Type Transaction", "Fact Sales Count");
        }

        // 6. Revenue & Profit by Market
        private async Task<List<MarketChartData>> GetRevenueByMarket(DashboardFilters filters)
        {
            string marketSetOnRows;

            if (filters.Markets != null && filters.Markets.Any())
            {
                marketSetOnRows = "{" + string.Join(",", filters.Markets.Select(m => $"[Fact Sales].[Market].&[{m}]")) + "}";
            }
            else
            {
                marketSetOnRows = "[Fact Sales].[Market].[Market].MEMBERS";
            }

            string whereClause = BuildWhereClause(filters, excludeMarket: true);

            string mdxQuery = $@"
                SELECT 
                    {{[Measures].[Total Price], [Measures].[Profit]}} ON COLUMNS,
                    NON EMPTY ORDER(
                        {marketSetOnRows},
                        [Measures].[Total Price],
                        BDESC
                    ) ON ROWS
                FROM [DataCo Sales]
                {whereClause}
            ";

            var data = await ExecuteMDXQuery(mdxQuery);
            return ParseMarketData(data);
        }

        // 7. Revenue & Profit by Year
        private async Task<List<YearChartData>> GetRevenueByYear(DashboardFilters filters)
        {
            string yearSetOnRows;

            if (filters.Years != null && filters.Years.Any())
            {
                yearSetOnRows = "{" + string.Join(",", filters.Years.Select(y => $"[Dim Date].[Year].&[{y}]")) + "}";
            }
            else
            {
                yearSetOnRows = "FILTER([Dim Date].[Year].[Year].MEMBERS, NOT IsEmpty([Measures].[Total Price]) OR NOT IsEmpty([Measures].[Profit]))";
            }

            string whereClause = BuildWhereClause(filters, excludeYear: true);

            string mdxQuery = $@"
                SELECT
                    {{[Measures].[Total Price], [Measures].[Profit]}} ON COLUMNS,
                    NON EMPTY 
                    ORDER(
                        {yearSetOnRows},
                        [Dim Date].[Year].CURRENTMEMBER.MEMBER_CAPTION,
                        ASC
                    ) ON ROWS
                FROM [DataCo Sales]
                {whereClause}
            ";

            var data = await ExecuteMDXQuery(mdxQuery);
            return ParseYearData(data);
        }

        // 8. Revenue & Profit by Department
        private async Task<List<DepartmentChartData>> GetRevenueByDepartment(DashboardFilters filters)
        {
            string deptSetOnRows;

            if (filters.Departments != null && filters.Departments.Any())
            {
                deptSetOnRows = "{" + string.Join(",", filters.Departments.Select(d => $"[Dim Department].[Department Name].&[{d}]")) + "}";
            }
            else
            {
                deptSetOnRows = "[Dim Department].[Department Name].[Department Name].MEMBERS";
            }

            string whereClause = BuildWhereClause(filters, excludeDepartment: true);

            string mdxQuery = $@"
                SELECT 
                    {{[Measures].[Total Price], [Measures].[Profit]}} ON COLUMNS,
                    NON EMPTY ORDER(
                        {deptSetOnRows},
                        [Measures].[Total Price],
                        BDESC
                    ) ON ROWS
                FROM [DataCo Sales]
                {whereClause}
            ";

            var data = await ExecuteMDXQuery(mdxQuery);
            return ParseDepartmentData(data);
        }

        // 9. Revenue & Profit by Category (Top 20)
        private async Task<List<CategoryChartData>> GetRevenueByCategory(DashboardFilters filters)
        {
            string mdxQuery = $@"
                SELECT 
                    {{[Measures].[Total Price], [Measures].[Profit]}} ON COLUMNS,
                    NON EMPTY TOPCOUNT(
                        [Dim Product].[Category Name].[Category Name].MEMBERS,
                        20,
                        [Measures].[Total Price]
                    ) ON ROWS
                FROM [DataCo Sales]
                {BuildWhereClause(filters)}
            ";

            var data = await ExecuteMDXQuery(mdxQuery);
            return ParseCategoryData(data);
        }

        // 10. Profit by State (Top 20)
        private async Task<List<StateChartData>> GetProfitByState(DashboardFilters filters)
        {
            string mdxQuery = $@"
                SELECT 
                    {{[Measures].[Total Price], [Measures].[Profit]}} ON COLUMNS,
                    NON EMPTY TOPCOUNT(
                        [Fact Sales].[Order State].[Order State].MEMBERS,
                        20,
                        [Measures].[Profit]
                    ) ON ROWS
                FROM [DataCo Sales]
                {BuildWhereClause(filters)}
            ";

            var data = await ExecuteMDXQuery(mdxQuery);
            return ParseStateData(data);
        }

        // Build WHERE clause from filters
        private string BuildWhereClause(
            DashboardFilters filters,
            bool excludeYear = false,
            bool excludeMarket = false,
            bool excludeDepartment = false)
        {
            var conditions = new List<string>();

            // 1. Filter Years
            if (!excludeYear && filters.Years != null && filters.Years.Any())
            {
                var members = string.Join(", ", filters.Years.Select(y => $"[Dim Date].[Year].&[{y}]"));
                conditions.Add($"{{{members}}}");
            }

            // 2. Filter Quarters
            if (filters.Quarters != null && filters.Quarters.Any())
            {
                var members = string.Join(", ", filters.Quarters.Select(q => $"[Dim Date].[Quarter].&[{q}]"));
                conditions.Add($"{{{members}}}");
            }

            // 3. Filter Markets
            if (!excludeMarket && filters.Markets != null && filters.Markets.Any())
            {
                var members = string.Join(", ", filters.Markets.Select(m => $"[Fact Sales].[Market].&[{m}]"));
                conditions.Add($"{{{members}}}");
            }

            // 4. Filter Departments
            if (!excludeDepartment && filters.Departments != null && filters.Departments.Any())
            {
                var members = string.Join(", ", filters.Departments.Select(d => $"[Dim Department].[Department Name].&[{d}]"));
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

                // Handle MEMBER_CAPTION format
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
                // Handle [Measures].[...] format
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

        private List<MarketChartData> ParseMarketData(DataTable table)
        {
            var result = new List<MarketChartData>();
            foreach (DataRow row in table.Rows)
            {
                result.Add(new MarketChartData
                {
                    Market = row["Market"]?.ToString() ?? "Unknown",
                    Revenue = row["Total Price"] != DBNull.Value ? Convert.ToDecimal(row["Total Price"]) : 0,
                    Profit = row["Profit"] != DBNull.Value ? Convert.ToDecimal(row["Profit"]) : 0
                });
            }
            return result;
        }

        private List<YearChartData> ParseYearData(DataTable table)
        {
            var result = new List<YearChartData>();
            foreach (DataRow row in table.Rows)
            {
                result.Add(new YearChartData
                {
                    Year = row["Year"]?.ToString() ?? "Unknown",
                    Revenue = row["Total Price"] != DBNull.Value ? Convert.ToDecimal(row["Total Price"]) : 0,
                    Profit = row["Profit"] != DBNull.Value ? Convert.ToDecimal(row["Profit"]) : 0
                });
            }
            return result;
        }

        private List<DepartmentChartData> ParseDepartmentData(DataTable table)
        {
            var result = new List<DepartmentChartData>();
            foreach (DataRow row in table.Rows)
            {
                result.Add(new DepartmentChartData
                {
                    Department = row["Department Name"]?.ToString() ?? "Unknown",
                    Revenue = row["Total Price"] != DBNull.Value ? Convert.ToDecimal(row["Total Price"]) : 0,
                    Profit = row["Profit"] != DBNull.Value ? Convert.ToDecimal(row["Profit"]) : 0
                });
            }
            return result;
        }

        private List<CategoryChartData> ParseCategoryData(DataTable table)
        {
            var result = new List<CategoryChartData>();
            foreach (DataRow row in table.Rows)
            {
                result.Add(new CategoryChartData
                {
                    Category = row["Category Name"]?.ToString() ?? "Unknown",
                    Revenue = row["Total Price"] != DBNull.Value ? Convert.ToDecimal(row["Total Price"]) : 0,
                    Profit = row["Profit"] != DBNull.Value ? Convert.ToDecimal(row["Profit"]) : 0
                });
            }
            return result;
        }

        private List<StateChartData> ParseStateData(DataTable table)
        {
            var result = new List<StateChartData>();
            foreach (DataRow row in table.Rows)
            {
                result.Add(new StateChartData
                {
                    State = row["Order State"]?.ToString() ?? "Unknown",
                    Revenue = row["Total Price"] != DBNull.Value ? Convert.ToDecimal(row["Total Price"]) : 0,
                    Profit = row["Profit"] != DBNull.Value ? Convert.ToDecimal(row["Profit"]) : 0
                });
            }
            return result;
        }
    }
    /*
    // Models
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

    public class ChartDataPoint
    {
        public string Label { get; set; } = "";
        public decimal Value { get; set; }
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
    }*/
}