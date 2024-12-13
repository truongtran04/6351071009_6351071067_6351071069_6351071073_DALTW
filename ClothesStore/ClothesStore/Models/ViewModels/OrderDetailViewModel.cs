using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClothesStore.Models
{
    public class OrderDetailViewModel
    {
        // Thông tin từ Profile
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }

        // Thông tin từ Order
        public int OrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public string OrderStatus { get; set; }
        public decimal TotalAmount { get; set; }

        // Danh sách sản phẩm (CartDetails)
        public List<OrderDetail> OrderDetails { get; set; }
    }

}