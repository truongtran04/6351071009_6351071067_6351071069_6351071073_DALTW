using ClothesStore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ClothesStore.Controllers
{
    public class BaseTotalCartAmoutController : Controller
    {
        // GET: BaseTotalCartAmout
        private TESTEntities db = new TESTEntities();

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);

            if (Session["UserId"] != null)
            {
                var userId = Convert.ToInt32(Session["UserId"]);
                ViewBag.TotalAmount = CalculateTotalCartAmount(userId);
            }
        }

        private decimal CalculateTotalCartAmount(int userId)
        {
            // Lấy tổng giá trị từ các mục trong giỏ hàng của user
            var totalAmount = db.CartDetails
                .Where(cd => cd.Cart.UserID == userId && !(cd.Cart.IsCompleted ?? false))
                .Sum(cd => (decimal?)cd.Quantity * cd.Price) ?? 0;
            return totalAmount;
        }
    }
}