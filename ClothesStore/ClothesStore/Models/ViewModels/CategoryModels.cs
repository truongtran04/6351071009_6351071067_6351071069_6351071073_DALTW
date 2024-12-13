using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClothesStore.Models.ViewModels
{
    public class CategoryModels
    {
        public List<Category> categories { get; set; }
        public List<ClothesViewModel> clothes { get; set; }
        public List<Category_ClothingType> clothingTypes { get; set; }
    }
}