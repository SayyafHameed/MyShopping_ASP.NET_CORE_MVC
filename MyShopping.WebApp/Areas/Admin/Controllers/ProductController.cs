using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MyShoping.Entities.Models;
using MyShoping.Entities.Repository;
using MyShoping.Entities.ViewModel;
using MyShopping.Utility;

namespace MyShoping.WebApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.AdminRole)]
    public class ProductController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProductController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
        }
        public IActionResult Index()
        {
            List<Product> objProductList = _unitOfWork.ProductRepos.GetAll(includeWords: "Category").ToList();

            return View(objProductList);
        }

       // [HttpGet]
        public IActionResult GetData()
        {
            List<Product> products = _unitOfWork.ProductRepos.GetAll(includeWords: "Category").ToList(); //Includeword: "Category"
            return Json(new { data = products });
        }

        [HttpGet]
        public IActionResult Create()
        {
            ProductVM productVM = new ProductVM()
            {
                Product = new Product(),
                CategoryList = _unitOfWork.ProductRepos.GetAll().Select(x => new SelectListItem
                {
                    Text = x.Name,
                    Value = x.Id.ToString()
                })
            };
            return View(productVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(ProductVM productVM, IFormFile file)
        {
            if (ModelState.IsValid)
            {
                string RootPath = _webHostEnvironment.WebRootPath; // => wwwroot\Images\Products\
                if (file != null)
                {
                    string fileName = Guid.NewGuid().ToString();
                    var upload = Path.Combine(RootPath, @"Images\Products"); 
                    var extension = Path.GetExtension(file.FileName);

                    using (var fileStream = new FileStream(Path.Combine(upload, fileName + extension), FileMode.Create))
                    {
                        file.CopyTo(fileStream);
                    }

                    productVM.Product.Image = @"Images\Products\" + fileName + extension;
                }
                //_dbContext.Categories.Add(Product);
                _unitOfWork.ProductRepos.Add(productVM.Product);

                //_dbContext.SaveChanges();
                _unitOfWork.Complete();
                TempData["create"] = "Data Has Created succesfully";
                return RedirectToAction("Index");
            }
            return View(productVM.Product);
        }

        [HttpGet]
        public IActionResult Edit(int? id)
        {
            if (id == null | id == 0)
            {
                NotFound();
            }
            ProductVM productVM = new ProductVM()
            {
                Product = _unitOfWork.ProductRepos.GetFirstOrDefault(x => x.Id == id),
                CategoryList = _unitOfWork.CategoryRepos.GetAll().Select(x => new SelectListItem
                {
                    Text = x.Name,
                    Value = x.Id.ToString()
                })
            };
            return View(productVM);
            //var ProductInDb = _unitOfWork.ProductRepos.GetFirstOrDefault(x => x.Id == id); //_dbContext.Categories.Find(id);
            //return View(ProductInDb);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(ProductVM productVM, IFormFile? file)
        {
            if (ModelState.IsValid)
            {
                string RootPath = _webHostEnvironment.WebRootPath; // => wwwroot\Images\Products\
                if (file != null)
                {
                    string fileName = Guid.NewGuid().ToString();
                    var upload = Path.Combine(RootPath, @"Images\Products");
                    var extension = Path.GetExtension(file.FileName);

                    if (productVM.Product.Image != null)
                    {
                        var oldImage = Path.Combine(RootPath, productVM.Product.Image.TrimStart('\\'));
                        if (System.IO.File.Exists(oldImage))
                        {
                            System.IO.File.Delete(oldImage);
                        }
                    }

                    using (var fileStream = new FileStream(Path.Combine(upload, fileName + extension), FileMode.Create))
                    {
                        file.CopyTo(fileStream);
                    }

                    productVM.Product.Image = @"Images\Products\" + fileName + extension;
                }

                //_dbContext.Categories.Update(Product);
                _unitOfWork.ProductRepos.Update(productVM.Product);

                //_dbContext.SaveChanges();
                _unitOfWork.Complete();

                TempData["update"] = "Data Has Updated succesfully";
                return RedirectToAction("Index");
            }
            return View(productVM.Product);
        }

        [HttpDelete]
        public IActionResult DeleteProduct(int? id)
        {
            var ProductInDb = _unitOfWork.ProductRepos.GetFirstOrDefault(x => x.Id == id); //_dbContext.Categories.Find(id);
            if (ProductInDb == null)
            {
                return Json(new { success = false, message = "Error while Deleting" });
            }
            //_dbContext.Categories.Remove(ProductInDb);
            _unitOfWork.ProductRepos.Remove(ProductInDb);

            var oldImage = Path.Combine(_webHostEnvironment.WebRootPath, ProductInDb.Image.TrimStart('\\'));
            if (System.IO.File.Exists(oldImage))
            {
                System.IO.File.Delete(oldImage);
            }

            //_dbContext.SaveChanges();
            _unitOfWork.Complete();

            return Json(new { success = true, message = "File Has been Deleted" });
        }
    }
}
