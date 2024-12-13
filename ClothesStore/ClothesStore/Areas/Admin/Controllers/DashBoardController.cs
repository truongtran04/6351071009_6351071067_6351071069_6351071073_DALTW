using ClothesStore.Models;
using Newtonsoft.Json;
using System;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ClothesStore.Areas.Admin.Controllers
{
    public class DashBoardController : Controller
    {
        private TESTEntities db = new TESTEntities();

        private int TotalClothes()
        {
            return db.Clothes.Where(clo => clo.IsDeleted == false).Count();
        }

        private decimal TotalAmountOnDay(DateTime date)
        {
            return db.Orders
                .Where(o => o.CreatedAt.HasValue && DbFunctions.TruncateTime(o.CreatedAt) == date.Date)
                .Sum(o => (decimal?)o.TotalAmount) ?? 0;
        }
        private decimal AverageAmountInMonth(DateTime date)
        {
            var averageAmount = db.Orders
                .Where(o => o.CreatedAt.HasValue
                            && o.CreatedAt.Value.Year == date.Year
                            && o.CreatedAt.Value.Month == date.Month)
                .Average(o => (decimal?)o.TotalAmount) ?? 0;

            return averageAmount;
        }

        private int TotalOrderOnDay(DateTime date)
        {
            return db.Orders
                .Where(o => o.CreatedAt.HasValue && DbFunctions.TruncateTime(o.CreatedAt) == date.Date)
                .Count();
        }
        private int MaxOrderInDay(DateTime date)
        {
            var dailyOrderCounts = db.Orders
                .Where(o => o.CreatedAt.HasValue
                            && o.CreatedAt.Value.Year == date.Year
                            && o.CreatedAt.Value.Month == date.Month)
                .GroupBy(o => DbFunctions.TruncateTime(o.CreatedAt.Value))
                .Select(g => g.Count())
                .ToList();

            var maxAmount = dailyOrderCounts.Any()
                ? dailyOrderCounts.Max()
                : 0;

            return maxAmount;
        }


        private int CountCustomer()
        {
            return db.UserRoles.Where(us => us.Role == "client").Count();
        }

        private string CountCate()
        {
            var clothes = db.Clothes.Where(clo => clo.IsDeleted == false)
                .GroupBy(cate => cate.Category.CategoryName)
                .Select(group => new
                {
                    Category = group.Key,
                    Count = group.Count(),
                })
                .ToList();

            // Đảm bảo thứ tự: Nam, Nữ, Bé trai, Bé gái
            var orderedClothes = clothes.OrderBy(cate =>
                cate.Category == "Nam" ? 0 :
                cate.Category == "Nữ" ? 1 :
                cate.Category == "Bé trai" ? 2 : 3)
                .ToList();

            var chartData = new
            {
                labels = orderedClothes.Select(cate => cate.Category).ToArray(),
                series = orderedClothes.Select(cate => cate.Count).ToArray(),
            };

            return JsonConvert.SerializeObject(chartData);
        }



        private string CountClothingType()
        {
            var clothes = db.Clothes.Where(clo => clo.IsDeleted == false)
                .GroupBy(cate => new { cate.ClothingType.ClothingTypeName, cate.Category.CategoryName })
                .Select(group => new
                {
                    ClothingType = group.Key.ClothingTypeName,
                    Category = group.Key.CategoryName,
                    Count = group.Count(),
                })
                .ToList();

            var orderedClothes = clothes.OrderBy(cate =>
                cate.Category == "Nam" ? 0 :
                cate.Category == "Nữ" ? 1 :
                cate.Category == "Bé trai" ? 2 : 3)
                .ToList();

            var chartData = new
            {
                labels = orderedClothes.Select(cate => cate.ClothingType).Distinct().ToArray(),
                series = new[]
                {
                    orderedClothes.Where(cate => cate.Category == "Nam").Select(cate => cate.Count).ToArray(),
                    orderedClothes.Where(cate => cate.Category == "Nữ").Select(cate => cate.Count).ToArray(),
                    orderedClothes.Where(cate => cate.Category == "Bé trai").Select(cate => cate.Count).ToArray(),
                    orderedClothes.Where(cate => cate.Category == "Bé gái").Select(cate => cate.Count).ToArray()
                },
            };

            return JsonConvert.SerializeObject(chartData);
        }


        // GET: Admin/DashBoard
        public ActionResult Index()
        {
            DateTime date = DateTime.Now.Date;
            // tính tiền trong ngày
            ViewBag.TotalDay = TotalAmountOnDay(date);
            ViewBag.TotalMonth = AverageAmountInMonth(date);
            // tính hóa đơn trong 1 ngày
            ViewBag.OrderDay = TotalOrderOnDay(date);
            ViewBag.OrderMaxDay = MaxOrderInDay(date);

            ViewBag.Clothes = TotalClothes();
            ViewBag.Customer = CountCustomer();
            ViewBag.CountCate = CountCate();
            ViewBag.CountClothingType = CountClothingType();

            ViewBag.NewCustommer = db.Users
            .Where(cus => db.UserRoles
                .Where(role => role.UserId == cus.Id)
                .All(role => role.Role == "client")
                && cus.IsActive == true)
            .OrderByDescending(cus => cus.CreatedAt)
            .ToList();


            ViewBag.NewOrder = db.Orders.OrderByDescending(or => or.CreatedAt).ToList();

            return View();
        }
        public ActionResult Logout()
        {
            // Xóa thông tin đăng nhập của người dùng
            Session.Clear(); // Hoặc Session.Remove("UserId");

            // Chuyển hướng về trang Home/Index sau khi đăng xuất
            return RedirectToAction("Index", "Home");
        }
    }
}
