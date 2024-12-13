using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClothesStore.Models
{
    public class ColorSizesViewModel
    {
        public string ColorID { get; set; }
        public List<SizeQuantityViewModel> Sizes { get; set; }
    }
    public class SizeQuantityViewModel
    {
        public string SizeID { get; set; }
        public string SizeName { get; set; }
        public int Quantity { get; set; }

    }
}