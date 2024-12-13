using ClothesStore.Models;
using ClothesStore.Models.ViewModels;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ClothesStore.Controllers
{
    public class HomeController : Controller
    {
        private TESTEntities db = new TESTEntities();
        // GET: Home

        public ActionResult Index()
        {
            var userId = Session["UserId"] != null ? Convert.ToInt32(Session["UserId"]) : 0;

            // Lọc CartDetails theo UserID
            var cartDetails = db.Carts
                .Where(c => c.UserID == userId) // Lọc theo UserID
                .SelectMany(c => c.CartDetails) // Lấy CartDetails của các Cart
                .ToList();

            ViewBag.CartDetails = cartDetails; // Gán kết quả vào ViewBag

            var clothesViewModel = db.Clothes.Where(clo => clo.IsDeleted == false)
              .Select(c => new ClothesViewModel
              {
                  ClothesItem = c,
                  Images = db.Images.Where(img => img.ClothesID == c.ClothesID).ToList(),
                  Clothes_Color_Sizes = db.Clothes_Color_Size.Where(clo => clo.ClothesID == c.ClothesID).ToList(),

                  Colors = db.Colors
                      .Where(col => db.Images.Any(img => img.ColorID == col.ColorID && img.ClothesID == c.ClothesID))
                      .ToList(),

                  // Get sizes associated with each color for the clothing item
                  Sizes = db.Sizes
                      .Where(size => db.Clothes_Color_Size
                          .Any(clo => clo.SizeID == size.SizeID && clo.ClothesID == c.ClothesID
                                       && clo.ColorID == db.Colors
                                          .Where(col => db.Images.Any(img => img.ColorID == col.ColorID && img.ClothesID == c.ClothesID))
                                          .Select(col => col.ColorID).FirstOrDefault()))
                      .ToList()
              })
              .ToList();
            

            // Tạo danh sách kết quả cuối cùng
            var finalClothes = new List<ClothesViewModel>();

            // Khởi tạo đối tượng Random để sinh số ngẫu nhiên
            var random = new Random();

            // Tổng số sản phẩm cần xử lý
            int totalProducts = 8;

            // Số lượng ngẫu nhiên cho từng loại sản phẩm (Nam, Nữ, Bé gái, Bé trai)
            int maleCount = random.Next(2, Math.Min(4, totalProducts));
            int femaleCount = random.Next(2, Math.Min(4, totalProducts - maleCount));
            int girlCount = random.Next(1, Math.Min(3, totalProducts - maleCount - femaleCount));
            int boyCount = totalProducts - maleCount - femaleCount - girlCount;
            // Group by ClothingTypeID
            var groupedClothes = clothesViewModel.GroupBy(clo => clo.ClothesItem.ClothingTypeID).ToList();

            foreach (var group in groupedClothes)
            {
                // Lấy sản phẩm nam và nữ trong từng nhóm
                var male = group.Where(clo => clo.ClothesItem.Category.CategoryName == "Nam").Take(maleCount).ToList();
                var female = group.Where(clo => clo.ClothesItem.Category.CategoryName == "Nữ").Take(femaleCount).ToList();
                var girl = group.Where(clo => clo.ClothesItem.Category.CategoryName == "Bé gái").Take(girlCount).ToList();
                var boy = group.Where(clo => clo.ClothesItem.Category.CategoryName == "Bé trai").Take(boyCount).ToList();

                // Nếu không đủ nam hoặc nữ, lấy bù từ danh sách còn lại
                int maleShortage = maleCount - male.Count;
                int femaleShortage = femaleCount - female.Count;
                int girlShortage = girlCount - girl.Count;
                int boyShortage = boyCount - boy.Count;

                if (maleShortage > 0)
                {
                    var extraMale = group.Where(clo => clo.ClothesItem.Category.CategoryName == "Nữ")
                                         .Skip(femaleCount)
                                         .Take(maleShortage)
                                         .ToList();
                    male.AddRange(extraMale);
                }

                if (femaleShortage > 0)
                {
                    var extraFemale = group.Where(clo => clo.ClothesItem.Category.CategoryName == "Nam")
                                           .Skip(maleCount)
                                           .Take(femaleShortage)
                                           .ToList();
                    female.AddRange(extraFemale);
                }

                // Gộp nam và nữ vào danh sách cuối cùng
                finalClothes.AddRange(male.Concat(female).Concat(girl).Concat(boy));
            }

            return View(finalClothes.Where(clo => clo.ClothesItem.IsDeleted == false).ToList());
        }

        public ActionResult _Category()
        {
            var cate = db.Categories.Where(cat => cat.IsHidden == false).ToList();
            return View(cate);
        }
        public ActionResult _ProductType()
        {
            var productTypes = db.ClothingTypes.ToList(); // Thay đổi db thành DbContext của bạn và sửa query để lấy loại sản phẩm
            return PartialView(productTypes);
        }

        //public ActionResult Search(string searchTerm, int? page, string sort)
        //{
        //    int pageSize = 12;
        //    int pageNumber = (page ?? 1);

        //    if (string.IsNullOrEmpty(searchTerm))
        //    {
        //        return RedirectToAction("Index");
        //    }

        //    // Lọc sản phẩm
        //    var query = db.Clothes.AsQueryable();
        //    query = query.Where(c =>
        //        c.ClothesName.Contains(searchTerm) ||
        //        db.Categories.Any(cat => cat.CategoryID == c.CategoryID && cat.CategoryName.Contains(searchTerm))
        //    );

        //    // Sắp xếp sản phẩm
        //    switch (sort)
        //    {
        //        case "newest":
        //            query = query.OrderByDescending(c => c.CreatedAt);
        //            break;
        //        case "price_asc":
        //            query = query.OrderBy(c => c.Price);
        //            break;
        //        case "price_desc":
        //            query = query.OrderByDescending(c => c.Price);
        //            break;
        //        default:
        //            break;
        //    }

        //    // Chuyển đổi sang ViewModel
        //    var searchResults = query.Where(clo => clo.IsDeleted == false)
        //        .Select(c => new ClothesViewModel
        //        {
        //            ClothesItem = c,
        //            Images = db.Images.Where(img => img.ClothesID == c.ClothesID).ToList(),
        //            Clothes_Color_Sizes = db.Clothes_Color_Size.Where(clo => clo.ClothesID == c.ClothesID).ToList(),
        //            Colors = db.Colors
        //                .Where(col => db.Images.Any(img => img.ColorID == col.ColorID && img.ClothesID == c.ClothesID))
        //                .ToList(),
        //            Sizes = db.Sizes
        //                .Where(size => db.Clothes_Color_Size
        //                    .Any(clo => clo.SizeID == size.SizeID && clo.ClothesID == c.ClothesID
        //                                 && clo.ColorID == db.Colors
        //                                    .Where(col => db.Images.Any(img => img.ColorID == col.ColorID && img.ClothesID == c.ClothesID))
        //                                    .Select(col => col.ColorID).FirstOrDefault()))
        //                .ToList()
        //        })
        //        .ToList();

        //    var pagedResults = searchResults.ToPagedList(pageNumber, pageSize);

        //    if (Request.IsAjaxRequest())
        //    {
        //        return PartialView("_ClothesList", pagedResults);
        //    }
        //    ViewBag.SearchTerm = searchTerm;
        //    return View("Search", pagedResults);
        //}

        public ActionResult Search(string searchTerm, int? page, string sort)
        {
            int pageSize = 12;
            int pageNumber = (page ?? 1);

            if (string.IsNullOrEmpty(searchTerm))
            {
                return RedirectToAction("Index");
            }

            // Lọc sản phẩm
            var searchTermLower = searchTerm.ToLower(); // Chuyển đổi từ khóa thành chữ thường
            var query = db.Clothes.AsQueryable().Where(c =>
                c.ClothesName.ToLower().Contains(searchTermLower) ||
                db.Categories.Any(cat => cat.CategoryName.ToLower().Contains(searchTermLower))
            );

            // Sắp xếp sản phẩm
            switch (sort)
            {
                case "newest":
                    query = query.OrderByDescending(c => c.CreatedAt);
                    break;
                case "price_asc":
                    query = query.OrderBy(c => c.Price);
                    break;
                case "price_desc":
                    query = query.OrderByDescending(c => c.Price);
                    break;
                default:
                    break;
            }

            // Chuyển đổi sang ViewModel
            var searchResults = query.Where(clo => clo.IsDeleted == false)
                .Select(c => new ClothesViewModel
                {
                    ClothesItem = c,
                    Images = db.Images.Where(img => img.ClothesID == c.ClothesID).ToList(),
                    Clothes_Color_Sizes = db.Clothes_Color_Size.Where(clo => clo.ClothesID == c.ClothesID).ToList(),
                    Colors = db.Colors
                        .Where(col => db.Images.Any(img => img.ColorID == col.ColorID && img.ClothesID == c.ClothesID))
                        .ToList(),
                    Sizes = db.Sizes
                        .Where(size => db.Clothes_Color_Size
                            .Any(clo => clo.SizeID == size.SizeID && clo.ClothesID == c.ClothesID
                                         && clo.ColorID == db.Colors
                                            .Where(col => db.Images.Any(img => img.ColorID == col.ColorID && img.ClothesID == c.ClothesID))
                                            .Select(col => col.ColorID).FirstOrDefault()))
                        .ToList()
                })
                .ToList();

            var pagedResults = searchResults.ToPagedList(pageNumber, pageSize);

            if (Request.IsAjaxRequest())
            {
                return PartialView("_ClothesList", pagedResults);
            }
            ViewBag.SearchTerm = searchTerm;
            return View("Search", pagedResults);
        }



    }
}