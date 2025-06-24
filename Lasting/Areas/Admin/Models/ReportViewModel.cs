using Lasting.Models;

namespace Lasting.Areas.Admin.Models
{
        public class ReportViewModel
        {
            public List<Order> Orders { get; set; } = new();
            public decimal TotalRevenue { get; set; }
            public int TotalCount { get; set; }

            public string? FromDate { get; set; }
            public string? ToDate { get; set; }
            public string? Status { get; set; }
            public List<DailyRevenueViewModel>? DailyStats { get; set; } 

    }
}
