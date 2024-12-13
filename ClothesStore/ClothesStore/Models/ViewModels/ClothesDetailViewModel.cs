using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClothesStore.Models
{
    public class ClothesDetailViewModel
    {
        public Cloth Cloth { get; set; }
        public List<Image> Images { get; set; }
        public List<ColorSizeQuantity> ColorSizeQuantities { get; set; } // Danh sách nhóm theo màu, kích thước và số lượng
        public bool IsDeleted { get; set; }
    }

    public class ColorSizeQuantity
    {
        public Color Color { get; set; }
        public List<SizeQuantity> SizeQuantities { get; set; } // Danh sách kích thước và số lượng
    }

    public class SizeQuantity
    {
        public Size Size { get; set; }
        public int Quantity { get; set; }
    }

}