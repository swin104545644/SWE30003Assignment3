using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace OnlineShop.Controllers
{
    [Authorize(Roles = "Admin")]
    public class DeliveriesController : Controller
    {
        private readonly IDeliveryService _deliveryService;
        private readonly ApplicationDbContext _db;

        public DeliveriesController(IDeliveryService deliveryService, ApplicationDbContext db)
        {
            _deliveryService = deliveryService;
            _db = db;
        }
        // GET: admin list
        public async Task<IActionResult> Index()
        {
            var list = await _db.Deliveries
                .Include(d => d.Order)
                .Include(d => d.DeliveryAddress)
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync();
            return View(list);
        }

        // GET: Admin/Deliveries/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var del = await _db.Deliveries.Include(d => d.Order).Include(d => d.DeliveryAddress).FirstOrDefaultAsync(d => d.Id == id);
            if (del == null) return NotFound();
            return View(del);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int id, DeliveryStatus status)
        {
            await _deliveryService.UpdateDeliveryStatusAsync(id, status, User?.Identity?.Name);
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        public async Task<IActionResult> AssignCourier(int id, string courierName, string trackingNumber)
        {
            await _deliveryService.AssignCourierAsync(id, courierName, trackingNumber);
            return RedirectToAction(nameof(Details), new { id });
        }
    }
}