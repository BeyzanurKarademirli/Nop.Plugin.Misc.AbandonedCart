using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc.Routing;

namespace Nop.Plugin.Misc.AbandonedCart.Infrastructure
{
    /// <summary>
    /// Represents Abandoned Cart plugin route provider
    /// </summary>
    public class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
        {
            // Admin routes
            endpointRouteBuilder.MapControllerRoute(
                name: "Plugin.Misc.AbandonedCart.Configure.Admin",
                pattern: "Admin/AbandonedCart/Configure",
                defaults: new { controller = "AbandonedCart", action = "Configure", area = AreaNames.Admin });

            endpointRouteBuilder.MapControllerRoute(
                name: "Plugin.Misc.AbandonedCart.LogList.Admin",
                pattern: "Admin/AbandonedCart/LogList",
                defaults: new { controller = "AbandonedCart", action = "LogList", area = AreaNames.Admin });

            // Plugins routes
            endpointRouteBuilder.MapControllerRoute(
                name: "Plugin.Misc.AbandonedCart.Configure",
                pattern: "Plugins/AbandonedCart/Configure",
                defaults: new { controller = "AbandonedCart", action = "Configure", area = AreaNames.Admin });

            endpointRouteBuilder.MapControllerRoute(
                name: "Plugin.Misc.AbandonedCart.LogList",
                pattern: "Plugins/AbandonedCart/LogList",
                defaults: new { controller = "AbandonedCart", action = "LogList", area = AreaNames.Admin });
        }

        public int Priority => 1000;
    }
}
