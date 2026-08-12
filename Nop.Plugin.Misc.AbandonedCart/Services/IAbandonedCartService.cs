using System.Threading.Tasks;
using Nop.Core;
using Nop.Plugin.Misc.AbandonedCart.Domain;

namespace Nop.Plugin.Misc.AbandonedCart.Services
{
    /// <summary>
    /// Represents Abandoned Cart service interface
    /// </summary>
    public interface IAbandonedCartService
    {
        /// <summary>
        /// Process abandoned carts and send reminder emails
        /// </summary>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task ProcessAbandonedCartsAsync();

        /// <summary>
        /// Gets paged list of abandoned cart reminder logs
        /// </summary>
        /// <param name="email">Filter by customer email</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task<IPagedList<AbandonedCartLog>> GetLogsAsync(string email = null, int pageIndex = 0, int pageSize = int.MaxValue);

        /// <summary>
        /// Inserts a log record
        /// </summary>
        /// <param name="log">Log entity</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task InsertLogAsync(AbandonedCartLog log);
    }
}
