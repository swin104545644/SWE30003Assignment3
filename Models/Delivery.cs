namespace OnlineShop.Models
{
    public class Delivery
    {
        public int Id { get; set; }
        public int? UserId { get; set; }
        public int OrderId { get; set; }
        public Order? Order { get; set; }

        public DeliveryType DeliveryType { get; set; }
        public DeliveryStatus Status { get; set; } = DeliveryStatus.Pending;

        public decimal Price { get; set; }

        public int? DeliveryAddressId { get; set; }
        public required DeliveryAddress DeliveryAddress { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        // public DateTime? ScheduledFor { get; set; }

        public string TrackingNumber { get; set; }

        public string Notes { get; set; }
    }
}

public enum DeliveryType
{
    HomeDelivery = 0,
    PickUp = 1,
    CourierDelivery = 2
}

public enum DeliveryStatus
{
    Pending = 0,
    Confirmed = 1,
    ReadyForPickup = 2,
    PickedUp = 3,
    InTransit = 4,
    Delivered = 5,
    Cancelled = 6,
    Failed = 7
}