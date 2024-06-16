using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyShoping.Entities.Models;
using MyShoping.Entities.Repository;
using MyShopping.Utility;

namespace MyShoping.WebApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.AdminRole)]
    public class CategoryController : Controller
    {
        private IUnitOfWork _unitOfWork;

        public CategoryController(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;
        public IActionResult Index()
        {
            List<Category> categories = _unitOfWork.CategoryRepos.GetAll().ToList(); //_dbContext.Categories.ToList(); 
            return View(categories);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Category category)
        {
            if (ModelState.IsValid)
            {
                //_dbContext.Categories.Add(category);
                _unitOfWork.CategoryRepos.Add(category);

                //_dbContext.SaveChanges();
                _unitOfWork.Complete();
                TempData["create"] = "Data Has Created succesfully";
                return RedirectToAction("Index");
            }
            return View();
        }

        [HttpGet]
        public IActionResult Edit(int? id)
        {
            if (id == null | id == 0)
            {
                NotFound();
            }
            var categoryInDb = _unitOfWork.CategoryRepos.GetFirstOrDefault(x => x.Id == id); //_dbContext.Categories.Find(id);
            return View(categoryInDb);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Category category)
        {
            if (ModelState.IsValid)
            {
                //_dbContext.Categories.Update(category);
                _unitOfWork.CategoryRepos.Update(category);

                //_dbContext.SaveChanges();
                _unitOfWork.Complete();

                TempData["update"] = "Data Has Updated succesfully";
                return RedirectToAction("Index");
            }
            return View(category);
        }

        [HttpGet]
        public IActionResult Delete(int? id)
        {
            if (id == null | id == 0)
            {
                NotFound();
            }
            var categoryInDb = _unitOfWork.CategoryRepos.GetFirstOrDefault(x => x.Id == id); //_dbContext.Categories.Find(id);
            return View(categoryInDb);
        }

        [HttpPost]
        public IActionResult DeleteCategory(int? id)
        {
            var categoryInDb = _unitOfWork.CategoryRepos.GetFirstOrDefault(x => x.Id == id); //_dbContext.Categories.Find(id);
            if (categoryInDb == null)
            {
                NotFound();
            }
            //_dbContext.Categories.Remove(categoryInDb);
            _unitOfWork.CategoryRepos.Remove(categoryInDb);

            //_dbContext.SaveChanges();
            _unitOfWork.Complete();

            TempData["delete"] = "Data Has Deleted succesfully";
            return RedirectToAction("Index");
        }
    }
}
