using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core.Domain.Messages;
using Nop.Core.Domain.ScheduleTasks;
using Nop.Data.Migrations;
using Nop.Plugin.Misc.AbandonedCart.Models;
using Nop.Plugin.Misc.AbandonedCart.Services;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.ScheduleTasks;
using Nop.Web.Framework.Models.Extensions;

namespace Nop.Plugin.Misc.AbandonedCart.Factories
{
    /// <summary>
    /// Represents the Abandoned Cart model factory implementation
    /// </summary>
    public class AbandonedCartModelFactory : IAbandonedCartModelFactory
    {
        #region Fields

        private readonly IAbandonedCartService _abandonedCartService;
        private readonly ISettingService _settingService;
        private readonly ILocalizationService _localizationService;
        private readonly IScheduleTaskService _scheduleTaskService;
        private readonly IMessageTemplateService _messageTemplateService;
        private readonly IEmailAccountService _emailAccountService;
        private readonly IMigrationManager _migrationManager;
        private readonly AbandonedCartSettings _settings;

        #endregion

        #region Ctor

        public AbandonedCartModelFactory(
            IAbandonedCartService abandonedCartService,
            ISettingService settingService,
            ILocalizationService localizationService,
            IScheduleTaskService scheduleTaskService,
            IMessageTemplateService messageTemplateService,
            IEmailAccountService emailAccountService,
            IMigrationManager migrationManager,
            AbandonedCartSettings settings)
        {
            _abandonedCartService = abandonedCartService;
            _settingService = settingService;
            _localizationService = localizationService;
            _scheduleTaskService = scheduleTaskService;
            _messageTemplateService = messageTemplateService;
            _emailAccountService = emailAccountService;
            _migrationManager = migrationManager;
            _settings = settings;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Prepare configuration model
        /// </summary>
        /// <param name="model">Configuration model</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task<ConfigurationModel> PrepareConfigurationModelAsync(ConfigurationModel model = null)
        {
            // Apply database migrations if missing
            try
            {
                _migrationManager.ApplyUpMigrations(typeof(AbandonedCartPlugin).Assembly);
            }
            catch
            {
                // Ignore if already migrated
            }

            // Auto-register ScheduleTask if missing
            var taskType = $"{typeof(AbandonedCartReminderTask).FullName}, Nop.Plugin.Misc.AbandonedCart";
            var oldTaskType = typeof(AbandonedCartReminderTask).FullName;

            var existingTask = await _scheduleTaskService.GetTaskByTypeAsync(taskType)
                ?? await _scheduleTaskService.GetTaskByTypeAsync(oldTaskType);

            if (existingTask == null)
            {
                await _scheduleTaskService.InsertTaskAsync(new ScheduleTask
                {
                    Enabled = true,
                    LastEnabledUtc = DateTime.UtcNow,
                    Seconds = 3600,
                    Name = "Sepette Ürün Unutan Müşterilere Hatırlatma Gönder (Abandoned Cart)",
                    Type = taskType
                });
            }
            else if (existingTask.Type != taskType)
            {
                existingTask.Type = taskType;
                await _scheduleTaskService.UpdateTaskAsync(existingTask);
            }

            // Auto-register MessageTemplate if missing
            var templates = await _messageTemplateService.GetMessageTemplatesByNameAsync("AbandonedCart.Reminder");
            if (!templates.Any())
            {
                var emailAccounts = await _emailAccountService.GetAllEmailAccountsAsync();
                var emailAccountId = emailAccounts.FirstOrDefault()?.Id ?? 0;

                await _messageTemplateService.InsertMessageTemplateAsync(new MessageTemplate
                {
                    Name = "AbandonedCart.Reminder",
                    Subject = "%Store.Name% - Sepetinizde Ürünler Unuttunuz!",
                    Body = @"<div style=""font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #e9ecef; border-radius: 8px;"">
    <h2 style=""color: #2b3e50;"">Merhaba %Customer.FullName%,</h2>
    <p>Sepetinizde tamamlanmayı bekleyen harika ürünler unuttunuz!</p>
    %ShoppingCart.Products%
    <div style=""text-align: center; margin: 25px 0;"">
        <a href=""%ShoppingCart.Url%"" style=""background-color: #28a745; color: #ffffff; padding: 12px 25px; text-decoration: none; border-radius: 5px; font-weight: bold; display: inline-block;"">Sepetime Git ve Alışverişi Tamamla</a>
    </div>
    <p style=""font-size: 12px; color: #6c757d;"">Bu e-posta %Store.Name% tarafından otomatik gönderilmiştir.</p>
</div>",
                    IsActive = true,
                    EmailAccountId = emailAccountId
                });
            }

            // Auto-register locales
            var resources = new Dictionary<string, string>
            {
                ["Plugins.Misc.AbandonedCart.Fields.Enabled"] = "Terk Edilmiş Sepet Hatırlatıcı Aktif mi?",
                ["Plugins.Misc.AbandonedCart.Fields.ReminderDelayHours"] = "Sepet Güncellendikten Kaç Saat Sonra Hatırlatma Atılsın? (Örn: 24 Saat)",
                ["Plugins.Misc.AbandonedCart.Fields.EmailSubject"] = "E-Posta Konusu (Varsayılan için boş bırakın)",
                ["Plugins.Misc.AbandonedCart.Fields.SearchEmail"] = "Müşteri E-Posta"
            };

            foreach (var resource in resources)
            {
                var val = await _localizationService.GetResourceAsync(resource.Key, 0, false, string.Empty, true);
                if (string.IsNullOrEmpty(val))
                    await _localizationService.AddOrUpdateLocaleResourceAsync(resource.Key, resource.Value);
            }

            model ??= new ConfigurationModel();

            model.Enabled = _settings.Enabled;
            model.ReminderDelayHours = _settings.ReminderDelayHours > 0 ? _settings.ReminderDelayHours : 24m;
            model.EmailSubject = _settings.EmailSubject;

            return model;
        }

        /// <summary>
        /// Prepare abandoned cart log list model
        /// </summary>
        /// <param name="searchModel">Abandoned cart log search model</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task<AbandonedCartLogListModel> PrepareAbandonedCartLogListModelAsync(AbandonedCartLogSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var pageIndex = searchModel.Page > 0 ? searchModel.Page - 1 : 0;
            var pageSize = searchModel.PageSize > 0 ? searchModel.PageSize : 15;

            var logs = await _abandonedCartService.GetLogsAsync(
                email: searchModel.SearchEmail,
                pageIndex: pageIndex,
                pageSize: pageSize
            );

            var model = await new AbandonedCartLogListModel().PrepareToGridAsync(searchModel, logs, () =>
            {
                return logs.SelectAwait(async log => new AbandonedCartLogModel
                {
                    Id = log.Id,
                    CustomerId = log.CustomerId,
                    CustomerEmail = log.CustomerEmail,
                    CartItemCount = log.CartItemCount,
                    SentOnUtc = log.SentOnUtc,
                    LastCartItemUpdatedOnUtc = log.LastCartItemUpdatedOnUtc
                });
            });

            return model;
        }

        #endregion
    }
}
