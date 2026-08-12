using Nop.Core.Configuration;

namespace Nop.Plugin.Misc.AbandonedCart.Services
{
    /// <summary>
    /// Represents settings for Abandoned Cart plugin
    /// </summary>
    public class AbandonedCartSettings : ISettings
    {
        /// <summary>
        /// Gets or sets a value indicating whether the reminder plugin is enabled
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Gets or sets the delay hours after cart update before sending reminder (e.g. 24, 1.5, 0.25)
        /// </summary>
        public decimal ReminderDelayHours { get; set; }

        /// <summary>
        /// Gets or sets custom email subject override (optional)
        /// </summary>
        public string EmailSubject { get; set; }
    }
}
