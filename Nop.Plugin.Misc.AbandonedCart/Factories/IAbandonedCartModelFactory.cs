using System.Threading.Tasks;
using Nop.Plugin.Misc.AbandonedCart.Models;

namespace Nop.Plugin.Misc.AbandonedCart.Factories
{
    /// <summary>
    /// Represents the Abandoned Cart model factory interface
    /// </summary>
    public interface IAbandonedCartModelFactory
    {
        /// <summary>
        /// Prepare configuration model
        /// </summary>
        /// <param name="model">Configuration model</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task<ConfigurationModel> PrepareConfigurationModelAsync(ConfigurationModel model = null);

        /// <summary>
        /// Prepare abandoned cart log list model
        /// </summary>
        /// <param name="searchModel">Abandoned cart log search model</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task<AbandonedCartLogListModel> PrepareAbandonedCartLogListModelAsync(AbandonedCartLogSearchModel searchModel);
    }
}
