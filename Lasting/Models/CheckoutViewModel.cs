namespace Lasting.Models
{
    public class CheckoutViewModel
    {
        public IEnumerable<CartItem> CartItems { get; set; }
        public decimal TotalAmount { get; set; }
        public ShippingAddress ShippingAddress { get; set; }
    }
}
