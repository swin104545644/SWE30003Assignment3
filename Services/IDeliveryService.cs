using OnlineShop.Models;

namespace OnlineShop.Services
{
    public interface IDeliveryService
    {
        Task<decimal> CalculateDeliveryPriceAsync(Order order, DeliveryMethod method, DeliveryAddress address, DateTime? scheduledFor = null);
        Task<Delivery> CreateDeliveryForOrderAsync(int orderId, DeliveryType type, DeliveryAddress address, DeliveryMethod method, DateTime? scheduledFor = null, string notes = null);
        Task<Delivery> UpdateDeliveryStatusAsync(int deliveryId, DeliveryStatus newStatus, string updatedBy = null);
        Task AssignCourierAsync(int deliveryId, string courierName, string trackingNumber = null);
    }

}
