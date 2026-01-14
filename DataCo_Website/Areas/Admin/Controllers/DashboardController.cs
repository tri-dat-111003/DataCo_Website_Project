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

        // ============================================================================
        // GET: Admin/Dashboard - Main dashboard view
        // ============================================================================
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

                // Tính Profit và Revenue trước để tính Profit Margin
                var totalProfit = await GetTotalProfit(filters);
                var totalRevenue = await GetTotalRevenue(filters);

                // Load dashboard data
                var dashboardData = new DashboardViewModel
                {
                    TotalProfit = totalProfit,
                    TotalRevenue = totalRevenue,

                    // Tính Profit Margin %
                    ProfitMargin = totalRevenue > 0 ? Math.Round((totalProfit / totalRevenue) * 100, 2) : 0,

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

        // ============================================================================
        // API endpoint cho Cross-Filtering
        // ============================================================================
        [HttpPost]
        public async Task<IActionResult> GetFilteredData([FromBody] CrossFilterRequest request)
        {
            try
            {
                _logger.LogInformation($"GetFilteredData called for dataType: {request.DataType}");

                // Build filters từ FormFilters trong request thay vì empty DashboardFilters
                var filters = new DashboardFilters
                {
                    Years = request.FormFilters?.Years,
                    Quarters = request.FormFilters?.Quarters,
                    Markets = request.FormFilters?.Markets,
                    Departments = request.FormFilters?.Departments
                };

                var crossFilters = BuildCrossFilters(request.Filters);

                // Route to appropriate data method
                object result = request.DataType switch
                {
                    "segment" => await GetProfitBySegment(filters, crossFilters),
                    "status" => await GetOrderCountByStatus(filters, crossFilters),
                    "transactionType" => await GetTransactionCountByType(filters, crossFilters),
                    "market" => await GetRevenueByMarket(filters, crossFilters),
                    "year" => await GetRevenueByYear(filters, crossFilters),
                    "department" => await GetRevenueByDepartment(filters, crossFilters),
                    "category" => await GetRevenueByCategory(filters, crossFilters),
                    "state" => await GetProfitByState(filters, crossFilters),
                    "kpis" => await GetKPIsWithCrossFilters(filters, crossFilters),
                    _ => throw new ArgumentException($"Invalid data type: {request.DataType}")
                };

                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in GetFilteredData for {request.DataType}");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // Get KPIs với cross-filters
        private async Task<KpiResponse> GetKPIsWithCrossFilters(DashboardFilters filters, Dictionary<string, string?> crossFilters)
        {
            var profit = await GetTotalProfit(filters, crossFilters);
            var revenue = await GetTotalRevenue(filters, crossFilters);

            return new KpiResponse
            {
                TotalProfit = profit,
                TotalRevenue = revenue,
                ProfitMargin = revenue > 0 ? Math.Round((profit / revenue) * 100, 2) : 0
            };
        }

        // Build cross-filter dictionary
        private Dictionary<string, string?> BuildCrossFilters(Dictionary<string, string?> filters)
        {
            var crossFilters = new Dictionary<string, string?>();

            foreach (var filter in filters)
            {
                if (!string.IsNullOrEmpty(filter.Value))
                {
                    crossFilters[filter.Key] = filter.Value;
                }
            }

            return crossFilters;
        }

        // ============================================================================
        // Get Available Options for Filters
        // ============================================================================

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

        // ============================================================================
        // Data Retrieval Methods với Cross-Filter Support
        // ============================================================================

        // 1. Total Profit
        private async Task<decimal> GetTotalProfit(DashboardFilters filters, Dictionary<string, string?>? crossFilters = null)
        {
            string mdxQuery = $@"
                SELECT 
                    [Measures].[Profit] ON COLUMNS 
                FROM [DataCo Sales]
                {BuildWhereClause(filters, crossFilters)}
            ";

            var table = await ExecuteMDXQuery(mdxQuery);
            if (table.Rows.Count > 0)
            {
                return table.Rows[0][0] != DBNull.Value ? Convert.ToDecimal(table.Rows[0][0]) : 0;
            }
            return 0;
        }

        // 2. Total Revenue
        private async Task<decimal> GetTotalRevenue(DashboardFilters filters, Dictionary<string, string?>? crossFilters = null)
        {
            string mdxQuery = $@"
                SELECT 
                    [Measures].[Total Price] ON COLUMNS 
                FROM [DataCo Sales]
                {BuildWhereClause(filters, crossFilters)}
            ";

            var table = await ExecuteMDXQuery(mdxQuery);
            if (table.Rows.Count > 0)
            {
                return table.Rows[0][0] != DBNull.Value ? Convert.ToDecimal(table.Rows[0][0]) : 0;
            }
            return 0;
        }

        // 3. Profit by Customer Segment
        private async Task<List<ChartDataPoint>> GetProfitBySegment(DashboardFilters filters, Dictionary<string, string?>? crossFilters = null)
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
                {BuildWhereClause(filters, crossFilters, excludeDimension: "segment")}
            ";

            var data = await ExecuteMDXQuery(mdxQuery);
            return ParseChartData(data, "Segment", "Profit");
        }

        // 4. Order Count by Status
        private async Task<List<ChartDataPoint>> GetOrderCountByStatus(DashboardFilters filters, Dictionary<string, string?>? crossFilters = null)
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
                {BuildWhereClause(filters, crossFilters, excludeDimension: "status")}
            ";

            var data = await ExecuteMDXQuery(mdxQuery);
            return ParseChartData(data, "Status", "Fact Sales Count");
        }

        // 5. Transaction Count by Type
        private async Task<List<ChartDataPoint>> GetTransactionCountByType(DashboardFilters filters, Dictionary<string, string?>? crossFilters = null)
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
                {BuildWhereClause(filters, crossFilters, excludeDimension: "transactionType")}
            ";

            var data = await ExecuteMDXQuery(mdxQuery);
            return ParseChartData(data, "Type Transaction", "Fact Sales Count");
        }

        // 6. Revenue & Profit by Market
        private async Task<List<MarketChartData>> GetRevenueByMarket(DashboardFilters filters, Dictionary<string, string?>? crossFilters = null)
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

            string whereClause = BuildWhereClause(filters, crossFilters, excludeMarket: true, excludeDimension: "market");

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
        private async Task<List<YearChartData>> GetRevenueByYear(DashboardFilters filters, Dictionary<string, string?>? crossFilters = null)
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

            string whereClause = BuildWhereClause(filters, crossFilters, excludeYear: true, excludeDimension: "year");

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
        private async Task<List<DepartmentChartData>> GetRevenueByDepartment(DashboardFilters filters, Dictionary<string, string?>? crossFilters = null)
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

            string whereClause = BuildWhereClause(filters, crossFilters, excludeDepartment: true, excludeDimension: "department");

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
        private async Task<List<CategoryChartData>> GetRevenueByCategory(DashboardFilters filters, Dictionary<string, string?>? crossFilters = null)
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
                {BuildWhereClause(filters, crossFilters, excludeDimension: "category")}
            ";

            var data = await ExecuteMDXQuery(mdxQuery);
            return ParseCategoryData(data);
        }

        // 10. Profit by State (Top 20)
        private async Task<List<StateChartData>> GetProfitByState(DashboardFilters filters, Dictionary<string, string?>? crossFilters = null)
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
                {BuildWhereClause(filters, crossFilters, excludeDimension: "state")}
            ";

            var data = await ExecuteMDXQuery(mdxQuery);
            return ParseStateData(data);
        }

        // ============================================================================
        // Build WHERE clause với Cross-Filter Support
        // ============================================================================
        private string BuildWhereClause(
            DashboardFilters filters,
            Dictionary<string, string?>? crossFilters = null,
            bool excludeYear = false,
            bool excludeMarket = false,
            bool excludeDepartment = false,
            string? excludeDimension = null)
        {
            var conditions = new List<string>();

            // 1. Filter Years (from form filters)
            if (!excludeYear && filters.Years != null && filters.Years.Any())
            {
                var members = string.Join(", ", filters.Years.Select(y => $"[Dim Date].[Year].&[{y}]"));
                conditions.Add($"{{{members}}}");
            }

            // 2. Filter Quarters (from form filters)
            if (filters.Quarters != null && filters.Quarters.Any())
            {
                var members = string.Join(", ", filters.Quarters.Select(q => $"[Dim Date].[Quarter].&[{q}]"));
                conditions.Add($"{{{members}}}");
            }

            // 3. Filter Markets (from form filters)
            if (!excludeMarket && filters.Markets != null && filters.Markets.Any())
            {
                var members = string.Join(", ", filters.Markets.Select(m => $"[Fact Sales].[Market].&[{m}]"));
                conditions.Add($"{{{members}}}");
            }

            // 4. Filter Departments (from form filters)
            if (!excludeDepartment && filters.Departments != null && filters.Departments.Any())
            {
                var members = string.Join(", ", filters.Departments.Select(d => $"[Dim Department].[Department Name].&[{d}]"));
                conditions.Add($"{{{members}}}");
            }

            // 5. Cross-Filters (from chart interactions)
            if (crossFilters != null && crossFilters.Any())
            {
                foreach (var filter in crossFilters)
                {
                    // Skip dimension đang được query (để chart vẫn hiển thị tất cả segments của nó)
                    if (filter.Key == excludeDimension) continue;

                    if (!string.IsNullOrEmpty(filter.Value))
                    {
                        var memberPath = filter.Key switch
                        {
                            "segment" => $"[Dim Customer].[Segment].&[{filter.Value}]",
                            "status" => $"[Fact Sales].[Status].&[{filter.Value}]",
                            "transactionType" => $"[Fact Sales].[Type Transaction].&[{filter.Value}]",
                            "market" => $"[Fact Sales].[Market].&[{filter.Value}]",
                            "year" => $"[Dim Date].[Year].&[{filter.Value}]",
                            "department" => $"[Dim Department].[Department Name].&[{filter.Value}]",
                            _ => null
                        };

                        if (memberPath != null)
                        {
                            conditions.Add($"{{{memberPath}}}");
                        }
                    }
                }
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

        // ============================================================================
        // Helper Methods
        // ============================================================================

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
}