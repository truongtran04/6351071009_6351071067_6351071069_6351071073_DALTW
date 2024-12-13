using System.Web.Mvc;

namespace ClothesStore.Areas.Admin
{
    public class AdminAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "Admin";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "Admin_default",
                "Admin/{controller}/{action}/{id}",
                new { Controller = "DashBoard", action = "Index", id = UrlParameter.Optional },
                namespaces: new[] { "ClothesStore.Areas.Admin.Controllers" }  // Specify admin controllers
            );
        }
    }
}