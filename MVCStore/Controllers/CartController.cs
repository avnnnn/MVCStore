using MVCStore.Models.Data;
using MVCStore.Models.ViewModels.Cart;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;

namespace MVCStore.Controllers
{
    public class CartController : Controller
    {
        // GET: Cart
        public ActionResult Index()
        {
            //declare list
            var cart = Session["cart"] as List<CartVM> ?? new List<CartVM>();
            //check to empty cart
            if (cart.Count == 0 || Session["cart"] == null)
            {
                ViewBag.Message = "Your cart is empty";
                return View();
            }
            decimal total = 0m;
            //else  record total in ViewBag
            foreach (var item in cart)
            {
                total += item.Total;
            }
            ViewBag.GrandTotal = total;
            //return list in View

            return View(cart);
        }
        public ActionResult CartPartial()
        {
            //declare model
            CartVM model = new CartVM();
            //declare quantity
            int quantity = 0;
            //declare price
            decimal price = 0m;

            //check cart session
            if (Session["cart"] != null)
            {
                //get total quanity and price
                var list = (List<CartVM>)Session["cart"];
                foreach (var item in list)
                {
                    quantity = item.Quantity;
                    price += item.Quantity * item.Price;
                }
                model.Quantity = quantity;
                model.Price = price;
            }
            else
            {
                model.Quantity = 0;
                model.Price = 0;
            }

            //return model to partial view

            return PartialView("_CartPartial", model);
        }
        public ActionResult AddToCartPartial(int id)
        {
            //declare list of CartVM
            List<CartVM> cart = Session["cart"] as List<CartVM> ?? new List<CartVM>();
            //declare model CartVM
            CartVM model = new CartVM();
            using (Db db = new Db())
            {
                //get product
                ProductDTO dto = db.Products.Find(id);
                //check to already existing in cart
                var productInCart = cart.FirstOrDefault(x => x.ProductId == id);
                //if no than add new product
                if (productInCart == null)
                {
                    cart.Add(new CartVM()
                    {
                        ProductId = dto.Id,
                        ProductName = dto.Name,
                        Quantity = 1,
                        Price = dto.Price,
                        Image = dto.ImageName

                    });
                }
                //else increment quantity
                else
                {
                    productInCart.Quantity++;
                }
            }
            //get total quantity of products abd add to model
            int quantity = 0;
            decimal price = 0m;

            foreach (var item in cart)
            {
                quantity += item.Quantity;
                price += item.Quantity * item.Price;
            }
            model.Quantity = quantity;
            model.Price = price;
            //save cart in session
            Session["cart"] = cart;
            //return  model in partial view 
            return PartialView("_AddToCartPartial", model);

        }
        //GET: /cart/IncrementProduct
        public JsonResult IncrementProduct(int productId)
        {
            //Declare CartVM list
            List<CartVM> cart = Session["cart"] as List<CartVM>;
            using (Db db = new Db())
            {
                //get model from list
                CartVM model = cart.FirstOrDefault(x => x.ProductId == productId);
                //increment quantity
                model.Quantity++;

                //save changes
                var result = new { qty = model.Quantity, price = model.Price };
                //return JSON
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }
        //GET: /cart/DecrementProduct
        public JsonResult DecrementProduct(int productId)
        {
            List<CartVM> cart = Session["cart"] as List<CartVM>;
            using (Db db = new Db())
            {
                //get model from list
                CartVM model = cart.FirstOrDefault(x => x.ProductId == productId);
                //decrement quantity
                if (model.Quantity > 1)
                {
                    model.Quantity--;
                }
                else
                {
                    model.Quantity = 0;
                    cart.Remove(model);
                }
                //save changes
                var result = new { qty = model.Quantity, price = model.Price };
                //return JSON
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }
        public void RemoveProduct(int productId)
        {
            List<CartVM> cart = Session["cart"] as List<CartVM>;
            using (Db db = new Db())
            {
                //get model from list
                CartVM model = cart.FirstOrDefault(x => x.ProductId == productId);
                //remove 
                cart.Remove(model);
            }
        }
        public ActionResult PaypalPartial()
        {
            //get list of products in cart
            List<CartVM> cart = Session["cart"] as List<CartVM>;
            //return partial view with list
            return PartialView(cart);
        }
        //POST /cart/Palceorder
        [HttpPost]
        public void PlaceOrder()
        {
            //get list of products in cart
            List<CartVM> cart = Session["cart"] as List<CartVM>;
            //get user name
            string userName = User.Identity.Name;
            //declare var for OrderId
            int orderId = 0;
            using (Db db = new Db())
            {
                //declare OrderDTO
                OrderDTO order = new OrderDTO();
                //get UserID

                int userID = db.Users.FirstOrDefault(x => x.UserName == userName).Id;

                //fill OrderDTO and save
                order.UserId = userID;
                order.CreatedAt = DateTime.Now;

                db.Orders.Add(order);
                db.SaveChanges();
                //get OrderID
                orderId = order.OrderId;
                //declare OrderDetailsDTO
                OrderDetailsDTO orderDetails = new OrderDetailsDTO();
                //fill OrderDetailsDTO
                foreach(var item in cart)
                {
                    orderDetails.OrderId = orderId;
                    orderDetails.UserId = userID;
                    orderDetails.ProductId = item.ProductId;
                    orderDetails.Quantity = item.Quantity;

                    db.OrderDetails.Add(orderDetails);
                    db.SaveChanges();
                }
            }

            //send message about order to administrator
            var client = new SmtpClient("smtp.mailtrap.io", 2525)
            {
                Credentials = new NetworkCredential("6fb5ccccba3d45", "9e04145e6817a7"),
                EnableSsl = true
            };
            client.Send("shop@example.com", "admin@example.com", "New Order", $"You have a new Order. Order number: {orderId}");

            //nullify session
            Session["Cart"] = null;
        }
    }
}