using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UdemySiparis.Data.Repository.IRepository;
using UdemySiparis.Models;
using UdemySiparis.Models.ViewModels;

namespace UdemySiparis.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CartController : Controller
    {
        private readonly IUnitOfWork _unitOfWOrk;
        public CartVM CartVM { get; set; }
        public CartController(IUnitOfWork unitOfWOrk)
        {
            _unitOfWOrk = unitOfWOrk;
        }

        public IActionResult Index()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity; // once kullanıcı buluyoruz sonra da sepeti ( alta olacak) 
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);  // claim bilgileri cektik 

            CartVM = new CartVM()                                                                                  // burdan asagıda sepeteki urunleri listeliyoruz
            {
                ListCart = _unitOfWOrk.Cart.GetAll(p => p.AppUserId == claim.Value, includeProperties: "Product"),   // Listcartta saklanacak bilgileri cekiyrouz, unitofwork araciı ile cart sorguluyrouz ,
                OrderProduct = new()                                                                                              // get all metodu ile yaparız, sisteme giriğ yapan kullanıcı idisine eşit olan sepeti eşit olan sepet kayırlarını getir  ,
            };
            foreach (var cart in CartVM.ListCart)
            {
                cart.Price = cart.Product.Price * cart.Count; // urunun toplam tutarı, aynı urun 2 adet mesela 
                CartVM.OrderProduct.OrderPrice += cart.Price; // sepette toplam tutar
            }

            return View(CartVM);
        }
        public IActionResult Order()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            CartVM = new CartVM()                                                                                 
            {
                ListCart = _unitOfWOrk.Cart.GetAll(p => p.AppUserId == claim.Value, includeProperties: "Product"),    
                OrderProduct = new()                                                                                        
            };
            CartVM.OrderProduct.AppUser = _unitOfWOrk.AppUser.GetFirstOrDefault(u=>u.Id == claim.Value);
            CartVM.OrderProduct.Name = CartVM.OrderProduct.AppUser.FullName;
            CartVM.OrderProduct.CellPhone = CartVM.OrderProduct.AppUser.CellPhone;
            CartVM.OrderProduct.Address = CartVM.OrderProduct.AppUser.Address;
            CartVM.OrderProduct.PostalCode = CartVM.OrderProduct.AppUser.PostalCode;

            foreach (var cart in CartVM.ListCart)
            {
                cart.Price = cart.Product.Price * cart.Count; // urunun toplam tutarı, aynı urun 2 adet mesela 
                CartVM.OrderProduct.OrderPrice += (cart.Price * cart.Count); // sepette toplam tutar
            }

            return View(CartVM);
        }

        [HttpPost]
        [ActionName("Order")]
        [ValidateAntiForgeryToken]

        public IActionResult OrderPost(CartVM cartVM)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            CartVM = new CartVM()
            {
                ListCart = _unitOfWOrk.Cart.GetAll(p => p.AppUserId == claim.Value, includeProperties: "Product"),
                OrderProduct = new()
            };
            AppUser appUser = _unitOfWOrk.AppUser.GetFirstOrDefault(u => u.Id == claim.Value);

            CartVM.OrderProduct.AppUser = appUser;
            CartVM.OrderProduct.OrderDate = System.DateTime.Now;
            CartVM.OrderProduct.AppUserId = claim.Value;

            CartVM.OrderProduct.Name = cartVM.OrderProduct.Name;
            CartVM.OrderProduct.CellPhone = cartVM.OrderProduct.CellPhone;
            CartVM.OrderProduct.Address = cartVM.OrderProduct.Address;
            CartVM.OrderProduct.PostalCode = cartVM.OrderProduct.PostalCode;
            CartVM.OrderProduct.OrderStatus = "Ordered";

            foreach (var cart in CartVM.ListCart)
            {
                cart.Price = cart.Product.Price * cart.Count; // urunun toplam tutarı, aynı urun 2 adet mesela 
                CartVM.OrderProduct.OrderPrice += (cart.Price * cart.Count); // sepette toplam tutar
            }
            _unitOfWOrk.OrderProduct.Add(CartVM.OrderProduct);  // cartvm-de doldurmus oldugum order product bilgileri kaydet
            _unitOfWOrk.Save();

            foreach (var cart in CartVM.ListCart)
            {
                OrderDetails OrderDetails = new()
                {
                    ProductId = cart.ProductId,        // her bir urun için foearc donduk, her urunun bilgileri order details içinde tututk
                                                       // ve order details kullanarak kayıt işlemi gerçeklestirebiliriz 
                    OrderProductId = CartVM.OrderProduct.Id,
                    Price = cart.Price,
                    Count = cart.Count
                };
                _unitOfWOrk.OrderDetails.Add(OrderDetails);   // girmiş olan order details bilgilerini ekle
                _unitOfWOrk.Save();
            }
            // sepetten kaldırılma işlem 
            List<Cart> Carts = _unitOfWOrk.Cart.GetAll(u=>u.AppUserId == CartVM.OrderProduct.AppUserId).ToList(); // list cart ile ilgili o yuzden cart kyllandık, get all diyerek burdaki kullanıcıya ait

            // sepet bilgi istiyoruz, tutyoruz,cartvm-den aldık, sepetteki urunleri cektik

            _unitOfWOrk.Cart.RemoveRange(Carts);     // silme işlemi
            _unitOfWOrk.Save();

            var cartCount = _unitOfWOrk.Cart.GetAll(u => u.AppUserId == claim.Value).ToList().Count;
            HttpContext.Session.SetInt32("SessionCartCount", cartCount);

            return RedirectToAction(nameof(Index), "Home", new {area="Customer"});
        }

        public IActionResult Increase(int cartId)
        {
            var cart = _unitOfWOrk.Cart.GetFirstOrDefault(c=>c.Id == cartId);
            cart.Count += 1;
            _unitOfWOrk.Save();
            return RedirectToAction(nameof(Index));
        }
        public IActionResult Decrease(int cartId)
        {
            var cart = _unitOfWOrk.Cart.GetFirstOrDefault(c => c.Id == cartId);

            if (cart.Count > 1)
            {
                cart.Count -= 1;               
            }
            else
            {
                _unitOfWOrk.Cart.Remove(cart);
                var cartCount = _unitOfWOrk.Cart.GetAll(u=>u.AppUserId == cart.AppUserId).ToList().Count -1 ;
                HttpContext.Session.SetInt32("SessionCartCount" , cartCount);
               
            }

            _unitOfWOrk.Save();
            return RedirectToAction(nameof(Index));
        }
    }
}
