using MVCStore.Areas.Admin.Models.ViewModels.Shop;
using MVCStore.Models.Data;
using MVCStore.Models.ViewModels;
using MVCStore.Models.ViewModels.Account;
using MVCStore.Models.ViewModels.Shop;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;

namespace MVCStore.Controllers
{
    public class AccountController : Controller
    {
        // GET: Account
        public ActionResult Index()
        {
            return RedirectToAction("Login");
        }
        //GET: /account/create-account
        [ActionName("create-account")]
        [HttpGet]
        public ActionResult CreateAccount()
        {
            return View("CreateAccount");
        }
        // GET: Account/Login
        [HttpGet]
        public ActionResult Login()
        {
            //account validation
            string userName = User.Identity.Name;

            if (!string.IsNullOrEmpty(userName))
                return RedirectToAction("user-profile");
            //return view
            return View();
        }
        //POST: /account/create-account
        [ActionName("create-account")]
        [HttpPost]
        public ActionResult CreateAccount(UserVM model)
        {
            // validate model 
            if (!ModelState.IsValid)
                return View("CreateAccount", model);
            //check pass. and conf. pass.
            if (!model.Password.Equals(model.ConfirmPassword))
            {
                ModelState.AddModelError("", "Password do not match");
                return View("CreateAccount", model);

            }
            using (Db db = new Db())
            {
                //check to unique user name
                if (db.Users.Any(x => x.UserName.Equals(model.UserName)))
                {
                    ModelState.AddModelError("", $"Username {model.UserName} is taken");
                    model.UserName = "";
                    return View("CreateAccount", model);
                }
                //create USerDTO

                UserDTO userDTO = new UserDTO() {
                    EmailAddress = model.EmailAddress,
                    UserName = model.UserName,
                    Password = model.Password
                };
                //add data to model
                db.Users.Add(userDTO);
                //save data
                db.SaveChanges();
                //add user role
                int id = userDTO.Id;
                UserRoleDTO userRoleDTO = new UserRoleDTO()
                {
                    UserId = id,
                    RoleId = 2
                };
                db.UserRoles.Add(userRoleDTO);
                db.SaveChanges();
                
            }
            //record in TempData
            TempData["SM"] = "You are now registered and can login";
            //redirect user
            return RedirectToAction("Login");
        }
        // POST: Account/Login
        [HttpPost]
        public ActionResult Login(LoginUserVM model)
        {
            //validate model
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            //valdidate user 
            bool isValid = false;
            using(Db db = new Db())
            {
                if (db.Users.Any(x => x.UserName.Equals(model.UserName) && x.Password.Equals(model.Password)))
                    isValid = true;
                if (!isValid)
                {
                    ModelState.AddModelError("", "Invalid username or password");
                    return View(model);
                }
                else
                {
                    FormsAuthentication.SetAuthCookie(model.UserName, model.RememberMe);
                    return Redirect(FormsAuthentication.GetRedirectUrl(model.UserName, model.RememberMe));
                }
            }   
        }
        // GET: Account/Logout
        [HttpGet]
        [Authorize]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Login");
        }
        [Authorize]
        public ActionResult UserNavPartial()
        {
            string userName = User.Identity.Name;
            UserNavPartialVM model = new UserNavPartialVM() { UserName = userName };

            //return view
            return PartialView("_UserNavPartial", model);
        }
        // GET: Account/user-profile
        [HttpGet]
        [ActionName("user-profile")]
        [Authorize]
        public ActionResult UserProfile()
        {
            //get UserName
            string userName = User.Identity.Name;
            //declare model
            UserProfileVM model;
            using (Db db = new Db())
            {
                //get user
                UserDTO dto = db.Users.FirstOrDefault(x => x.UserName == userName);
                //inti model with datas
                model = new UserProfileVM(dto);
            }
            //return model in view


            return View("UserProfile",model);
        }
        // POST: Account/user-profile
        [HttpPost]
        [ActionName("user-profile")]
        [Authorize]
        public ActionResult UserProfile(UserProfileVM model)
        {
            bool isNameChanged = false;
            //validate model
            if (!ModelState.IsValid)
            {
                return View("UserProfile", model);  
            }

            //check password changes
            if (!string.IsNullOrWhiteSpace(model.Password))
            {
                if (!(model.Password == model.ConfirmPassword))
                {
                    ModelState.AddModelError("", "Password do not match");
                    return View("UserProfile", model);
                }
            }
            using (Db db = new Db()) {
                //get user name
                string userName = User.Identity.Name;

                if(userName != model.UserName)
                {
                    userName = model.UserName;
                    isNameChanged = true;
                }
                //check to unique name if it needs
                if (db.Users.Where(x => x.Id != model.Id).Any(x => x.UserName ==  userName))
                {
                    ModelState.AddModelError("", $"User name {model.UserName} is already exist");
                    model.UserName = "";
                    return View("UserProfile", model);

                }
                //change model  
                UserDTO dto = db.Users.Find(model.Id);
                //save data
                dto.UserName = model.UserName;
                dto.EmailAddress = model.EmailAddress;
                if (!string.IsNullOrWhiteSpace(model.Password))
                {
                    dto.Password = model.Password;
                }
                db.SaveChanges();
            }

            //record to  TempData
            TempData["SM"] = "You profile was changed";
            //return VIew with model

            if (!isNameChanged)
                return View("UserProfile", model);
            else
                return RedirectToAction("Logout");
        }
        //GET: Account/orders
        [HttpGet]
        [Authorize(Roles ="User")]
        public ActionResult Orders()
        {
            //init model OrdersForUserVM
            List<OrdersForUserVM> ordersForUsers = new List<OrdersForUserVM>();
            using (Db db = new Db()) {
                //get userId
                UserDTO user = db.Users.FirstOrDefault(x => x.UserName == User.Identity.Name);

                //init model OrderVM
                List<OrderVM> orders = db.Orders.Where(x => x.UserId == user.Id).ToArray().Select(x => new OrderVM(x)).ToList();

                //get list of products in OrderVM
                foreach (var order in orders)
                {
                    //declare dictionary
                    Dictionary<string, int> productAndQty = new Dictionary<string, int>();
                    //declare total cost 
                    decimal total = 0m;
                    //init model OrderDetailsDTO
                    List<OrderDetailsDTO> orderDetails = db.OrderDetails.Where(x => x.OrderId == order.OrderId).ToList();
                    //get list OrderDetailsDTO
                    foreach (var orderDetail in orderDetails) {
                        //get product
                        ProductDTO product = db.Products.FirstOrDefault(x => x.Id == orderDetail.ProductId);
                        //get product price
                        decimal price = product.Price;
                        //get product name
                        string productName = product.Name;
                        //add product to dictionary
                        productAndQty.Add(productName, orderDetail.Quantity);
                        //get total cost
                        total = orderDetail.Quantity * price;
                    }
                    //fill OrderForUserVM model with data
                    ordersForUsers.Add(new OrdersForUserVM()
                    {
                        OrderNumber = order.OrderId,
                        Total = total,
                        ProductsAndQty = productAndQty,
                        CreatedAt = order.CreatedAt
                                
                    }) ;
                }
            }
            //return model in view
            return View(ordersForUsers);

        }


    }
}