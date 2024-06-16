using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyShoping.DataAccess;
using MyShoping.Entities.Models;
using MyShoping.Entities.Repository;
using MyShopping.Utility;
using System.Security.Claims;
using X.PagedList;

namespace MyShoping.WebApp.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public HomeController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        // GET: /Customer/
        public IActionResult Index(int? page)
        {
            var PageNumber = page ?? 1;
            int PageSize = 8;

            var products = _unitOfWork.ProductRepos.GetAll().ToPagedList(PageNumber, PageSize);
            return View(products);
        }

        // GET: /Customer/Details/5
        public IActionResult Details(int ProductId)
        {
            ShoppingCart cart = new ShoppingCart()
            {
                ProductId = ProductId,
                Product = _unitOfWork.ProductRepos.GetFirstOrDefault(det => det.Id == ProductId, "Category"),
                Count = 1
            };
            return View(cart);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public IActionResult Details(ShoppingCart shoppingCart)
        {
            var claimIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            shoppingCart.ApplicationUserId = userId;

            ShoppingCart cartDb = _unitOfWork.ShoppingCart.GetFirstOrDefault(u => u.ApplicationUserId == userId && u.ProductId == shoppingCart.ProductId);

            if(cartDb == null)
            {
                _unitOfWork.ShoppingCart.Add(shoppingCart);
                _unitOfWork.Complete();
                HttpContext.Session.SetInt32(SD.SessionKey,
                    _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == userId).ToList().Count());
            }
            else
            {
                _unitOfWork.ShoppingCart.IncreaseCount(cartDb, shoppingCart.Count);
                _unitOfWork.Complete();
            }
            _unitOfWork.Complete();

            return RedirectToAction("Index");
        }        
    }
}
