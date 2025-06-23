using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Lasting.Models
{
    public class CheckoutViewModel
    {
        [ValidateNever]
        public IEnumerable<CartItem> CartItems { get; set; }
        public decimal TotalAmount { get; set; }
        public ShippingAddress ShippingAddress { get; set; }
    }
}
