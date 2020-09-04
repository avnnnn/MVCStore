using MVCStore.Models.Data;
using MVCStore.Models.ViewModels.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MVCStore.Areas.Admin.Controllers
{
    public class PagesController : Controller
    {
        // GET: Admin/Pages
        public ActionResult Index()
        {
            //Declare list for view (PageVM)
            List<PageVM> pageList;
            //init list (Db)
            using (Db db = new Db())
            {
                pageList = db.Pages.ToArray().OrderBy(x => x.Sorting).Select(x => new PageVM(x)).ToList();
            }
            //return list in view
            return View(pageList);
        }
        // GET: Admin/Pages/AddPage
        [HttpGet]
        public ActionResult AddPage()
        {
            return View();
        }
        // POST: Admin/Pages/AddPage
        [HttpPost]
        public ActionResult AddPage(PageVM model)
        {
            //Checking the model for validation
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            using (Db db = new Db())
            {
                //Declare variable for Slug
                string slug;
                //Init Class PageDTO
                PagesDTO dto = new PagesDTO();
                //Assign models header
                dto.Title = model.Title.ToUpper();
                //Check, is there Slug, if not, assign it
                if (string.IsNullOrWhiteSpace(model.Slug))
                {
                    slug = model.Title.Replace(" ", "-").ToLower();
                }
                else
                {
                    slug = model.Slug.Replace(" ", "-").ToLower();
                }
                //Make sure that header and slug is unique


                if (db.Pages.Any(x => x.Title == model.Title))
                {
                    ModelState.AddModelError("", "That title already exist.");
                    return View(model);
                }
                else if (db.Pages.Any(x => x.Slug == model.Slug))
                {
                    ModelState.AddModelError("", "That Slug alredy exist.");
                    return View(model);
                }

                //Assign remaining values of model
                dto.Slug = slug;
                dto.Body = model.Body;
                dto.HasSidebar = model.HasSidebar;
                dto.Sorting = 100;
                //Save to data base
                db.Pages.Add(dto);
                db.SaveChanges();
            }
            //send message throug TempData
            TempData["SM"] = "You have added a new page! ыыаыв";
            //forward user to metod Index
            return RedirectToAction("Index");
        }
        // GET: Admin/Pages/EditPage/id
        [HttpGet]
        public ActionResult EditPage(int id)
        {
            //Init model PageVM
            PageVM model;

            using (Db db = new Db())
            {
                //Get page
                PagesDTO dto = db.Pages.Find(id);
                //Check page is available
                if(dto == null)
                {
                    return Content("The page does not exist.");
                }
                //Init models data
                model = new PageVM(dto);
            }
             //return model in view
            return View(model);
        }
        // POST: Admin/Pages/EditPage/id
        [HttpPost]
        public ActionResult EditPage(PageVM model)
        {
            //Checking the model for validation
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            using (Db db = new Db())
            {
                //get page id
                int id = model.Id;

                //Declare variable for Slug
                string slug;
                //Get page by id
                PagesDTO dto = db.Pages.Find(id);
                //Assign models header
                dto.Title = model.Title;
                //Check, is there Slug, if not, assign it
                if (model.Slug != "home")
                {
                    if (string.IsNullOrWhiteSpace(model.Slug))
                    {
                        slug = model.Title.Replace(" ", "-").ToLower();
                    }
                    else
                    {
                        slug = model.Slug.Replace(" ", "-").ToLower();
                    }

                    //Make sure that header and slug is unique
                    if (db.Pages.Where(x => x.Id != id).Any(x => x.Title == model.Title))
                    {
                        ModelState.AddModelError("", "This title already exist!");
                        return View(model);
                    }
                    else if (db.Pages.Where(x => x.Id != id).Any(x => x.Slug == slug))
                    {
                        ModelState.AddModelError("", "This slug already exist!");
                        return View(model);
                    }
                    //Assign remaining values of model
                    dto.Slug = slug;
                    dto.Body = model.Body;
                    dto.HasSidebar = model.HasSidebar;
                    //Save to data base
                    db.SaveChanges();
                }
                //send message throug TempData
                TempData["SM"] = "You have edited page";
                //forward user 
                return RedirectToAction("EditPage");
            }
        }
        // GET: Admin/Pages/PageDetails/id
        public ActionResult PageDetails(int id)
        {
            //Declare model PageVM
            PageVM model;
            using (Db db = new Db())
            {
                //get page by id
                PagesDTO dto = db.Pages.Find(id);
                //Confirm that page is available
                if(dto == null)
                {
                    return Content("The page is does not exist");
                }
                //Assign to model info from db
                model = new PageVM(dto);
            }
            //return model in view
            return View(model);
        }
        // GET: Admin/Pages/Delete/id
        [HttpGet ]
        public ActionResult DeletePage(int id)
        {
            using (Db db = new Db())
            {
                //get page by id
                PagesDTO dto = db.Pages.Find(id);
                //delete page
                db.Pages.Remove(dto);
                //save changes in db
                db.SaveChanges();
            }
            //send message through TempData
            TempData["SM"] = "You have deleted a page";
            //redirect   user
            return RedirectToAction("Index");
        }
        //POST: Admin/Pages/ReorderPages
        [HttpPost]
        public void ReorderPages(int[] id)
        {
            using (Db db = new Db())
            {
                //realize start counter
                int count = 1;
                //init model of datas
                PagesDTO dto;
                //setup sorting for each page
                foreach(var pageId in id)
                {
                    dto = db.Pages.Find(pageId);
                    dto.Sorting = count;
                    db.SaveChanges();
                    count++;
                }
            }

        }
        //GET: Admin/Pages/EditSidebar
        [HttpGet]
        public ActionResult EditSidebar()
        {
            //Declare model
            SidebarVM model;
            
            using(Db db = new Db()) {
                //get all datas from db
                SidebarDTO dto = db.Sidebars.Find(1);//govnokod  constant !
                //Fill model
                model = new SidebarVM(dto);
            }
            //retrun model in view
            return View(model);
        }
        //GET: Admin/Pages/EditSidebar
        [HttpPost]
        public ActionResult EditSidebar(SidebarVM model)
        {
            using (Db db = new Db()) {
                //get data from DTO
                SidebarDTO dto = db.Sidebars.Find(1); //govnokod
                //assign datas 
                dto.Body = model.Body;
                //save changes
                db.SaveChanges();
            }
            //send message through TempData
            TempData["SM"] = "You have edited sidebar!";
             //redirect user
             return RedirectToAction("EditSidebar");
        }



    }
}