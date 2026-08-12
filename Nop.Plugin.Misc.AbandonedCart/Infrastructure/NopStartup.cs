using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using Nop.Plugin.Misc.AbandonedCart.Factories;
using Nop.Plugin.Misc.AbandonedCart.Services;

namespace Nop.Plugin.Misc.AbandonedCart.Infrastructure
{
    /// <summary>
    /// Represents object for the initializing services on application startup
    /// </summary>
    public class NopStartup : INopStartup
    {
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IAbandonedCartService, AbandonedCartService>();
            services.AddScoped<IAbandonedCartModelFactory, AbandonedCartModelFactory>();
            services.AddScoped<AbandonedCartReminderTask>();
        }

        public void Configure(IApplicationBuilder application)
        {
        }

        public int Order => 3000;
    }
}
