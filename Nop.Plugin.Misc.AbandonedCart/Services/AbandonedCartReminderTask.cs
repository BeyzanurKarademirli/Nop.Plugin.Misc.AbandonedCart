using System.Threading.Tasks;
using Nop.Services.ScheduleTasks;

namespace Nop.Plugin.Misc.AbandonedCart.Services
{
    /// <summary>
    /// Represents a schedule task to send abandoned cart reminder emails
    /// </summary>
    public class AbandonedCartReminderTask : IScheduleTask
    {
        #region Fields

        private readonly IAbandonedCartService _abandonedCartService;

        #endregion

        #region Ctor

        public AbandonedCartReminderTask(IAbandonedCartService abandonedCartService)
        {
            _abandonedCartService = abandonedCartService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Execute task
        /// </summary>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task ExecuteAsync()
        {
            await _abandonedCartService.ProcessAbandonedCartsAsync();
        }

        #endregion
    }
}
