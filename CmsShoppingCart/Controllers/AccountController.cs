using System;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using CmsShoppingCart.Models;
using CmsShoppingCart.Models.ViewModels.Account;
using CmsShoppingCart.Models.Data;
using System.Web.Security;
using System.Collections.Generic;
using CmsShoppingCart.Models.ViewModels.Shop;

namespace CmsShoppingCart.Controllers
{
    
    public class AccountController : Controller
    {
        //GET:Account
        public ActionResult Index()
        {
            return Redirect("~/account/login");
        }

        //GET:/account/login
        [HttpGet]
        public ActionResult Login()
        {
            //Confirm user is not loggged in

            string username = User.Identity.Name;

            if (!string.IsNullOrEmpty(username))
                return RedirectToAction("user-profile");

            return View();
        }

        //Post:/account/login
        [HttpPost]
        public ActionResult Login(LoginUserVM model)
        {
            //Check model state
            if(!ModelState.IsValid)
            {
                return View(model);
            }

            //Check if the user is valid

            bool isValid = false;

            using(Db db = new Db())
            {
                if(db.Users.Any(x=>x.UserName.Equals(model.UserName) && x.Password.Equals(model.Password)))
                {
                    isValid = true;
                }
            }

            if(!isValid)
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

        //GET:/account/create-account
        [ActionName("create-account")]
        [HttpGet]
        public ActionResult CreateAccount()
        {
            return View("CreateAccount");
        }

        //POST:/account/create-account
        [ActionName("create-account")]
        [HttpPost]
        public ActionResult CreateAccount(UserVM model)
        {
            //Check model state
            if(!ModelState.IsValid)
            {
                return View("CreateAccount", model);
            }

            //Check if password match
            if(!model.Password.Equals(model.ConfirmPassword))
            {
                ModelState.AddModelError("", "Passwords do not match.");
                return View("CreateAccount", model);
            }

            using (Db db = new Db())
            {
                //Make sure username is unique
                if(db.Users.Any(x => x.UserName.Equals(model.UserName)))
                {
                    ModelState.AddModelError("", "Username " + model.UserName + " is taken.");
                    model.UserName = "";
                    return View("CreateAccount", model);
                }

                //Create userDTO
                UserDTO userDTO = new UserDTO()
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    EmailAddress = model.EmailAddress,
                    UserName = model.UserName,
                    Password = model.Password
               
                };

                //Add the DTO
                db.Users.Add(userDTO);

                //Save
                db.SaveChanges();

                //Add to userRoleDTO
                int id = userDTO.Id;

                UserRoleDTO userRoleDTO = new UserRoleDTO()
                {
                    UserId = id,
                    RoleId = 2

                };

                db.UserRoles.Add(userRoleDTO);
                db.SaveChanges();
            }

            //Create a TempData message
            TempData["SM"] = "You are now registered and can login.";

            return Redirect("~/account/login");
           

        }

        //GET:/account/logout
        [Authorize]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return Redirect("~/account/login");
        }

        [Authorize]
        public ActionResult UserNavPartial()
        {
            //Get username
            string username = User.Identity.Name;

            //Declare model
            UserNavPartialVM model;

            using (Db db = new Db())
            {
                //Get the user
                UserDTO dto = db.Users.FirstOrDefault(x => x.UserName == username);

                //Build the model
                model = new UserNavPartialVM()
                {
                    FirstName = dto.FirstName,
                    LastName = dto.LastName
                };
            }
            
            //Return partial view with model
            return PartialView(model);
        }

        [HttpGet]
        [ActionName("user-profile")]
        [Authorize]
        public ActionResult UserProfile()
        {
            //Get username
            string username = User.Identity.Name;

            //Declare model
            UserProfileVM model;

            using (Db db = new Db())
            {
                //Get User
                UserDTO dto = db.Users.FirstOrDefault(x => x.UserName == username);

                //Build model
                model = new UserProfileVM(dto);
            }

            return View("UserProfile", model);

            
        }

        [HttpPost]
        [ActionName("user-profile")]
        [Authorize]
        public ActionResult UserProfile(UserProfileVM model)
        {
            //Check model state
            if(!ModelState.IsValid)
            {
                return View("UserProfiel", model);
            }

            //Check if passwords match if need be
            if(!string.IsNullOrWhiteSpace(model.Password))
            {
                if(!model.Password.Equals(model.ConfirmPassword))
                {
                    ModelState.AddModelError("", "Passwords do not match.");
                    return View("UserProfile", model);
                }
            }

            using (Db db = new Db())
            {
                //Get username
                string username = User.Identity.Name;

                //Make sure username is unique
                if (db.Users.Where(x => x.Id != model.Id).Any(x => x.UserName == username))
                {
                    ModelState.AddModelError("", "Username" + model.UserName + " already exists.");
                    model.UserName = "";
                    return View("UserProfile", model);
                }

                //Edit dto
                UserDTO dto = db.Users.Find(model.Id);

                dto.FirstName = model.FirstName;
                dto.LastName = model.LastName;
                dto.EmailAddress = model.EmailAddress;
                dto.UserName = model.UserName;

                if(!string.IsNullOrWhiteSpace(model.Password))
                {
                    dto.Password = model.Password;
                }

                //Save
                db.SaveChanges();
            }
            
            //Set TempData message
            TempData["SM"] = "You have edited your profile!";

            //Redirect
            return Redirect("~/account/user-profile");
        }

        //GET: /account/orders
        [Authorize(Roles = "User")]
        public ActionResult Orders()
        {
            //Init list of OrdersForUserVM
            List<OrdersForUserVM> ordersForUser = new List<OrdersForUserVM>();

            using (Db db = new Db())
            {
                //Get User id
                UserDTO user = db.Users.Where(x => x.UserName == User.Identity.Name).FirstOrDefault();
                int userId = user.Id;

                //Init List of orderVM
                List<OrderVM> orders = db.Orders.Where(x => x.UserId == userId).ToArray().Select(x => new OrderVM(x)).ToList();

                //Loop through list orders
                foreach (var order in orders)
                {
                    //Init products dict
                    Dictionary<string, int> productsAndQty = new Dictionary<string, int>();

                    //Declare total
                    decimal total = 0m;

                    //Init list of OrderDetailsDTO
                    List<OrderDetailsDTO> orderDetailsDTO = db.OrderDetails.Where(x => x.OrderId == order.OrderId).ToList();

                    //Loop through list of OrderDetailsDTO
                    foreach(var orderDetails in orderDetailsDTO)
                    {
                        //Get product
                        ProductDTO product = db.Products.Where(x => x.Id == orderDetails.ProductId).FirstOrDefault();

                        //Get product price
                        decimal price = product.Price;

                        //Get product name
                        string productname = product.Name;

                        //Add to product dict
                        productsAndQty.Add(productname, orderDetails.Quantity);

                        //Get total
                        total += orderDetails.Quantity * price;
                    }

                    //Add to OrderForUserVM list
                    ordersForUser.Add(new OrdersForUserVM()
                    {
                        OrderNumber = order.OrderId,
                        Total = total,
                        ProductsAndQty = productsAndQty,
                        CreatedAt = order.CreatedAt
                    });
                }
            }
            return View(ordersForUser);
        }
    }
}