using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Misc.AbandonedCart.Factories;
using Nop.Plugin.Misc.AbandonedCart.Models;
using Nop.Plugin.Misc.AbandonedCart.Services;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Misc.AbandonedCart.Controllers
{
    [AuthorizeAdmin]
    [Area(AreaNames.Admin)]
    [AutoValidateAntiforgeryToken]
    public class AbandonedCartController : BasePluginController
    {
        #region Fields

        private readonly IAbandonedCartModelFactory _abandonedCartModelFactory;
        private readonly ISettingService _settingService;
        private readonly INotificationService _notificationService;
        private readonly IPermissionService _permissionService;
        private readonly ILocalizationService _localizationService;
        private readonly AbandonedCartSettings _settings;

        #endregion

        #region Ctor

        public AbandonedCartController(
            IAbandonedCartModelFactory abandonedCartModelFactory,
            ISettingService settingService,
            INotificationService notificationService,
            IPermissionService permissionService,
            ILocalizationService localizationService,
            AbandonedCartSettings settings)
        {
            _abandonedCartModelFactory = abandonedCartModelFactory;
            _settingService = settingService;
            _notificationService = notificationService;
            _permissionService = permissionService;
            _localizationService = localizationService;
            _settings = settings;
        }

        #endregion

        #region Methods

        public async Task<IActionResult> Configure()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            var model = await _abandonedCartModelFactory.PrepareConfigurationModelAsync();

            return View("~/Plugins/Misc.AbandonedCart/Views/Configure.cshtml", model);
        }

        [HttpPost]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            if (ModelState.TryGetValue(nameof(ConfigurationModel.ReminderDelayHours), out var entry) && entry.Errors.Count > 0)
            {
                var rawValue = entry.AttemptedValue;
                if (!string.IsNullOrWhiteSpace(rawValue))
                {
                    rawValue = rawValue.Replace(',', '.');
                    if (decimal.TryParse(rawValue, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var parsedValue))
                    {
                        model.ReminderDelayHours = parsedValue;
                        ModelState.Remove(nameof(ConfigurationModel.ReminderDelayHours));
                    }
                }
            }

            if (!ModelState.IsValid)
                return await Configure();

            _settings.Enabled = model.Enabled;
            _settings.ReminderDelayHours = model.ReminderDelayHours > 0 ? model.ReminderDelayHours : 24m;
            _settings.EmailSubject = model.EmailSubject;

            await _settingService.SaveSettingAsync(_settings);

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Plugins.Saved"));

            return await Configure();
        }

        [HttpPost]
        public async Task<IActionResult> LogList(AbandonedCartLogSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePlugins))
                return await AccessDeniedDataTablesJson();

            var model = await _abandonedCartModelFactory.PrepareAbandonedCartLogListModelAsync(searchModel);

            return Json(model);
        }

        #endregion
    }
}
