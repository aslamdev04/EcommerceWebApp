using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ShopNest.Controllers
{
    [Authorize(Roles = "User")]
    public class CustomerController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Browse()
        {
            return View();
        }
        public IActionResult Cart()
        {
            return View();
        }
        public IActionResult CheckOut()
        {
            return View();
        }
        public IActionResult Profile()
        {
            return View();
        }
        public IActionResult MyOrders()
        {
            return View();
        }
        public IActionResult Order_Track()
        {
            return View();
           
        }
        public IActionResult Reviews()
        {
            return View();
           
        }
        public IActionResult Notifications()
        {
            return View();
           
        }
        public IActionResult Payment()
        {
            return View();
           
        }
  
    }
}
