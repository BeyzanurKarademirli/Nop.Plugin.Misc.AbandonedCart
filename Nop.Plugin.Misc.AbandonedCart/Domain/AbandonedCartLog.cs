using System;
using Nop.Core;

namespace Nop.Plugin.Misc.AbandonedCart.Domain
{
    /// <summary>
    /// Represents an abandoned cart reminder log record
    /// </summary>
    public class AbandonedCartLog : BaseEntity
    {
        /// <summary>
        /// Gets or sets the customer identifier
        /// </summary>
        public int CustomerId { get; set; }

        /// <summary>
        /// Gets or sets the customer email
        /// </summary>
        public string CustomerEmail { get; set; }

        /// <summary>
        /// Gets or sets the total cart item count at the time of reminder
        /// </summary>
        public int CartItemCount { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the reminder was sent (UTC)
        /// </summary>
        public DateTime SentOnUtc { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the cart was last updated (UTC)
        /// </summary>
        public DateTime LastCartItemUpdatedOnUtc { get; set; }
    }
}
