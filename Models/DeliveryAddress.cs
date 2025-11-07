namespace OnlineShop.Models
{
    public class DeliveryAddress
    {
        public int Id { get; set; }
        public string RecipientName { get; set; }
        public string Line1 { get; set; }
        public string Line2 { get; set; }
        public string Suburb { get; set; }
        public string Postcode { get; set; }

        public string PhoneNumber { get; set; }
    }
}