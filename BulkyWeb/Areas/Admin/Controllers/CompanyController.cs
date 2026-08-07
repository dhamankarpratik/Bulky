using Bulky.DataAccess.Data;
using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Bulky.Models.ViewModels;
using Bulky.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace BulkyWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class CompanyController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public CompanyController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            List<Company> objCompanyList = _unitOfWork.Company.GetAll().ToList();
            
            return View(objCompanyList);
        }
        public IActionResult UpSert(int? id)
        {
            if(id == null || id == 0)
            {
                //Create
                return View(new Company());
            }
            else
            {
                //Update
                Company CompanyObj = _unitOfWork.Company.Get(u => u.Id == id);
                return View(CompanyObj);
            }
        }

        [HttpPost]
        public IActionResult UpSert(Company CompanyObj, IFormFile? file)
        {
            if (ModelState.IsValid)
            {
                if(CompanyObj.Id == 0)
                {
                    _unitOfWork.Company.Add(CompanyObj);
                }
                else
                {
                    _unitOfWork.Company.Update(CompanyObj);
                }
                _unitOfWork.Save();
                TempData["success"] = "Company created successfully.";
                return RedirectToAction("Index");
            }
            else
            {
                return View(CompanyObj);
            }
        }



        #region API CALLS
        [HttpGet]
        public IActionResult GetAll(int? id) 
        {
            List<Company> objCompanyList = _unitOfWork.Company.GetAll().ToList();
            return Json(new { data = objCompanyList });
        }

        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            var productToBeDeleted = _unitOfWork.Company.Get(u=>u.Id == id);
            if (productToBeDeleted == null)
            {
                return Json(new { Success= false, message = "Error while deleting" });
            }

            _unitOfWork.Company.Remove(productToBeDeleted);
            _unitOfWork.Save();

            return Json(new { Success = true, message = "Delete Successfully." });
        }
        #endregion

        //before Upsert We have 2 seperate Actions Create and update
        //public IActionResult Create() 
        //{
        //    IEnumerable<SelectListItem> CategoryList = _unitOfWork.Category
        //        .GetAll().Select(u => new SelectListItem
        //        {
        //            Text = u.Name,
        //            Value = u.Id.ToString()
        //        });
        //    //ViewBag.CategoryList = CategoryList;
        //    //ViewData["CategoryList"] = CategoryList;
        //    CompanyVM productVM = new()
        //    {
        //        CategoryList = CategoryList,
        //        Company = new Company()
        //    };
        //    return View(productVM);
        //}
        //[HttpPost]
        //public IActionResult Create(CompanyVM productVM)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        _unitOfWork.Company.Add(productVM.Company);
        //        _unitOfWork.Save();
        //        TempData["success"] = "Company created successfully.";
        //        return RedirectToAction("Index");
        //    }
        //    else
        //    {
        //        productVM.CategoryList = _unitOfWork.Category
        //        .GetAll().Select(u => new SelectListItem
        //        {
        //            Text = u.Name,
        //            Value = u.Id.ToString()
        //        });

        //        return View(productVM);
        //    }
        //}


        //public IActionResult Edit(int? id)
        //{
        //    if(id == null || id == 0)
        //    {
        //        return NotFound();
        //    }
        //    Company? productFromDb = _unitOfWork.Company.Get(u=> u.Id == id);//only primarykey
        //    //Company? productFromDb = _CompanyRepo.Categories.Find(id);//only primarykey
        //    //Company? productFromDb1 = _db.Categories.FirstOrDefault(u=>u.Id == id);//search on any parameters
        //    //Company? productFromDb2 = _db.Categories.Where(u=>u.Id == id).FirstOrDefault();
        //    if (productFromDb == null)
        //    {
        //        return NotFound();
        //    }
        //    return View(productFromDb);
        //}
        //[HttpPost]
        //public IActionResult Edit(Company obj)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        _unitOfWork.Company.Update(obj);
        //        _unitOfWork.Save();
        //        TempData["success"] = "Company updated successfully.";
        //        return RedirectToAction("Index");
        //    }
        //    return View();
        //}

        //public IActionResult Delete(int? id)
        //{
        //    if (id == null || id == 0)
        //    {
        //        return NotFound();
        //    }
        //    Company? productFromDb = _unitOfWork.Company.Get(u => u.Id == id);//only primarykey
        //    //Company? productFromDb1 = _db.Categories.FirstOrDefault(u=>u.Id == id);//search on any parameters
        //    //Company? productFromDb2 = _db.Categories.Where(u=>u.Id == id).FirstOrDefault();
        //    if (productFromDb == null)
        //    {
        //        return NotFound();
        //    }
        //    return View(productFromDb);
        //}
        //[HttpPost, ActionName("Delete")]
        //public IActionResult DeletePOST(int? id)
        //{
        //    Company obj = _unitOfWork.Company.Get(u => u.Id == id);
        //    if (obj == null)
        //    {
        //        return NotFound();
        //    }
        //    _unitOfWork.Company.Remove(obj);
        //    _unitOfWork.Save();
        //    TempData["success"] = "Company deleted successfully.";
        //    return RedirectToAction("Index");
        //}
    }
} 

