using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WebApplication4.Models;
using WebApplication4.ViewModels;

namespace WebApplication4.Controllers
{
    //[Route("Home")] //[Route("[controller]")]  czyli nazwa klasy
    //[Route("[controller]/[action]")]
    [AllowAnonymous]
    public class HomeController : Controller
    {
        private readonly IEmployeeRepository _employeeRepository;//zeby nie zmieniac tej wartrosci poza konstruktorem
        private readonly IHostingEnvironment hostingEnvironment;
        private readonly ILogger logger;

        public HomeController(IEmployeeRepository employeeRepository, IHostingEnvironment hostingEnvironment, ILogger<HomeController> logger)
        {
            _employeeRepository = employeeRepository;
            this.hostingEnvironment = hostingEnvironment;
            this.logger = logger;
        }
        //[Route("")]
        // [Route("Home/Index")] majac home na gorze mozna wywalic
        //[Route("Index")]  //[Route("[action]")]  czyli nazwa funkcji
        //[Route("~/Home")] tez replace home
        //[Route("~/")]//replaceuje home
        public ViewResult Index()
        {
            var model = _employeeRepository.GetAllEmployees();
            return View(model);
            // return View("~/Views/Home/Index.cshtml",model); //absolute route
        }

        //[Route("{id?}")]
        // [Route("Details/{id?}")] //[Route("[action]/{id?}")] czyli nazwa Details
        public ViewResult Details(int? id)//?moze być null
       //Form<-Route<-Query (priority skad value)
        {
            //throw new Exception("Some error");

            //logger.LogTrace("Trace Log");
            //logger.LogDebug("Debug Log");
            //logger.LogInformation("Information Log");
            //logger.LogWarning("Warning Log");
            //logger.LogError("Error Log");
            //logger.LogCritical("Critical Log");



            Employee employee = _employeeRepository.GetEmployee(id.Value);
            if (employee == null)
            {
                Response.StatusCode = 404;
                return View("EmployeeNotFound", id);
            }

            HomeDetailsViewModel homeDetailsViewModel = new HomeDetailsViewModel()
            {
                //Employee = _employeeRepository.GetEmployee(id ?? 1), //if not null use value if null use 1
                Employee= employee,
                PageTitle = "Employee Details"
            };
            return View(homeDetailsViewModel);

            //   Employee model = _employeeeRepository.GetEmployee(1);

            //ViewData Loosly Typed, misspell i blad
            /*
            ViewData["Employee"] = model;
            ViewData["PageTitle"] = "Employee Details";
            */

            //ViewBag dynamiczne, again misspell i blad
            /*
            ViewBag.Employee = model;
            ViewBag.PageTitle = "Employee Details";
            */

            //@model WebApplication4.Models.Employee strong typed i widac bledy przed kompilacja
            /*
            ViewBag.PageTitle = "Employee Details";
            return View(model);
            */

            // return View("~/MyViews/Test.cshtml",model); 
            //bez stringa szuka pod ta samo nazwa, mozna podac scieszke cala z rozszezeniem
        }
        [HttpGet]
        [Authorize] //można dać globalnie wtedy można dla każdej akcji dać [AllowAnonymous]
        public ViewResult Create()
        {
            return View();
        }
        [HttpPost]
        [Authorize]
        public IActionResult Create(EmployeeCreateViewModel model)
        //public IActionResult Create(Employee employee)
        //RedirectToActionResult wczesniej
        {
            if (ModelState.IsValid)
            {
                string uniqueFileName = ProcessUploadedFile(model);
                //  Employee newEmployee = _employeeeRepository.Add(employee);
                Employee newEmployee = new Employee
                {
                    Name = model.Name,
                    Email = model.Email,
                    Department = model.Department,
                    PhotoPath = uniqueFileName
                };
                _employeeRepository.Add(newEmployee);
                return RedirectToAction("details", new { id = newEmployee.Id });
            }
            return View();
        }
        [HttpGet]
        [Authorize]
        public ViewResult Edit(int id)
        {
            Employee employee = _employeeRepository.GetEmployee(id);
            EmployeeEditViewModel employeeEditViewModel = new EmployeeEditViewModel
            {
                Id = employee.Id,
                Name = employee.Name,
                Email = employee.Email,
                Department = employee.Department,
                ExistingPhotoPath = employee.PhotoPath
            };
            return View(employeeEditViewModel);
        }


        // Through model binding, the action method parameter
        // EmployeeEditViewModel receives the posted edit form data
        [HttpPost]
        [Authorize]
        public IActionResult Edit(EmployeeEditViewModel model)
        {
            // Check if the provided data is valid, if not rerender the edit view
            // so the user can correct and resubmit the edit form
            if (ModelState.IsValid)
            {
                // Retrieve the employee being edited from the database
                Employee employee = _employeeRepository.GetEmployee(model.Id);
                // Update the employee object with the data in the model object
                employee.Name = model.Name;
                employee.Email = model.Email;
                employee.Department = model.Department;

                // If the user wants to change the photo, a new photo will be
                // uploaded and the Photo property on the model object receives
                // the uploaded photo. If the Photo property is null, user did
                // not upload a new photo and keeps his existing photo
                if (model.Photos != null)
                {
                    // If a new photo is uploaded, the existing photo must be
                    // deleted. So check if there is an existing photo and delete
                    if (model.ExistingPhotoPath != null)
                    {
                        string filePath = Path.Combine(hostingEnvironment.WebRootPath,"img", model.ExistingPhotoPath);
                        System.IO.File.Delete(filePath);
                    }
                    // Save the new photo in wwwroot/images folder and update
                    // PhotoPath property of the employee object which will be
                    // eventually saved in the database
                    employee.PhotoPath = ProcessUploadedFile(model);
                }

                // Call update method on the repository service passing it the
                // employee object to update the data in the database table
                Employee updatedEmployee = _employeeRepository.Update(employee);

                return RedirectToAction("index");
            }

            return View(model);
        }

        private string ProcessUploadedFile(EmployeeCreateViewModel model)
        {
            string uniqueFileName = null;

            if (model.Photos != null)
            {
                string uploadsFolder = Path.Combine(hostingEnvironment.WebRootPath, "img");
                uniqueFileName = Guid.NewGuid().ToString() + "_" + model.Photos.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    model.Photos.CopyTo(fileStream);
                }
            }

            return uniqueFileName;
        }


        #region Unused
        public JsonResult IndexJson()
        {
            return Json(new { id = 1, name = "Miczo" });
        }

        public string IndexSingle()
        {
            return _employeeRepository.GetEmployee(1).Name;
        }
        public JsonResult DetailsJson()
        {
            Employee model = _employeeRepository.GetEmployee(1);
            return Json(model);
        }

        public ObjectResult DetailsObject()
        {
            Employee model = _employeeRepository.GetEmployee(1);
            return new ObjectResult(model);             //w kodzie jest na xml
        }
        #endregion


    }
}
