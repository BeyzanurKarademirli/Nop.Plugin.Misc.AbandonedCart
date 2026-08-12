using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.AbandonedCart.Models
{
    /// <summary>
    /// Represents abandoned cart log search model
    /// </summary>
    public record AbandonedCartLogSearchModel : BaseSearchModel
    {
        [NopResourceDisplayName("Plugins.Misc.AbandonedCart.Fields.SearchEmail")]
        public string SearchEmail { get; set; }
    }
}
