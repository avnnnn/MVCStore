using MVCStore.Models.Data;
using MVCStore.Models.ViewModels.Shop;
using PagedList;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;

namespace MVCStore.Areas.Admin.Controllers
{
    public class ShopController : Controller
    {
        // GET: Admin/Shop
        [HttpGet]

        public ActionResult Categories()
        {
            //Declare model as List type
            List<CategoryVM> categories;
            using (Db db = new Db())
            {
                //Inti model with data
                categories = db.Categories.
                    ToArray()
                    .OrderBy(x => x.Sorting)
                    .Select(x => new CategoryVM(x))
                    .ToList();
            }
            //Return model in view
            return View(categories);
        }

        // POST: Admin/Shop/AddNewCategory
        [HttpPost]
        public string AddNewCategory(string catName)
        {
            //declatre string variable ID
            string id;
            using (Db db = new Db())
            {
                //check name for uniqe
                if (db.Categories.Any(x => x.Name == catName))
                    return "titletaken";
                //init dto model
                CategoryDTO dto = new CategoryDTO();
                //add data into model
                dto.Name = catName;
                dto.Slug = catName.Replace(" ", "-").ToLower();
                dto.Sorting = 100;
                //save to db
                db.Categories.Add(dto);
                db.SaveChanges();

                //get id for return
                id = dto.Id.ToString();
            }
            //return 
            return id;
        }
        //POST: Admin/Shop/ReorderCategories
        [HttpPost]
        public void ReorderCategories(int[] id)
        {
            using (Db db = new Db())
            {
                //realize start counter
                int count = 1;
                //init model of datas
                CategoryDTO dto;
                //setup sorting for each page
                foreach (var catId in id)
                {
                    dto = db.Categories.Find(catId);
                    dto.Sorting = count;
                    db.SaveChanges();
                    count++;
                }
            }

        }
        // GET: Admin/Pages/Delete/id
        [HttpGet]
        public ActionResult DeleteCategories(int id)
        {
            using (Db db = new Db())
            {
                //get category by id
                CategoryDTO dto = db.Categories.Find(id);
                //delete category
                db.Categories.Remove(dto);
                //save changes in db
                db.SaveChanges();
            }
            //send message through TempData
            TempData["SM"] = "You have deleted a category";
            //redirect   user
            return RedirectToAction("Categories");
        }
        //POST: Admin/Shop/RenameCategory
        [HttpPost]
        public string RenameCategory(string newCatName, int id)
        {
            using (Db db = new Db()) {
                //check name for uniqie
                if (db.Categories.Any(x => x.Name == newCatName))
                    return "titletaken";

                //het DTO model
                CategoryDTO dto = db.Categories.Find(id);
                //edit model
                dto.Name = newCatName;
                dto.Slug = newCatName.Replace(" ", "-").ToLower();
                //save changes
                db.SaveChanges();
            }
            //return name
            return newCatName;
        }
        // GET: Admin/Shop/AddProduct
        [HttpGet]
        public ActionResult AddProduct()
        {
            //declare model
            ProductVM model = new ProductVM();
            //add list of categories from db
            using (Db db = new Db())
            {
                model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
            }
            //return model
            return View(model);
        }
        // POST: Admin/Shop/AddProduct
        [HttpPost]
        public ActionResult AddProduct(ProductVM model, HttpPostedFileBase file)
        {
            
            //Validation
            if ((!ModelState.IsValid))
            {
                using (Db db = new Db())
                {
                    model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                    return View(model);
                }
            }

            //Check product name for unique
            using (Db db = new Db())
            {
                if (db.Products.Any(x => x.Name == model.Name))
                {
                    model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                    ModelState.AddModelError("", "That product name is already exists");
                    return View(model);
                }
            }
             //declare variable productId
            int id;

           //init and save model DTO
            using(Db db = new Db()){
                ProductDTO product = new ProductDTO();
                product.Name = model.Name;
                product.Slug = model.Name.Replace(" ", "-").ToLower();
                product.Description = model.Description;
                product.Price = model.Price;
                product.CategoryId = model.CategoryId;

                CategoryDTO catDTO = db.Categories.FirstOrDefault(x => x.Id == model.CategoryId);
                product.CategoryName = catDTO.Name;
                db.Products.Add(product);
                db.SaveChanges();
                id = product.Id;
            
            }
            //send message through TempDAta
            TempData["SM"] = "You have added a product";

            #region Upload Image
            //create nessesary links to directory
            var originalDirectory = new DirectoryInfo(string.Format($"{Server.MapPath(@"\")}Images\\Uploads"));
            var pathString1 = Path.Combine(originalDirectory.ToString(), "Products");

            var pathString2 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString());

            var pathString3 = Path.Combine(originalDirectory.ToString(), "Products\\"+id.ToString() + "\\Thumbs");

            var pathString4 = Path.Combine(originalDirectory.ToString(), "Products\\"+id.ToString() + "\\Gallery");
           
            var pathString5 = Path.Combine(originalDirectory.ToString(), "Products\\"+id.ToString() + "\\Gallery\\Thumbs");

            //check if the directory exists(if not, than create)
            if (!Directory.Exists(pathString1))
                Directory.CreateDirectory(pathString1);

            if (!Directory.Exists(pathString2))
                Directory.CreateDirectory(pathString2);

            if (!Directory.Exists(pathString3))
                Directory.CreateDirectory(pathString3);

            if (!Directory.Exists(pathString4))
                Directory.CreateDirectory(pathString4);

            if (!Directory.Exists(pathString5))
                Directory.CreateDirectory(pathString5);

            //Check, is file uploaded
            if (file != null && file.ContentLength > 0)
            {
                //get FileName
                string ext = file.ContentType.ToLower();
                //check file extention 
                if (ext != "image/jpg" &&
                    ext != "image/jpeg" &&
                    ext != "image/pjpeg" &&
                    ext != "image/gif" &&
                    ext != "image/x-png" &&
                    ext != "image/png")
                {
                    using (Db db = new Db())
                    {
                        model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                        ModelState.AddModelError("", "The image was not uploaded - wrong image extension");
                        return View(model);
                    }
                }



                //declare variable with file name
                string imageName = file.FileName;
                //save name in model DTO
                using (Db db = new Db())
                {
                    ProductDTO dto = db.Products.Find(id);
                    dto.ImageName = imageName;
                    db.SaveChanges();
                }
                //Assign path to original and thumbs
                var path1 = string.Format($"{pathString2}\\{imageName}");

                var path2 = string.Format($"{pathString3}\\{imageName}");

                //Save original img
                file.SaveAs(path1);

                //create and save decreased copy
                WebImage img = new WebImage(file.InputStream);
                img.Resize(200, 200);
                img.Save(path2);
            }
#endregion




            //redirect user
            return RedirectToAction("AddProduct");
        }
        // GET: Admin/Shop
        [HttpGet]
        public ActionResult Products(int? page, int? catId)
        {
            //declare ProductVM List
            List<ProductVM> listOfProductsVM;
            //set page number
            var pageNum = page?? 1;
            using (Db db = new Db())
            {
                //init list and fill it
                listOfProductsVM = db.Products.ToArray()
                    .Where(x => catId == null || catId == 0 || x.CategoryId == catId)
                    .Select(x => new ProductVM(x))
                    .ToList();
                //fill category for sort

                ViewBag.Categories = new SelectList(db.Categories.ToList(), "Id","Name");

                //set choised category
                ViewBag.SelectedCat = catId.ToString();
            }
            //pagination
            var onePageOfProducts = listOfProductsVM.ToPagedList(pageNum, 3);
            ViewBag.onePageOfProducts = onePageOfProducts;
            //return view



            return View(listOfProductsVM);
        }
        // GET: Admin/Shop/EditProduct/id
        [HttpGet]
        public ActionResult EditProduct(int id)
        {
            //declare  model ProductVM
            ProductVM model;
            using (Db db = new Db())
            {
                //get Product
                ProductDTO dto = db.Products.Find(id);
                //check availability of product
                if(dto == null)
                {
                    return Content("That product does not exist");
                }
                //init models data
                model = new ProductVM(dto);
                //create list of categories
                model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");

                //get image from gallery
                model.GalleryImages = Directory
                    .EnumerateFiles(Server.MapPath("/Images/Uploads/Products/" + id + "/Gallery/Thumbs"))
                    .Select(fn => Path.GetFileName(fn));
            }
            //return model in view
            return View(model);
        }
        // POST: Admin/Shop/EditProduct
        [HttpPost]
        public ActionResult EditProduct(ProductVM model, HttpPostedFileBase file)
        {
            //get product id
            int id = model.Id;

            //fill list with categories and pictures
            using(Db db = new Db())
            {
                model.Categories = new SelectList(db.Categories, "Id", "Name");
            }
            model.GalleryImages = Directory
                .EnumerateFiles(Server.MapPath("/Images/Uploads/Products/" + id + "/Gallery/Thumbs"))
                .Select(fn => Path.GetFileName(fn));           
            //Validate moedel
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            //check product name to unique
            using (Db db = new Db()) {
                if (db.Products.Where(x => x.Id != id).Any(x => x.Name == model.Name))
                {
                    model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                    ModelState.AddModelError("", "That product name is taken");
                    return View(model);
                }

            }
            //update product
            using (Db db = new Db())
            {
                ProductDTO dto = db.Products.Find(id);
                dto.Name = model.Name;
                dto.Slug = model.Name.Replace(" ", "-").ToLower();
                dto.Description = model.Description;
                dto.Price = model.Price;
                dto.CategoryId = model.CategoryId;
                dto.ImageName = model.ImageName;

                CategoryDTO categoryDTO = db.Categories.FirstOrDefault(x => x.Id == model.CategoryId);
                dto.CategoryName = categoryDTO.Name;
                db.SaveChanges();

            }

            //message for TempData
            TempData["SM"] = "You have edited a product";
            //  
            #region ImageUpload

            //check file load
            
            if (file != null && file.ContentLength > 0)
            {

                //check file extension
                string ext = file.ContentType.ToLower();
                //check extension
                if (ext != "image/jpg" &&
                    ext != "image/jpeg" &&
                    ext != "image/pjpeg" &&
                    ext != "image/gif" &&
                    ext != "image/x-png" &&
                    ext != "image/png")
                {
                    using (Db db = new Db())
                    {
                        ModelState.AddModelError("", "The image was not uploaded - wrong image extension");
                        return View(model);
                    }
                }
                //set path 
                var originalDirectory = new DirectoryInfo(string.Format($"{Server.MapPath(@"\")}Images\\Uploads"));
                var pathString1 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString());
                var pathString2 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Thumbs");

                //delete exiting files and directory
                DirectoryInfo di1 = new DirectoryInfo(pathString1);
                DirectoryInfo di2 = new DirectoryInfo(pathString2);

                foreach (var file1 in di1.GetFiles())
                {
                    file1.Delete();
                }
                foreach (var file2 in di1.GetFiles())
                {
                    file2.Delete();
                }
                //save image Name
                string imageName = file.FileName;
                using (Db db = new Db())
                {
                    ProductDTO dto = db.Products.Find(id);
                    dto.ImageName = imageName;
                    db.SaveChanges();
                }
                //save original and preview
                var path1 = string.Format($"{pathString1}\\{imageName}");

                var path2 = string.Format($"{pathString2}\\{imageName}");

                //Save original img
                file.SaveAs(path1);

                //create and save decreased copy
                WebImage img = new WebImage(file.InputStream);
                img.Resize(200, 200);
                img.Save(path2);
            }

            #endregion

            //forward user
            return RedirectToAction("EditProduct");
        }

    }
}