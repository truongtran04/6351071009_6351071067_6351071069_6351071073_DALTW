using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClothesStore.Models.ViewModels
{
    public class ClothesViewModel
    {
        public Cloth ClothesItem { get; set; }
        public IEnumerable<Image> Images { get; set; }
        public IEnumerable<Color> Colors { get; set; }
        public IEnumerable<Size> Sizes { get; set; }
        public IEnumerable<Clothes_Color_Size> Clothes_Color_Sizes { get; set; }
    }
}