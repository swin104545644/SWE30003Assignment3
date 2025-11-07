namespace OnlineShop.Models;

public class DeliveryService : IDeliveryService
{
    private readonly ApplicationDbContext _db;
    private readonly ILogger<DeliveryService> _logger;
    private readonly IShippingZoneService _zoneService;


    public DeliveryService(ApplicationDbContext db, ILogger<DeliveryService> logger, IShippingZoneService zoneService = null)
    {
        _db = db;
        _logger = logger;
        _zoneService = zoneService;
    }

    public async Task<decimal> CalculateDeliveryPriceAsync(Order order, DeliveryMethod method, DeliveryAddress address, DateTime? scheduledFor = null)
    {
        if (method == null) throw new ArgumentNullException(nameof(method));
        if (order == null) throw new ArgumentNullException(nameof(order));

        // If order total >= FreeOverAmount (75) -> 0
        if (method.FreeOverAmount > 0 && order.Total >= method.FreeOverAmount) return 0m;

        decimal price = method.BasePrice;

        // Distance-based price (if zone service provided)
        if (_zoneService != null && (method.PerKmPrice > 0 || method.MaxDistanceKm > 0))
        {
            var storeLocation = await _zoneService.GetStoreLocationAsync();
            var distanceKm = await _zoneService.CalculateDistanceKmAsync(storeLocation, address);
            if (method.MaxDistanceKm > 0 && distanceKm > method.MaxDistanceKm)
            {
                throw new InvalidOperationException("Delivery address is outside the delivery area for this method.");
            }

            price += (decimal)distanceKm * method.PerKmPrice;
        }


        return Math.Round(price, 2);
    }

    public async Task<Delivery> CreateDeliveryForOrderAsync(int orderId, DeliveryType type, DeliveryAddress address, DeliveryMethod method, DateTime? scheduledFor = null, string notes = null)
    {
        var order = await _db.Orders.Include(o => o.OrderItems).ThenInclude(i => i.Product).FirstOrDefaultAsync(o => o.Id == orderId);
        if (order == null) throw new ArgumentException("Order not found", nameof(orderId));

        var price = await CalculateDeliveryPriceAsync(order, method, address, scheduledFor);

        if (address != null && address.Id == 0)
        {
            _db.DeliveryAddresses.Add(address);
            await _db.SaveChangesAsync();
        }

        var delivery = new Delivery
        {
            OrderId = orderId,
            DeliveryType = type,
            DeliveryAddressId = address?.Id,
            Price = price,
            Notes = notes,
            Status = DeliveryStatus.Pending
        };

        _db.Deliveries.Add(delivery);
        await _db.SaveChangesAsync();


        return delivery;
    }

    public async Task<Delivery> UpdateDeliveryStatusAsync(int deliveryId, DeliveryStatus newStatus, string updatedBy = null)
    {
        var del = await _db.Deliveries.Include(d => d.Order).FirstOrDefaultAsync(d => d.Id == deliveryId);
        if (del == null) throw new ArgumentException(nameof(deliveryId));

        del.Status = newStatus;
        _db.Deliveries.Update(del);
        await _db.SaveChangesAsync();

        return del;
    }

    public async Task AssignCourierAsync(int deliveryId, string courierName, string trackingNumber = null)
    {
        var del = await _db.Deliveries.FirstOrDefaultAsync(d => d.Id == deliveryId);
        if (del == null) throw new ArgumentException(nameof(deliveryId));

        del.CourierName = courierName;
        del.TrackingNumber = trackingNumber;
        del.Status = DeliveryStatus.InTransit;

        _db.Deliveries.Update(del);
        await _db.SaveChangesAsync();
    }
}