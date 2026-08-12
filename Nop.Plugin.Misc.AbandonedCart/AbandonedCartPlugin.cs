using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Nop.Core.Domain.Messages;
using Nop.Core.Domain.ScheduleTasks;
using Nop.Plugin.Misc.AbandonedCart.Services;
using Nop.Services.Cms;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Plugins;
using Nop.Services.ScheduleTasks;

namespace Nop.Plugin.Misc.AbandonedCart
{
    /// <summary>
    /// Represents the Abandoned Cart plugin
    /// </summary>
    public class AbandonedCartPlugin : BasePlugin, IMiscPlugin
    {
        #region Fields

        private readonly IActionContextAccessor _actionContextAccessor;
        private readonly ISettingService _settingService;
        private readonly ILocalizationService _localizationService;
        private readonly IUrlHelperFactory _urlHelperFactory;
        private readonly IScheduleTaskService _scheduleTaskService;
        private readonly IMessageTemplateService _messageTemplateService;
        private readonly IEmailAccountService _emailAccountService;

        #endregion

        #region Ctor

        public AbandonedCartPlugin(
            IActionContextAccessor actionContextAccessor,
            ISettingService settingService,
            ILocalizationService localizationService,
            IUrlHelperFactory urlHelperFactory,
            IScheduleTaskService scheduleTaskService,
            IMessageTemplateService messageTemplateService,
            IEmailAccountService emailAccountService)
        {
            _actionContextAccessor = actionContextAccessor;
            _settingService = settingService;
            _localizationService = localizationService;
            _urlHelperFactory = urlHelperFactory;
            _scheduleTaskService = scheduleTaskService;
            _messageTemplateService = messageTemplateService;
            _emailAccountService = emailAccountService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets a configuration page URL
        /// </summary>
        public override string GetConfigurationPageUrl()
        {
            return _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext).RouteUrl("Plugin.Misc.AbandonedCart.Configure");
        }

        /// <summary>
        /// Install the plugin
        /// </summary>
        public override async Task InstallAsync()
        {
            // Install settings
            var settings = new AbandonedCartSettings
            {
                Enabled = true,
                ReminderDelayHours = 24m,
                EmailSubject = string.Empty
            };
            await _settingService.SaveSettingAsync(settings);

            // Install scheduled task
            var taskType = $"{typeof(AbandonedCartReminderTask).FullName}, Nop.Plugin.Misc.AbandonedCart";
            if (await _scheduleTaskService.GetTaskByTypeAsync(taskType) == null)
            {
                await _scheduleTaskService.InsertTaskAsync(new ScheduleTask
                {
                    Enabled = true,
                    LastEnabledUtc = DateTime.UtcNow,
                    Seconds = 3600, // Run every 1 hour
                    Name = "Sepette Ürün Unutan Müşterilere Hatırlatma Gönder (Abandoned Cart)",
                    Type = taskType
                });
            }

            // Install email message template
            var emailAccounts = await _emailAccountService.GetAllEmailAccountsAsync();
            var defaultEmailAccount = emailAccounts.FirstOrDefault();
            var emailAccountId = defaultEmailAccount?.Id ?? 0;

            var existingTemplates = await _messageTemplateService.GetMessageTemplatesByNameAsync("AbandonedCart.Reminder");
            if (!existingTemplates.Any())
            {
                var template = new MessageTemplate
                {
                    Name = "AbandonedCart.Reminder",
                    BccEmailAddresses = null,
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
                    EmailAccountId = emailAccountId,
                    LimitedToStores = false
                };

                await _messageTemplateService.InsertMessageTemplateAsync(template);
            }

            // Install locale resources
            await _localizationService.AddOrUpdateLocaleResourceAsync(new Dictionary<string, string>
            {
                ["Plugins.Misc.AbandonedCart.Fields.Enabled"] = "Terk Edilmiş Sepet Hatırlatıcı Aktif mi?",
                ["Plugins.Misc.AbandonedCart.Fields.ReminderDelayHours"] = "Sepet Güncellendikten Kaç Saat Sonra Hatırlatma Atılsın? (Örn: 24 Saat)",
                ["Plugins.Misc.AbandonedCart.Fields.EmailSubject"] = "E-Posta Konusu (Varsayılan için boş bırakın)",
                ["Plugins.Misc.AbandonedCart.Fields.SearchEmail"] = "Müşteri E-Posta"
            });

            await base.InstallAsync();
        }

        /// <summary>
        /// Uninstall the plugin
        /// </summary>
        public override async Task UninstallAsync()
        {
            // Uninstall settings
            await _settingService.DeleteSettingAsync<AbandonedCartSettings>();

            // Uninstall scheduled task
            var taskType = typeof(AbandonedCartReminderTask).FullName;
            var task = await _scheduleTaskService.GetTaskByTypeAsync(taskType);
            if (task != null)
                await _scheduleTaskService.DeleteTaskAsync(task);

            // Uninstall message template
            var existingTemplates = await _messageTemplateService.GetMessageTemplatesByNameAsync("AbandonedCart.Reminder");
            foreach (var template in existingTemplates)
            {
                await _messageTemplateService.DeleteMessageTemplateAsync(template);
            }

            // Uninstall locale resources
            await _localizationService.DeleteLocaleResourcesAsync(new List<string>
            {
                "Plugins.Misc.AbandonedCart.Fields.Enabled",
                "Plugins.Misc.AbandonedCart.Fields.ReminderDelayHours",
                "Plugins.Misc.AbandonedCart.Fields.EmailSubject",
                "Plugins.Misc.AbandonedCart.Fields.SearchEmail"
            });

            await base.UninstallAsync();
        }

        #endregion
    }
}
