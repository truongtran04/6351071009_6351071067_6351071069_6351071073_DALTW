using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ClothesStore.Models;

namespace ClothesStore.Library
{
    public class SelectItem
    {
        public static string CategoriesID(string categoryID)
        {
            using (TESTEntities db = new TESTEntities())
            {
                // Truy vấn trực tiếp CategoryID và CategoryName
                var categoryName = db.Categories
                    .Where(c => c.CategoryID == categoryID && c.IsHidden == false)
                    .Select(c => c.CategoryName)
                    .FirstOrDefault();

                // Nếu tìm thấy CategoryName, chuyển đổi sang slug
                if (categoryName != null)
                {
                    return XString.Str_Slug(categoryName); // Chuyển CategoryName sang slug
                }

                return null; // Trả về null nếu không tìm thấy
            }
        }
        public static string ClothingTypesID(string clothingTypesID)
        {
            using (TESTEntities db = new TESTEntities())
            {
                // Truy vấn trực tiếp CategoryID và CategoryName
                var clothingTypesName = db.ClothingTypes
                    .Where(c => c.ClothingTypeID == clothingTypesID && c.IsHidden == false)
                    .Select(c => c.ClothingTypeName)
                    .FirstOrDefault();

                // Nếu tìm thấy CategoryName, chuyển đổi sang slug
                if (clothingTypesName != null)
                {
                    return XString.Str_Slug(clothingTypesName); // Chuyển CategoryName sang slug
                }

                return null; // Trả về null nếu không tìm thấy
            }
        }
        public static string ClothingStylesID(string clothingTypesID)
        {
            using (TESTEntities db = new TESTEntities())
            {
                // Truy vấn trực tiếp CategoryID và CategoryName
                var clothingStylesName = db.ClothingStyles
                    .Where(c => c.ClothingStyleID == clothingTypesID && c.IsHidden == false)
                    .Select(c => c.ClothingStyleName)
                    .FirstOrDefault();

                // Nếu tìm thấy CategoryName, chuyển đổi sang slug
                if (clothingStylesName != null)
                {
                    return XString.Str_Slug(clothingStylesName); // Chuyển CategoryName sang slug
                }

                return null; // Trả về null nếu không tìm thấy
            }
        }
        public static string ClothesID(string clothesID)
        {
            using (TESTEntities db = new TESTEntities())
            {
                // Truy vấn trực tiếp CategoryID và CategoryName
                var clothesName = db.Clothes
                    .Where(c => c.ClothesID == clothesID && c.IsDeleted == false)
                    .Select(c => c.ClothesName)
                    .FirstOrDefault();

                // Nếu tìm thấy CategoryName, chuyển đổi sang slug
                if (clothesName != null)
                {
                    return XString.Str_Slug(clothesName); // Chuyển CategoryName sang slug
                }

                return null; // Trả về null nếu không tìm thấy
            }
        }
    }
}