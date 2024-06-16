using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyShoping.Entities.Models;
using MyShoping.Entities.Repository;
using MyShoping.Entities.ViewModel;
using MyShopping.Utility;
using Stripe;
using Stripe.Climate;

namespace MyShoping.WebApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.AdminRole)]
    public class OrderController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        [BindProperty]
        public OrderVM OrderVM { get; set; }

        public OrderController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            return View();
        }

        // [HttpGet]
        public IActionResult GetData()
        {
            IEnumerable<OrderHeader> orderHeaders;
            orderHeaders = _unitOfWork.OrderHeaderRepos.GetAll(includeWords: "ApplicationUser");
            return Json(new { data = orderHeaders });
        }

        public IActionResult Details(int orderid)
        {
            OrderVM orderVM = new OrderVM()
            {
                OrderHeader = _unitOfWork.OrderHeaderRepos.GetFirstOrDefault(u => u.Id == orderid, Includeword: "ApplicationUser"),
                OrderDetail = _unitOfWork.OrderDetailRepos.GetAll(x => x.OrderHeaderId == orderid, includeWords: "Product")
            };

            return View(orderVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateOrderDetails()
        {
            var orderFromDb = _unitOfWork.OrderHeaderRepos.GetFirstOrDefault(u => u.Id == OrderVM.OrderHeader.Id);
            orderFromDb.Name = OrderVM.OrderHeader.Name;
            orderFromDb.PhoneNumber = OrderVM.OrderHeader.PhoneNumber;
            orderFromDb.Address = OrderVM.OrderHeader.Address;
            orderFromDb.City = OrderVM.OrderHeader.City;

            if (OrderVM.OrderHeader.Carrier != null)
            {
                orderFromDb.Carrier = OrderVM.OrderHeader.Carrier;
            }

            if (OrderVM.OrderHeader.TrackingNumber != null)
            {
                orderFromDb.TrackingNumber = OrderVM.OrderHeader.TrackingNumber;
            }

            _unitOfWork.OrderHeaderRepos.Update(orderFromDb);
            _unitOfWork.Complete();
            TempData["update"] = "Data Has Updated succesfully";
            return RedirectToAction("Details", "Order", new { orderid = orderFromDb.Id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult StartProcess()
        {
            _unitOfWork.OrderHeaderRepos.UpdateOrderStatus(OrderVM.OrderHeader.Id, SD.Processing, null);
            _unitOfWork.Complete();

            TempData["update"] = "Order Status Has Updated succesfully";
            return RedirectToAction("Details", "Order", new { orderid = OrderVM.OrderHeader.Id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult StartShip()
        {
            var orderFromDb = _unitOfWork.OrderHeaderRepos.GetFirstOrDefault(u => u.Id == OrderVM.OrderHeader.Id);
            orderFromDb.TrackingNumber = OrderVM.OrderHeader.TrackingNumber;
            orderFromDb.Carrier = OrderVM.OrderHeader.Carrier;
            orderFromDb.OrderStatus = SD.Shipped;
            orderFromDb.ShippingDate = DateTime.Now;

            _unitOfWork.OrderHeaderRepos.Update(orderFromDb);
            _unitOfWork.Complete();

            TempData["update"] = "Order Has Shipped succesfully";
            return RedirectToAction("Details", "Order", new { orderid = OrderVM.OrderHeader.Id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CancelOrder()
        {
            var orderFromDb = _unitOfWork.OrderHeaderRepos.GetFirstOrDefault(u => u.Id == OrderVM.OrderHeader.Id);
            if (orderFromDb.PaymentStatus == SD.Approved)
            {
                var option = new RefundCreateOptions
                {
                    Reason = RefundReasons.RequestedByCustomer,
                    PaymentIntent = orderFromDb.PaymentIntentId
                };

                var service = new RefundService();
                Refund refund = service.Create(option);

                _unitOfWork.OrderHeaderRepos.UpdateOrderStatus(orderFromDb.Id, SD.Cancelled, SD.Refunded);
            }
            else
            {
                _unitOfWork.OrderHeaderRepos.UpdateOrderStatus(orderFromDb.Id, SD.Cancelled, SD.Cancelled);
            }
            _unitOfWork.Complete();

            TempData["update"] = "Order Has Cancelled succesfully";
            return RedirectToAction("Details", "Order", new { orderid = OrderVM.OrderHeader.Id });
        }
    }
}

