using MVCStore.Models.Data;
using MVCStore.Models.ViewModels.Pages;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace MVCStore.Controllers
{
    public class PagesController : Controller
    {
        // GET: Index/{page}
        [HttpGet]
         public ActionResult Index(string page = "")
        {
            // get/set  slug
            if (string.IsNullOrEmpty(page))
                page = "home";
            //declare models and class dto
            PageVM model;
            PagesDTO dto;

            //check page availiblity  
            using (Db db = new Db())
            {
                if (!db.Pages.Any(x => x.Slug == (page)))
                    return RedirectToAction("Index", new { page = "" });
            }
            //get dto of page
            using (Db db = new Db())
            {
                dto = db.Pages.Where(x => x.Slug == page).FirstOrDefault();
            }
            //set title of page
            ViewBag.PageTitle = dto.Title;
            //check sidebar
            ViewBag.SideBar = dto.HasSidebar;
            
            //fill model with data
            model = new PageVM(dto);
            //return view
            return View(model);
        }



        public ActionResult PagesMenuPartial()
        {
            //init  list of page
            List<PageVM> pageVMList;
            //get all pages except home
            using(Db db = new Db())
            {
                pageVMList = db.Pages.ToArray().OrderBy(x => x.Sorting).Where(x => x.Slug != "home").Select(x => new PageVM(x)).ToList();
            }
            //return list in partial  view

            return PartialView("_PagesMenuPartial", pageVMList);
        }

        public ActionResult SidebarPartial()
        {
            //declare model
            SidebarVM model;

            //init model
            using (Db db = new Db())
            {
                SidebarDTO dto = db.Sidebars.Find(1);
                model = new SidebarVM(dto);
            }

            //return model in partial view
            return PartialView("_SidebarPartial",model);
        }
    }
}