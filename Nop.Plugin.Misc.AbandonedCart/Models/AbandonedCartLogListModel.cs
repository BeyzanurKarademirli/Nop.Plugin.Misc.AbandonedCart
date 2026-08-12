using Nop.Web.Framework.Models;

namespace Nop.Plugin.Misc.AbandonedCart.Models
{
    /// <summary>
    /// Represents abandoned cart log list model
    /// </summary>
    public record AbandonedCartLogListModel : BasePagedListModel<AbandonedCartLogModel>
    {
    }
}
