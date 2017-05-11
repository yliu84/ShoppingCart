using CmsShoppingCart.Models.Data;
using CmsShoppingCart.Models.ViewModels.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CmsShoppingCart.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class PagesController : Controller
    {
        // GET: Admin/Pages
        public ActionResult Index()
        {   
            //Declare list of pageVM
            List<PageVM> pagesList;

            using (Db db = new Db())
            {
                //Init the list
                pagesList = db.pages.ToArray().OrderBy(x => x.Sorting).Select(x => new PageVM(x)).ToList();
            }
            //Return View with list
            return View(pagesList);
        }

        //GET:Admin/Pages/AddPage
        [HttpGet]
        public ActionResult AddPage()
        {
            return View();
        }

        //POST:Admin/Pages/AddPage
        [HttpPost]
        public ActionResult AddPage(PageVM model)
        {
            //Check model state
            if(!ModelState.IsValid)
            {
                return View(model);
            }

            using(Db db = new Db())
            {
                //Declare slug
                string slug;

                //Init PageDTO
                PageDTO dto = new PageDTO();

                //DTO title
                dto.Title = model.Title;

                //Check for and set slug if need be
                if(string.IsNullOrEmpty(model.slug))
                {
                    slug = model.Title.Replace(" ", "-").ToLower();
                }
                else
                {
                    slug = model.slug.Replace(" ", "-").ToLower();
                }

                //Make sure title and slug are unique
                if(db.pages.Any(x => x.Title == model.Title) || db.pages.Any(x => x.slug == slug))
                {
                    ModelState.AddModelError("", "That title or slug already exists.");
                }

                //DTO the rest
                dto.slug = slug;
                dto.Body = model.Body;
                dto.HasSidebar = model.HasSidebar;
                dto.Sorting = 100;

                //Save DTO
                db.pages.Add(dto);
                db.SaveChanges();
            }


            //Set template message
            TempData["SM"] = "You have added a new page!";

            //Redirct


            return RedirectToAction("AddPage");
        }

        //GET:Admin/Pages/EditPage/id
        [HttpGet]
        public ActionResult EditPage(int id)
        {
            //Declare pageVM
            PageVM model;

            
            using (Db db = new Db())
            {
                //Get the page
                PageDTO dto = db.pages.Find(id);

                //confirm page exists
                if(dto == null)
                {
                    return Content("The page does not exist.");
                }

                //Init pageVM
                model = new PageVM(dto);


            }

            //Return view with model
            return View(model);

            
        }


        //POST:Admin/Pages/EditPage/id
        [HttpPost]
        public ActionResult EditPage(PageVM model)
        {

            //Check model state
            if(!ModelState.IsValid)
            {
                return View(model);
            }

            using(Db db = new Db())
            {
                //GET page id
                int id = model.Id;

                //Delcare slug
                string slug = "home";

                //Get the page
                PageDTO dto = db.pages.Find(id);

                //DTO the title
                dto.Title = model.Title;

                //CHeck for slug and set it if need be
                if(model.slug != "home")
                {
                    if(string.IsNullOrWhiteSpace(model.slug))
                    {
                        slug = model.Title.Replace(" ", "-").ToLower();
                    }
                    else
                    {
                        slug = model.slug.Replace(" ", "-").ToLower();
                    }
                }

                //Make sure title and slug are unique
                if(db.pages.Where(x => x.Id != id).Any(x => x.Title == model.Title) ||
                    db.pages.Where(x => x.Id != id).Any(x => x.slug == model.slug))
                {
                    ModelState.AddModelError("", "That title or slug already exists.");
                }

                //DTO the rest
                dto.slug = slug;
                dto.Body = model.Body;
                dto.HasSidebar = model.HasSidebar;

                //Save the DTO
                db.SaveChanges();
            }

            //Set TempData message
            TempData["SM"] = "You have edited the page!";

            //Redirect
            return RedirectToAction("EditPage");
        }


        //GET:Admin/Pages/PageDetails/id
        public ActionResult PageDetails(int id)
        {
            //Declear pageVM
            PageVM model;

            using(Db db = new Db())
            {
                PageDTO dto = db.pages.Find(id);

                //Confirm page exists
                if(dto == null)
                {
                    return Content("The page does not exist");
                }

                //Init pageVM
                model = new PageVM(dto);
            }

            //Return view with model
            return View(model);
        }

        public ActionResult DeletePage(int id)
        {
            using(Db db = new Db())
            { 
                //GET the page
                PageDTO dto = db.pages.Find(id);

                //REMOVE the page
                db.pages.Remove(dto);


                //Save
                db.SaveChanges();
            }
            //Redirect

            return RedirectToAction("Index");
        }

        //POST:Admin/Pages/ReorderPages
        [HttpPost]
        public void ReorderPages(int[] id)
        {
            using (Db db = new Db()) {
                //Set initial count
                int count = 1;

                //Declare pageDTO
                PageDTO dto;


                //Set Sorting for each page
                foreach(var pageId in id)
                {
                    dto = db.pages.Find(pageId);
                    dto.Sorting = count;

                    db.SaveChanges();

                    count++;
                }
            }
        }

        //GET:Admin/Pages/EditSidebar
        public ActionResult EditSidebar()
        {
            //Declare model
            SidebarVM model;

            using(Db db = new Db())
            {
                //get the DTO
                SidebarDTO dto = db.Sidebar.Find(1);

                //Init model
                model = new SidebarVM(dto);
            }

            //Return view with model
            return View(model);
           

        }

        //POST:Admin/Pages/EditSidebar
        [HttpPost]
        public ActionResult EditSidebar(SidebarVM model)
        {
            using(Db db = new Db())
            {
                //Get the DTO
                SidebarDTO dto = db.Sidebar.Find(1);

                //DTO the body
                dto.Body = model.Body;

                //Save
                db.SaveChanges();
            }
           
            //Set TempData message
            TempData["SM"] = "You have edited the sidebar!";

            //Redirect

            return RedirectToAction("EditSidebar");
        }
    }
}