using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace ClothesStore.Models.DAO
{
    public class CategoriesDAO
    {
        private TESTEntities db = new TESTEntities();

        public List<Category> getList(string status = "All")
        {
            List<Category> list = null;
            switch (status)
            {
                case "Index":
                    {
                        list = db.Categories.Where(row => row.IsHidden == false).ToList();
                        break;
                    }
                case "Trash":
                    {
                        list = db.Categories.Where(row => row.IsHidden != false).ToList();
                        break;
                    }
                default:
                    {
                        list = db.Categories.ToList();
                        break;
                    }

            }
            return list;
        }
        public Category getRow(string id)
        {
            if (id == null)
            {
                return null;
            }
            else
            {
                return db.Categories.Find(id);
            }
        }
        public int Insert(Category category)
        {
            db.Categories.Add(category);
            return db.SaveChanges();
        }
        public int Update(Category category)
        {
            db.Entry(category).State = EntityState.Modified;
            return db.SaveChanges();
        }

        public int Delete()
        {
            return db.SaveChanges();
        }
    }
}