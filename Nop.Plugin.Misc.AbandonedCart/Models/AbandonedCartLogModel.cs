using System;
using Nop.Web.Framework.Models;

namespace Nop.Plugin.Misc.AbandonedCart.Models
{
    /// <summary>
    /// Represents abandoned cart log model
    /// </summary>
    public record AbandonedCartLogModel : BaseNopEntityModel
    {
        public int CustomerId { get; set; }
        public string CustomerEmail { get; set; }
        public int CartItemCount { get; set; }
        public DateTime SentOnUtc { get; set; }
        public DateTime LastCartItemUpdatedOnUtc { get; set; }
    }
}
