using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UdemySiparis.Data.Repository.IRepository;
using UdemySiparis.Models;

namespace UdemySiparis.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class OrderController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public OrderController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            IEnumerable<OrderProduct> orderProduct;

            var claimsIdentity = (ClaimsIdentity)User.Identity;                             // once kullanıcı buluyoruz sonra da sepeti ( alta olacak) 
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);               // claim bilgileri cektik 

            orderProduct = _unitOfWork.OrderProduct.GetAll(u=>u.AppUserId == claim.Value);  // claim ile kullanıcın id aldık 

            return View(orderProduct);
        }
        public IActionResult CancelOrder(int id)
        {
            var order = _unitOfWork.OrderProduct.GetFirstOrDefault(x => x.Id == id);

            if (order.OrderStatus=="Ordered")

            //_unitOfWork.OrderProduct.Remove(order); // hemen silmek istiyorsanız
            order.OrderStatus = "Cancel";
            _unitOfWork.OrderProduct.Update(order);
            _unitOfWork.Save();
            return RedirectToAction("Index");
        }
    }
}
