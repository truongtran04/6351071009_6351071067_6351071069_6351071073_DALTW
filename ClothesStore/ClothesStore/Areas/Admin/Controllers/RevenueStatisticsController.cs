using ClothesStore.Models;
using System;
using System.Linq;
using System.Web.Mvc;

namespace ClothesStore.Areas.Admin.Controllers
{
    public class RevenueStatisticsController : Controller
    {
        private TESTEntities db = new TESTEntities();

        public ActionResult Index(DateTime? startDate = null, DateTime? endDate = null)
        {
            // Thiết lập giá trị mặc định cho ngày bắt đầu và ngày kết thúc
            startDate = startDate ?? DateTime.Now.AddMonths(-1); // Mặc định 1 tháng trước
            endDate = endDate ?? DateTime.Now;  // Mặc định đến ngày hiện tại

            // Lọc các đơn hàng đã giao trong khoảng thời gian
            var orders = db.Orders
                .Where(o => o.Status == "Đã_giao" && o.CreatedAt >= startDate && o.CreatedAt <= endDate)
                .ToList();

            // Tính toán doanh thu theo ngày
            var revenueByDay = orders
                .GroupBy(o => o.CreatedAt) // Sử dụng CreatedAt.Date để nhóm theo ngày mà không cần quan tâm đến giờ
                .Select(g => new
                {
                    Date = g.Key,
                    TotalRevenue = g.Sum(o => o.TotalAmount)
                })
                .OrderBy(g => g.Date)
                .ToList();

            // Tính tổng doanh thu trong khoảng thời gian
            decimal totalRevenue = revenueByDay.Sum(r => r.TotalRevenue);

            // Gửi dữ liệu doanh thu, các ngày đã lọc và tổng doanh thu qua ViewBag
            ViewBag.RevenueData = revenueByDay;
            ViewBag.StartDate = startDate;
            ViewBag.EndDate = endDate;
            ViewBag.TotalRevenue = totalRevenue;

            return View(revenueByDay);  // Trả về view với dữ liệu doanh thu

        }
    }
}

