using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyShoping.Entities.Models;
using MyShoping.Entities.Repository;
using MyShoping.Entities.ViewModel;
using MyShopping.Utility;
using Stripe.Checkout;

//using Stripe.BillingPortal;
using System.Security.Claims;
using static System.Net.WebRequestMethods;

namespace MyShoping.WebApp.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CartController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public ShoppingCartVM ShoppingCartVM { get; set; }

        public CartController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            var claimIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimIdentity.FindFirst(ClaimTypes.NameIdentifier);

            ShoppingCartVM = new ShoppingCartVM()
            {
                shoppingCarts = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == userId.Value, "Product")
            };

            foreach (var item in ShoppingCartVM.shoppingCarts)
            {
                ShoppingCartVM.TotalCarts += item.Count * item.Product.Price;
            }
            return View(ShoppingCartVM);
        }

        [HttpGet]
        public IActionResult Summary()
        {
            var claimIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimIdentity.FindFirst(ClaimTypes.NameIdentifier);

            ShoppingCartVM = new ShoppingCartVM()
            {
                shoppingCarts = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == userId.Value, includeWords: "Product"),
                OrderHeader = new()
            };

            ShoppingCartVM.OrderHeader.ApplicationUser = _unitOfWork.AppUserRepos.GetFirstOrDefault(x => x.Id == userId.Value);

            ShoppingCartVM.OrderHeader.Name = ShoppingCartVM.OrderHeader.ApplicationUser.Name;
            ShoppingCartVM.OrderHeader.Address = ShoppingCartVM.OrderHeader.ApplicationUser.Address;
            ShoppingCartVM.OrderHeader.City = ShoppingCartVM.OrderHeader.ApplicationUser.City;
            ShoppingCartVM.OrderHeader.PhoneNumber = ShoppingCartVM.OrderHeader.ApplicationUser.PhoneNumber;


            foreach (var item in ShoppingCartVM.shoppingCarts)
            {
                ShoppingCartVM.TotalCarts += item.Count * item.Product.Price;
            }
            return View(ShoppingCartVM);
        }

        [HttpPost]
        [ActionName("Summary")]
        [ValidateAntiForgeryToken]
        public IActionResult SummaryPOST(ShoppingCartVM shoppingCartVM)
        {
            var claimsIdentity = User.Identity as ClaimsIdentity;
            if (claimsIdentity == null)
            {
                return BadRequest("User is not authenticated");
            }
            var userIdClaim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                // Handle the null case
                return BadRequest("User ID claim not found");
            }
            var userId = userIdClaim.Value;

            if (shoppingCartVM == null)
            {
                return BadRequest("ShoppingCartVM is null");
            }

            shoppingCartVM.shoppingCarts = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == userId, includeWords: "Product").ToList();

            if (shoppingCartVM.OrderHeader == null)
            {
                shoppingCartVM.OrderHeader = new OrderHeader();
            }

            shoppingCartVM.OrderHeader.OrderStatus = SD.Pending;
            shoppingCartVM.OrderHeader.PaymentStatus = SD.Pending;
            shoppingCartVM.OrderHeader.OrderDate = DateTime.Now;
            shoppingCartVM.OrderHeader.ApplicationUserId = userId;

            if (shoppingCartVM.shoppingCarts == null)
            {
                shoppingCartVM.shoppingCarts = new List<ShoppingCart>();
            }

            if (shoppingCartVM != null && shoppingCartVM.shoppingCarts != null)
            {
                foreach (var item in shoppingCartVM.shoppingCarts)
                {
                    if (item.Product == null)
                    {
                        return BadRequest("Product is null in shopping cart item");
                    }
                    shoppingCartVM.TotalCarts += item.Count * item.Product.Price;
                }
            }

            //foreach (ShoppingCart item in ShoppingCartVM.shoppingCarts)
            //{
            //    ShoppingCartVM.TotalCarts += item.Count * item.Product.Price;
            //}
            _unitOfWork.OrderHeaderRepos.Add(shoppingCartVM.OrderHeader);
            _unitOfWork.Complete();

            foreach (var item in shoppingCartVM.shoppingCarts)
            {
                OrderDetail orderDetail = new()
                {
                    ProductId = item.ProductId,
                    OrderHeaderId = shoppingCartVM.OrderHeader.Id,
                    Price = item.Product.Price,
                    Count = item.Count
                };
                _unitOfWork.OrderDetailRepos.Add(orderDetail);
                _unitOfWork.Complete();
            }
            var domain = "https://localhost:7108/";
            var options = new SessionCreateOptions
            {
                LineItems = new List<SessionLineItemOptions>(),

                Mode = "payment",
                SuccessUrl = domain + $"customer/cart/OrderConfirmation?id={shoppingCartVM.OrderHeader.Id}",
                CancelUrl = domain + $"customer/cart/index",
            };

            foreach (var item in shoppingCartVM.shoppingCarts)
            {
                var sessionLineItemOption = new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)(item.Product.Price * 100),
                        Currency = "usd",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = item.Product.Name
                        }
                    },
                    Quantity = item.Count
                };
                options.LineItems.Add(sessionLineItemOption);
            }

            var service = new SessionService();
            Session session = service.Create(options);
            shoppingCartVM.OrderHeader.SessionId = session.Id;

            _unitOfWork.Complete();

            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);
        }

        public IActionResult OrderConfirmation(int id)
        {
            OrderHeader orderHeader = _unitOfWork.OrderHeaderRepos.GetFirstOrDefault(u => u.Id == id);

            var service = new SessionService();
            Session session = service.Get(orderHeader.SessionId);
            //orderHeader.PaymentIntentId = session.PaymentIntentId;

            if (session.PaymentStatus.ToLower() == "paid")
            {
                _unitOfWork.OrderHeaderRepos.UpdateOrderStatus(id, SD.Approved, SD.Approved);
                orderHeader.PaymentIntentId = session.PaymentIntentId;
                _unitOfWork.Complete();
            }
            //HttpContext.Session.Clear();

            List<ShoppingCart> shoppingCarts = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == orderHeader.ApplicationUserId).ToList();

            _unitOfWork.ShoppingCart.RemoveRange(shoppingCarts);
            _unitOfWork.Complete();

            return View(id);
        }

        public IActionResult Plus(int cartId)
        {
            var shoppingCart = _unitOfWork.ShoppingCart.GetFirstOrDefault(u => u.Id == cartId);
            _unitOfWork.ShoppingCart.IncreaseCount(shoppingCart, 1);
            _unitOfWork.Complete();
            return RedirectToAction("Index");
        }

        public IActionResult Minus(int cartId)
        {
            var shoppingCart = _unitOfWork.ShoppingCart.GetFirstOrDefault(u => u.Id == cartId);
            if (shoppingCart.Count <= 1)
            {
                //remove that from cart

                _unitOfWork.ShoppingCart.Remove(shoppingCart);
                var count = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == shoppingCart.ApplicationUserId).ToList().Count() - 1;
                HttpContext.Session.SetInt32(SD.SessionKey, count);
            }
            else
            {
                _unitOfWork.ShoppingCart.DecreaseCount(shoppingCart, 1);
                _unitOfWork.Complete();
            }

            _unitOfWork.Complete();
            return RedirectToAction("Index");
        }

        public IActionResult Remove(int cartId)
        {
            var shoppingCart = _unitOfWork.ShoppingCart.GetFirstOrDefault(u => u.Id == cartId);

            _unitOfWork.ShoppingCart.Remove(shoppingCart);

            //HttpContext.Session.SetInt32(SD.CustomerRole, _unitOfWork.ShoppingCart
            //  .GetAll(u => u.ApplicationUserId == shoppingCart.ApplicationUserId).Count() - 1);
            _unitOfWork.Complete();
            var count = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == shoppingCart.ApplicationUserId).ToList().Count();
            HttpContext.Session.SetInt32(SD.SessionKey, count);
            return RedirectToAction("Index");
        }
    }
}
