using Microsoft.AspNetCore.Mvc;

namespace ShopNest.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult user_mgmt()
        {
            return View();
        }
        public IActionResult Cat_mgmt()
        {
            return View();
        }
        public IActionResult Product_mon()
        {
            return View();
        }
        public IActionResult Order_mgmt()
        {
            return View();
        }
        public IActionResult Payment_mgmt()
        {
            return View();
        }
        public IActionResult Offer_noti()
        {
            return View();
        }
        public IActionResult Reports()
        {
            return View();
        }
     
    }
}
