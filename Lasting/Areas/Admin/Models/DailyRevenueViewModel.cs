namespace Lasting.Areas.Admin.Models
{
    public class DailyRevenueViewModel
    {
        public DateTime Date { get; set; }
        public decimal TotalRevenue { get; set; }
        public int OrderCount { get; set; }
    }
}
