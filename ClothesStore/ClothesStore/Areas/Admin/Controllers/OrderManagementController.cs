using ClothesStore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using static ClothesStore.Controllers.OrdersController;

namespace ClothesStore.Areas.Admin.Controllers
{
    public class OrderManagementController : Controller
    {
        private TESTEntities db = new TESTEntities();
        // GET: Admin/OrderManagement
        public ActionResult Index()
        {
            var orders = db.Orders.Include("User");
            ViewBag.OrderStatuses = Enum.GetValues(typeof(ClothesStore.Models.OrderStatus)).Cast<ClothesStore.Models.OrderStatus>();
            return View(orders.OrderByDescending(or => or.OrderID).ToList());

        }

        [HttpPost]
        public ActionResult UpdateStatus(int orderId, string status)
        {
            var order = db.Orders.Find(orderId);
            if (order != null)
            {
                if (Enum.TryParse(status, out ClothesStore.Models.OrderStatus orderStatus))
                {
                    order.Status = orderStatus.ToString();
                    db.SaveChanges();
                }
            }
            return RedirectToAction("Index", "OrderManagement", new { area = "Admin" });
        }

        public ActionResult OrderDetails(int id)
        {

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Order order = db.Orders.Find(id);
            if (order == null)
            {
                return HttpNotFound();
            }

            var user = db.Profiles.FirstOrDefault(p => p.UserId == order.UserID);

            // Lấy chi tiết đơn hàng
            var orderDetails = db.OrderDetails.Where(od => od.OrderID == order.OrderID).ToList();

            var orderDetailViewModel = new OrderDetailViewModel
            {
                OrderId = order.UserID,
                OrderDate = order.CreatedAt ?? DateTime.Now,
                OrderStatus = order.Status,
                TotalAmount = order.TotalAmount,
                FullName = user.FullName,
                PhoneNumber = user.PhoneNumber,
                Address = user.Address,
                OrderDetails = orderDetails,
            };

            return View(orderDetailViewModel);
        }
    }
}