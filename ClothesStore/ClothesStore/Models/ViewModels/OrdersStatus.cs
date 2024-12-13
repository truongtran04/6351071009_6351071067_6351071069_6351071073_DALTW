using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClothesStore.Models
{
    public enum OrderStatus
    {
        Đang_chờ_xử_lý,  // Đang chờ xử lý
        Đã_xử_lý,  // Đã xử lý
        Đã_gửi,  // Đã gửi
        Đã_giao,  // Đã giao
        Đã_hủy  // Đã hủy
    }
}
