using Microsoft.AspNetCore.Mvc;
using UdemySiparis.Data.Repository.IRepository;
using UdemySiparis.Models;
using UdemySiparis.Models.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;

namespace UdemySiparis.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ProductController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _hostEnvironment;
        public ProductController(IUnitOfWork unitOfWork, IWebHostEnvironment hostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _hostEnvironment = hostEnvironment;
        }

        public IActionResult Index()
        {
            var productList = _unitOfWork.Product.GetAll();
            return View(productList);
        }
        //public IActionResult Create()
        //{
        //    return View();
        //}
        //[HttpPost]
        //public IActionResult Create(Product product)
        //{
        //    _unitOfWork.Product.Add(product);
        //    _unitOfWork.Save();
        //    return RedirectToAction("Index");
        //}
        public IActionResult Crup(int? id)
        {
            ProductVM productVM = new()
            {
                Product = new(),
                CategoryList = _unitOfWork.Category.GetAll().Select(l => new SelectListItem
                {
                    Text = l.Name,
                    Value = l.Id.ToString()
                })
            };
            if (id == null || id <= 0)
            {
                return View(productVM);
            }
            productVM.Product = _unitOfWork.Product.GetFirstOrDefault(x => x.Id == id); // "x"bize gonderilen id-iye eşittir, producti bul ve burda sakla
           
            if (productVM.Product == null)
            {
                return View(productVM);
            }
            return View(productVM);
        }
        [HttpPost]
        public IActionResult Crup(ProductVM productVM,IFormFile file)
        {
            string wwwRootPath = _hostEnvironment.WebRootPath;
            if (file != null)
            {
                string fileName = Guid.NewGuid().ToString();     // dosya
                var uploadRoot = Path.Combine(wwwRootPath,@"img\products");    
                var extension = Path.GetExtension(file.FileName);   // dosya uzantısı

                // urunun resmini gucelliyorsak eskisini guncellemeyecegız, productın resmi veri tabanda kayıtlı ise silmem lazım asagıda yazacagız 
                if(productVM.Product.Picture != null)
                {
                    var oldPicPath = Path.Combine(wwwRootPath, productVM.Product.Picture);
                    if(System.IO.File.Exists(oldPicPath))
                    {
                        System.IO.File.Delete(oldPicPath);
                    }
                }
                using (var fileStream = new FileStream(Path.Combine(uploadRoot,fileName + extension),
                    FileMode.Create))
                {
                    file.CopyTo(fileStream);
                }
                productVM.Product.Picture = @"\img\products\" + fileName + extension;  // kaydediyoruz

            }
              

            if (productVM.Product.Id <= 0)
            {
                _unitOfWork.Product.Add(productVM.Product);
            }
            else
            {
                _unitOfWork.Product.Update(productVM.Product);
            }
                //_unitOfWork.Product.Update(product);   // product uzerinden update metodu cagır . burdaki metodun icerisinde product bilgisini gonder ve bilgileri yazdıktan sonra unit of worok uzerinden
                                                       // /bilgileri veri tabanında kaydet, ve tamalandıysa geriye indexe don
                _unitOfWork.Save();
                return RedirectToAction("Index");
            
        }
        public IActionResult Delete(int? id)
        {
            if (id == null || id <= 0)
            {
                return NotFound();
            }
            var product = _unitOfWork.Product.GetFirstOrDefault(x => x.Id == id); // producti bulduk 
            _unitOfWork.Product.Remove(product);
            _unitOfWork.Save();

            return RedirectToAction("Index");
        }

        
        
    }
}
