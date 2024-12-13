using ClothesStore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ClothesStore.Controllers
{
    public class CartController : Controller
    {
        // GET: Cart
        private readonly TESTEntities db = new TESTEntities();

        private void UpdateTotalAmount(int cartId)
        {
            var cart = db.Carts.Find(cartId);
            if (cart != null)
            {
                cart.TotalAmount = db.CartDetails
                    .Where(cd => cd.CartID == cartId)
                    .Sum(cd => (decimal?)(cd.Quantity * cd.Price)) ?? 0;
                db.SaveChanges();
            }
        }
        private void UpdateSessionCartDetails(int cartId)
        {
            var cartDetails = db.CartDetails.Where(cd => cd.CartID == cartId).ToList();
            Session["CartDetails"] = cartDetails;
        }
        [HttpPost]

        public ActionResult AddToCart(string clothesId, string clothesName, string color, string size, decimal price, string mainImage)
        {
            // Kiểm tra người dùng đã đăng nhập chưa
            if (Session["UserId"] == null)
            {
                Session["ReturnUrl"] = Request.Url.ToString(); // Lưu URL hiện tại
                return Json(new { success = false, message = "Bạn cần đăng nhập để thực hiện thao tác này." });
                //return RedirectToAction("Login", "Account");
            }

            // Lấy userId từ Session
            int userId = (int)Session["UserId"];

            // Tìm giỏ hàng chưa hoàn tất của người dùng
            var cart = db.Carts.SingleOrDefault(c => c.UserID == userId && !(c.IsCompleted ?? false));

            if (cart == null)
            {
                // Nếu không tìm thấy giỏ hàng, tạo mới
                cart = new Cart { UserID = userId, TotalAmount = 0 }; // Khởi tạo TotalAmount là 0 khi tạo giỏ mới
                db.Carts.Add(cart);
                db.SaveChanges();
            }

            // Kiểm tra sản phẩm đã tồn tại trong CartDetail chưa
            var cartItem = db.CartDetails.SingleOrDefault(item =>
                item.CartID == cart.CartID && item.ClothesID == clothesId && item.ColorID == color && item.SizeName == size);

            if (cartItem == null)
            {
                // Thêm sản phẩm mới vào CartDetail
                cartItem = new CartDetail
                {
                    CartID = cart.CartID,
                    ClothesID = clothesId,
                    ClothesName = clothesName,
                    MainImage = mainImage,
                    SizeName = size,
                    ColorID = color,
                    Quantity = 1,
                    Price = price
                };
                db.CartDetails.Add(cartItem);
            }
            else
            {
                // Cập nhật số lượng nếu sản phẩm đã có trong giỏ
                cartItem.Quantity += 1;
            }

            db.SaveChanges();


            // Cập nhật lại TotalAmount của giỏ hàng
            UpdateTotalAmount(cart.CartID);
            UpdateSessionCartDetails(cart.CartID);
            return Json(new { success = true, message = $"{clothesName} đã được thêm vào giỏ hàng." });
        }
        // Xóa sản phẩm khỏi giỏ hàng
        [HttpPost]
        public ActionResult RemoveFromCart(string uniqueId)
        {
            var parts = uniqueId.Split('_');
            var clothesId = parts[0];
            var colorId = parts[1];
            var sizeName = parts[2];
            if (Session["UserId"] == null)
            {
                Session["ReturnUrl"] = Request.Url.ToString(); // Lưu URL hiện tại
                return Json(new { success = false, message = "Bạn cần đăng nhập để thực hiện thao tác này." });
                //return RedirectToAction("Login", "Account");
            }


            int userId = (int)Session["UserId"];
            var cart = db.Carts.SingleOrDefault(c => c.UserID == userId && !(c.IsCompleted ?? false));

            if (cart != null)
            {
                var itemToRemove = db.CartDetails.SingleOrDefault(item =>
                    item.CartID == cart.CartID &&
                    item.ClothesID == clothesId &&
                    item.ColorID == colorId &&
                    item.SizeName == sizeName);

                if (itemToRemove != null)
                {
                    db.CartDetails.Remove(itemToRemove);
                    db.SaveChanges();

                    UpdateTotalAmount(cart.CartID);
                    UpdateSessionCartDetails(cart.CartID);

                    return Json(new { success = true, message = "Sản phẩm đã được xóa thành công." });
                }
                return Json(new { success = false, message = "Sản phẩm không tìm thấy trong giỏ hàng." });
            }
            return Json(new { success = false, message = "Giỏ hàng không tồn tại." });
        }

        // Cập nhật số lượng sản phẩm trong giỏ hàng
        [HttpPost]
        public JsonResult UpdateCart(string clothesID, string colorID, string sizeName, int quantity)
        {
            if (quantity < 0)
                return Json(new { success = false, message = "Số lượng không được âm." });

            if (Session["UserId"] == null)
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập trước khi cập nhật giỏ hàng." });
            }

            int userId = (int)Session["UserId"];
            var cart = db.Carts.SingleOrDefault(c => c.UserID == userId && !(c.IsCompleted ?? false));

            if (cart != null)
            {
                var cartItem = db.CartDetails.FirstOrDefault(cd => cd.CartID == cart.CartID && cd.ClothesID == clothesID && cd.ColorID == colorID && cd.SizeName == sizeName);
                if (cartItem != null)
                {
                    cartItem.Quantity = quantity; // Cập nhật số lượng
                    cartItem.TotalPrice = cartItem.Price * quantity; // Cập nhật tổng giá
                    db.SaveChanges(); // Lưu thay đổi vào cơ sở dữ liệu
                    UpdateTotalAmount(cart.CartID); // Cập nhật tổng số tiền giỏ hàng
                    UpdateSessionCartDetails(cart.CartID);
                    return Json(new { success = true });
                }
            }
            return Json(new { success = false, message = "Sản phẩm không tìm thấy." });
        }
        public PartialViewResult GetCartPartial()
        {
            int userId = (int)Session["UserId"];
            var cart = db.Carts.FirstOrDefault(c => c.UserID == userId && !(c.IsCompleted ?? false));

            // Lấy danh sách sản phẩm trong giỏ hàng
            var cartDetails = cart != null
                ? db.CartDetails.Where(cd => cd.CartID == cart.CartID).ToList()
                : new List<CartDetail>();
            //var cartDetails = Session["CartDetails"] as List<CartDetail> ?? new List<CartDetail>();
            return PartialView("_CartPartial", cartDetails);
        }
    }
}