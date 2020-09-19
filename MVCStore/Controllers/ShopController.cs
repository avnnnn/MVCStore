using MVCStore.Models.Data;
using MVCStore.Models.ViewModels.Shop;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;

namespace MVCStore.Controllers
{
    public class ShopController : Controller
    {
        // GET: Shop
        [HttpGet]
        public ActionResult Index()
        {
            return RedirectToAction("Index", "Pages");
        }
        public ActionResult CategoryMenuPartial()
        {
            //declare list of categories
            List<CategoryVM> categoryVMList;

            //get categories
            using (Db db = new Db())
            {
                categoryVMList = db.Categories.ToArray().OrderBy(x => x.Sorting).Select(x => new CategoryVM(x)).ToList();
            }


            // return list in partial view
            return PartialView("_CategoryMenuPartial", categoryVMList);
        }
        // GET: Shop/Category/name
        [HttpGet]
        public ActionResult Category(string name)
        {
            //declare list 
            List<ProductVM> productVMList;

            //init category id
            using (Db db = new Db())
            {
                CategoryDTO categordDto = db.Categories.Where(x => x.Slug == name).FirstOrDefault();
                int catId = categordDto.Id;

                //init list with data
                productVMList = db.Products.ToArray().Where(x => x.CategoryId == catId).Select(x => new ProductVM(x)).ToList();

                //get category name
                var productCat = db.Products.Where(x => x.CategoryId == catId).FirstOrDefault();

                //check to null
                if(productCat == null)
                {
                    var catName = db.Categories.Where(x => x.Slug == name).Select(x => x.Name).FirstOrDefault();
                    ViewBag.CategoryName = catName;
                }
                else
                {
                    ViewBag.CategoryName = productCat.CategoryName;


                }

            }

            //retrurn view with model

            return View(productVMList);
        }
        // GET: Shop/product-details/name
        [HttpGet]
        [ActionName("product-details")]
        public ActionResult ProductDetails(string name)
        {
            //declare model DTO and VM
            ProductDTO dto;
            ProductVM model;

            //init product id
            int id = 0;
            using (Db db = new Db())
            {
                //check to availibility of product
                if(!db.Products.Any(x => x.Slug.Equals(name)))
                {
                    return RedirectToAction("Index", "Shop");
                }
                //init model dto with datas
                dto = db.Products.Where(x => x.Slug == name).FirstOrDefault();
                //get id
                id = dto.Id;
                //init model vm  with data
                model = new ProductVM(dto);
            }
            //get picture from gallery
            model.GalleryImages = Directory.EnumerateFiles(Server.MapPath("~/Images/Uploads/Products/" + id + "/Gallery/Thumbs"))
                .Select(fn => Path.GetFileName(fn));
            //return model in view




            return View("ProductDetails", model);
        }
    }
}