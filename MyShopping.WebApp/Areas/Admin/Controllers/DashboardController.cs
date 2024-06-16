using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyShoping.Entities.Repository;
using MyShopping.Utility;

namespace MyShoping.WebApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.AdminRole)]
    public class DashboardController : Controller
    {
        private IUnitOfWork _unitOfWork;

        public DashboardController(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

        public IActionResult Index()
        {
            ViewBag.Orders = _unitOfWork.OrderHeaderRepos.GetAll().Count();
            ViewBag.Approved = _unitOfWork.OrderHeaderRepos.GetAll(x => x.OrderStatus == SD.Approved).Count();
            ViewBag.Users = _unitOfWork.AppUserRepos.GetAll().Count();
            ViewBag.Products = _unitOfWork.ProductRepos.GetAll().Count();
            return View();
        }
    }
}
