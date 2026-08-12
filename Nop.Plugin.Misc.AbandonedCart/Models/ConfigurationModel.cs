using System.ComponentModel.DataAnnotations;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.AbandonedCart.Models
{
    /// <summary>
    /// Represents configuration model
    /// </summary>
    public record ConfigurationModel : BaseNopModel
    {
        [NopResourceDisplayName("Plugins.Misc.AbandonedCart.Fields.Enabled")]
        public bool Enabled { get; set; }

        [NopResourceDisplayName("Plugins.Misc.AbandonedCart.Fields.ReminderDelayHours")]
        public decimal ReminderDelayHours { get; set; }

        [NopResourceDisplayName("Plugins.Misc.AbandonedCart.Fields.EmailSubject")]
        public string EmailSubject { get; set; }
    }
}
