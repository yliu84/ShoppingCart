using CmsShoppingCart.Models.Data;
using CmsShoppingCart.Models.ViewModels.Pages;
using CmsShoppingCart.Models.ViewModels.Shop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CmsShoppingCart.Controllers
{
    public class PagesController : Controller
    {
        // GET: Pages
        public ActionResult Index(string page="")
        {
            //Get/set page slug
            if (page == "")
                page = "home";

            //Declear model and DTO
            PageVM model;
            PageDTO dto;

            //Check if page exists
            using (Db db = new Db())
            {
                if(!db.pages.Any(x =>x.slug.Equals(page)))
                {
                    return RedirectToAction("Index", new { page = "" });
                }
            }

            //Get page DTO
            using(Db db = new Db())
            {
                dto = db.pages.Where(x => x.slug == page).FirstOrDefault();
            }

            //Set Page Title
            ViewBag.PageTitle = dto.Title;

            //Check for sidebar
            if (dto.HasSidebar == true)
            {
                ViewBag.Sidebar = "Yes";
            }
            else
            {
                ViewBag.Sidebar = "No";
            }

            //Init model
            model = new PageVM(dto);

            //Return view with model
            return View(model);
        }

        public ActionResult PagesMenuPartial()
        {
            //Declare list of pageVM
            List<PageVM> pageVMList;


            //Get all page except home
            using (Db db = new Db())
            {
                pageVMList = db.pages.ToArray().OrderBy(x => x.Sorting).Where(x => x.slug != "home").Select(x => new PageVM(x)).ToList();
            }

            //Return partial view with list
            return PartialView(pageVMList);
        }

        public ActionResult SidebarPartial()
        {
            //Declare model
            SidebarVM model;

            //Init model
            using(Db db = new Db())
            {
                SidebarDTO dto = db.Sidebar.Find(1);

                model = new SidebarVM(dto);


            }


            //Return partial view with model
            return PartialView(model);
        }

       
    }
}