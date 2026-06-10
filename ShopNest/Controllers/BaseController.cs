using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using ShopNest.Repositories.Interfaces;
using System.Security.Claims;

namespace ShopNest.Controllers
{
    // Controllers/BaseController.cs
    public class BaseController : Controller
    {
        private readonly IUnitOfWork _uow;

        public BaseController(IUnitOfWork uow)
        {
            _uow = uow;
        }

        // Har action se pehle yeh chalega
        public override void OnActionExecuting(
            ActionExecutingContext context)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                var userIdClaim = User.FindFirst(
                    ClaimTypes.NameIdentifier)?.Value;

                if (int.TryParse(userIdClaim, out int userId))
                {
                    // Cart Count
                    var carts = _uow.Carts
                        .GetAllAsync().Result
                        .Where(c => c.UserId == userId)
                        .ToList();
                    ViewBag.CartCount = carts.Sum(c => c.Quantity);

                    // Orders Count
                    var orders = _uow.Orders
                        .GetAllAsync().Result
                        .Where(o => o.UserId == userId)
                        .ToList();
                    ViewBag.TotalOrders = orders.Count;
                    ViewBag.PendingOrders = orders
                        .Count(o => o.Status == "Pending" ||
                                    o.Status == "Processing");
                }
            }

            base.OnActionExecuting(context);
        }
    }
}
