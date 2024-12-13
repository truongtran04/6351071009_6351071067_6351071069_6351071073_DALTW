using ClothesStore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ClothesStore.Controllers
{
    public class OrdersController : Controller
    {
        private TESTEntities db = new TESTEntities();

        public enum PaymentMethods
        {
            MoMo,
            ATM,
            COD
        }

        public enum OrderStatus
        {
            Đang_chờ_xử_lý,  // Đang chờ xử lý
            Đã_xử_lý,  // Đã xử lý
            Đã_gửi,  // Đã gửi
            Đã_giao,  // Đã giao
            Đã_hủy  // Đã hủy
        }

        public string GetStatusBadge(OrderStatus status)
        {
            switch (status)
            {
                case OrderStatus.Đang_chờ_xử_lý:
                    return "bg-warning text-dark"; // Màu vàng cho trạng thái chờ
                case OrderStatus.Đã_xử_lý:
                    return "bg-success text-white"; // Màu xanh cho trạng thái đã hoàn thành
                case OrderStatus.Đã_gửi:
                    return "bg-success text-white";
                case OrderStatus.Đã_giao:
                    return "bg-success text-white"; // Màu đỏ cho trạng thái đã hủy
                default:
                    return "bg-secondary text-white"; // Màu xám cho các trạng thái khác
            }
        }



        // GET: Orders
        public ActionResult Index()
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            int userId = (int)Session["UserId"];

            // Lấy thông tin user
            var user = db.Users.FirstOrDefault(u => u.Id == userId);
            // Lấy profile của user
            var userProfile = db.Profiles.FirstOrDefault(p => p.UserId == userId);

            ViewBag.UserProfile = userProfile; // Lưu thông tin profile vào ViewBag

            ViewBag.Categories = db.Categories.ToList();
            ViewBag.Colors = db.Colors.ToList();

            // Lấy thông tin của cartDetail qua userID
            var cartDetails = db.Carts
                .Where(c => c.UserID == userId)
                .SelectMany(c => c.CartDetails)
                .ToList();
            ViewBag.CartDetails = cartDetails;

            // Tính tổng tiền cho giỏ hàng
            ViewBag.TotalAmount = cartDetails.Sum(cd => cd.TotalPrice);

            return View();
        }

        [HttpPost]
        public JsonResult ProcessPayment(string paymentMethod, string fullName, string phoneNumber, string address, decimal totalAmount, string status)
        {
            // Kiểm tra thông tin giao hàng
            if (string.IsNullOrEmpty(paymentMethod) || string.IsNullOrEmpty(fullName) || string.IsNullOrEmpty(phoneNumber) || string.IsNullOrEmpty(address))
            {
                return Json(new { success = false, message = "Vui lòng điền đầy đủ thông tin thanh toán và giao hàng." });
            }

            int userId = (int)Session["UserId"];

            // Lấy thông tin chi tiết giỏ hàng
            var cartDetails = db.Carts
                .Where(c => c.UserID == userId)
                .SelectMany(c => c.CartDetails)
                .ToList();

            if (!cartDetails.Any())
            {
                return Json(new { success = false, message = "Giỏ hàng của bạn trống." });
            }

            // Kiểm tra số lượng trong kho
            foreach (var item in cartDetails)
            {
                var product = db.Clothes_Color_Size
                    .FirstOrDefault(p => p.ClothesID == item.ClothesID && p.ColorID == item.ColorID && p.Size.SizeName == item.SizeName);

                if (product == null)
                {
                    return Json(new { success = false, message = $"Sản phẩm {item.ClothesName} không tồn tại." });
                }

                if (product.Quantity == 0 || product.Quantity < item.Quantity)
                {

                    return Json(new { success = false, message = $"Sản phẩm {item.ClothesName} không đủ số lượng." });
                }
            }

            // Tạo một đơn hàng mới
            var newOrder = new Order
            {
                UserID = userId,
                TotalAmount = totalAmount,
                CreatedAt = DateTime.Now,
                Status = OrderStatus.Đang_chờ_xử_lý.ToString(),
            };

            db.Orders.Add(newOrder);
            db.SaveChanges(); // Lúc này OrderID sẽ được tự động gán

            // Cập nhật số lượng sản phẩm trong kho và thêm chi tiết đơn hàng
            foreach (var item in cartDetails)
            {
                var product = db.Clothes_Color_Size
                    .FirstOrDefault(p => p.ClothesID == item.ClothesID && p.ColorID == item.ColorID && p.Size.SizeName == item.SizeName);

                if (product != null)
                {
                    product.Quantity -= item.Quantity; // Trừ số lượng
                }

                var orderDetail = new OrderDetail
                {
                    OrderID = newOrder.OrderID, // FK đến Order
                    ClothesID = item.ClothesID, // FK đến sản phẩm
                    ClothesName = item.ClothesName,
                    MainImage = item.MainImage,
                    ColorID = item.ColorID, // FK đến màu sắc
                    SizeName = item.SizeName, // Kích cỡ
                    Quantity = item.Quantity, // Số lượng
                    Price = item.Price, // Giá sản phẩm
                };

                db.OrderDetails.Add(orderDetail);
            }
            db.SaveChanges(); // Lưu cập nhật số lượng và chi tiết đơn hàng

            // Xóa sản phẩm khỏi giỏ hàng
            var cart = db.Carts.FirstOrDefault(c => c.UserID == userId);
            if (cart != null)
            {
                db.CartDetails.RemoveRange(cart.CartDetails);
                db.SaveChanges();
            }

            // Trả về JSON với OrderID để client có thể sử dụng để điều hướng
            return Json(new { success = true, orderId = newOrder.OrderID, totalAmount = totalAmount });
        }



        [HttpGet]
        public ActionResult OrderDetail(int orderId)
        {
            int userId = (int)Session["UserId"];

            // Truy vấn thông tin đơn hàng từ bảng Order
            var order = db.Orders.FirstOrDefault(o => o.OrderID == orderId && o.UserID == userId);
            if (order != null && order.Status == "Đang_chờ_xử_lý")
            {
                ViewBag.IsPaymentSuccess = true;
            }
            else
            {
                ViewBag.IsPaymentSuccess = false;
            }

            // Truy vấn thông tin người dùng từ bảng Profile
            var userProfile = db.Profiles.FirstOrDefault(p => p.UserId == userId);

            // Lấy chi tiết đơn hàng
            var orderDetails = db.OrderDetails
            .Where(od => od.OrderID == orderId)
            .ToList();

            // Tạo ViewModel kết hợp thông tin từ Order và Profile
            var orderDetailViewModel = new OrderDetailViewModel
            {
                OrderId = orderId,
                OrderDate = order.CreatedAt ?? DateTime.Now,
                OrderStatus = order.Status,
                TotalAmount = order.TotalAmount,
                FullName = userProfile.FullName,
                PhoneNumber = userProfile.PhoneNumber,
                Address = userProfile.Address,
                OrderDetails = orderDetails,
            };

            string badgeClass = GetStatusBadge((OrderStatus)Enum.Parse(typeof(OrderStatus), order.Status));
            ViewBag.OrderBadgeClass = badgeClass;
            return View(orderDetailViewModel);
        }



        [HttpPost]
        public ActionResult UpdateOrderStatus(int orderId, OrderStatus newStatus)
        {
            var order = db.Orders.Find(orderId);
            if (order != null)
            {
                order.Status = newStatus.ToString(); // Cập nhật trạng thái
                db.SaveChanges();
                return Json(new { success = true });
            }
            return Json(new { success = false, message = "Không tìm thấy đơn hàng." });
        }
        [HttpPost]
        public ActionResult ProcessOrder(int orderId)
        {
            // Lấy thông tin đơn hàng
            var order = db.Orders.Include("OrderDetails").SingleOrDefault(o => o.OrderID == orderId);

            if (order == null)
            {
                return Json(new { success = false, message = "Đơn hàng không tồn tại." });
            }

            foreach (var detail in order.OrderDetails)
            {
                // Tìm sản phẩm trong bảng Clothes_Color_Size
                var product = db.Clothes_Color_Size.SingleOrDefault(c =>
                    c.ClothesID == detail.ClothesID &&
                    c.ColorID == detail.ColorID);

                if (product != null)
                {
                    if (product.Quantity >= detail.Quantity)
                    {
                        // Giảm số lượng sản phẩm
                        product.Quantity = (int)(product.Quantity - detail.Quantity);
                    }
                    else
                    {
                        return Json(new { success = false, message = $"Sản phẩm không đủ số lượng trong kho." });
                    }
                }
                else
                {
                    return Json(new { success = false, message = $"Sản phẩm với ID {detail.ClothesID} không tồn tại trong kho." });
                }
            }

            // Cập nhật trạng thái đơn hàng
            order.Status = "Completed"; // Đánh dấu đơn hàng là hoàn thành
            db.SaveChanges(); // Lưu thay đổi vào cơ sở dữ liệu

            return Json(new { success = true, message = "Thanh toán thành công, kho đã được cập nhật." });
        }

    }
}