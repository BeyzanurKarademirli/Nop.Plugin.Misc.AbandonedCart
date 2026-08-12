using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Messages;
using Nop.Core.Domain.Orders;
using Nop.Data;
using Nop.Data.Migrations;
using Nop.Plugin.Misc.AbandonedCart.Domain;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Stores;

namespace Nop.Plugin.Misc.AbandonedCart.Services
{
    /// <summary>
    /// Represents Abandoned Cart service implementation
    /// </summary>
    public class AbandonedCartService : IAbandonedCartService
    {
        #region Fields

        private readonly IRepository<ShoppingCartItem> _shoppingCartItemRepository;
        private readonly IRepository<AbandonedCartLog> _logRepository;
        private readonly ICustomerService _customerService;
        private readonly IProductService _productService;
        private readonly IMessageTemplateService _messageTemplateService;
        private readonly IEmailAccountService _emailAccountService;
        private readonly IQueuedEmailService _queuedEmailService;
        private readonly IStoreService _storeService;
        private readonly IWebHelper _webHelper;
        private readonly IMigrationManager _migrationManager;
        private readonly AbandonedCartSettings _settings;

        #endregion

        #region Ctor

        public AbandonedCartService(
            IRepository<ShoppingCartItem> shoppingCartItemRepository,
            IRepository<AbandonedCartLog> logRepository,
            ICustomerService customerService,
            IProductService productService,
            IMessageTemplateService messageTemplateService,
            IEmailAccountService emailAccountService,
            IQueuedEmailService queuedEmailService,
            IStoreService storeService,
            IWebHelper webHelper,
            IMigrationManager migrationManager,
            AbandonedCartSettings settings)
        {
            _shoppingCartItemRepository = shoppingCartItemRepository;
            _logRepository = logRepository;
            _customerService = customerService;
            _productService = productService;
            _messageTemplateService = messageTemplateService;
            _emailAccountService = emailAccountService;
            _queuedEmailService = queuedEmailService;
            _storeService = storeService;
            _webHelper = webHelper;
            _migrationManager = migrationManager;
            _settings = settings;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Process abandoned carts and send reminder emails
        /// </summary>
        public async Task ProcessAbandonedCartsAsync()
        {
            if (!_settings.Enabled)
                return;

            // Ensure database table exists before processing
            try
            {
                _migrationManager.ApplyUpMigrations(typeof(AbandonedCartPlugin).Assembly);
            }
            catch
            {
                // Ignore if already migrated
            }

            var delayHours = (double)(_settings.ReminderDelayHours > 0 ? _settings.ReminderDelayHours : 24m);
            var cutoffUtc = DateTime.UtcNow.AddHours(-delayHours);

            // Fetch shopping cart items
            var cartItems = await _shoppingCartItemRepository.GetAllAsync(query =>
            {
                return query.Where(sci => sci.ShoppingCartTypeId == (int)ShoppingCartType.ShoppingCart);
            });

            if (!cartItems.Any())
                return;

            var customerCartGroups = cartItems.GroupBy(ci => ci.CustomerId);

            var emailAccounts = await _emailAccountService.GetAllEmailAccountsAsync();
            var emailAccount = emailAccounts.FirstOrDefault();
            if (emailAccount == null)
                return;

            var stores = await _storeService.GetAllStoresAsync();
            var store = stores.FirstOrDefault();
            var storeLocation = _webHelper.GetStoreLocation();

            var templates = await _messageTemplateService.GetMessageTemplatesByNameAsync("AbandonedCart.Reminder");
            var template = templates.FirstOrDefault();
            if (template == null || !template.IsActive)
                return;

            foreach (var group in customerCartGroups)
            {
                var customerId = group.Key;
                var customer = await _customerService.GetCustomerByIdAsync(customerId);
                if (customer == null || string.IsNullOrWhiteSpace(customer.Email) || customer.Deleted || !customer.Active)
                    continue;

                var lastUpdatedUtc = group.Max(ci => ci.UpdatedOnUtc);
                if (lastUpdatedUtc > cutoffUtc)
                    continue; // Cart was updated recently

                // Check if already sent a reminder for this exact cart update
                AbandonedCartLog existingLog = null;
                try
                {
                    existingLog = await _logRepository.Table.FirstOrDefaultAsync(l =>
                        l.CustomerId == customerId && l.LastCartItemUpdatedOnUtc == lastUpdatedUtc);
                }
                catch
                {
                    // Table might not exist yet
                }

                if (existingLog != null)
                    continue;

                // Prepare HTML list of products in the cart
                var productListBuilder = new StringBuilder();
                productListBuilder.Append("<table style=\"width:100%; border-collapse:collapse; margin: 15px 0;\">");
                productListBuilder.Append("<thead><tr style=\"background-color:#f8f9fa; text-align:left;\">");
                productListBuilder.Append("<th style=\"padding:10px; border:1px solid #dee2e6;\">Ürün</th>");
                productListBuilder.Append("<th style=\"padding:10px; border:1px solid #dee2e6; text-align:center;\">Adet</th>");
                productListBuilder.Append("</tr></thead><tbody>");

                foreach (var item in group)
                {
                    var product = await _productService.GetProductByIdAsync(item.ProductId);
                    var productName = product?.Name ?? "Ürün";

                    productListBuilder.Append("<tr>");
                    productListBuilder.Append($"<td style=\"padding:10px; border:1px solid #dee2e6;\"><strong>{productName}</strong></td>");
                    productListBuilder.Append($"<td style=\"padding:10px; border:1px solid #dee2e6; text-align:center;\">{item.Quantity}</td>");
                    productListBuilder.Append("</tr>");
                }
                productListBuilder.Append("</tbody></table>");

                var fullName = await _customerService.GetCustomerFullNameAsync(customer);
                var customerName = !string.IsNullOrWhiteSpace(fullName) ? fullName : customer.Email;

                var subject = !string.IsNullOrWhiteSpace(_settings.EmailSubject)
                    ? _settings.EmailSubject
                    : template.Subject;

                subject = subject
                    .Replace("%Store.Name%", store?.Name ?? "Mağazamız")
                    .Replace("%Customer.FullName%", customerName);

                var body = template.Body
                    .Replace("%Customer.FullName%", customerName)
                    .Replace("%Customer.Email%", customer.Email)
                    .Replace("%ShoppingCart.Products%", productListBuilder.ToString())
                    .Replace("%ShoppingCart.Url%", $"{storeLocation}cart")
                    .Replace("%Store.Name%", store?.Name ?? "Mağazamız")
                    .Replace("%Store.URL%", storeLocation);

                // Queue email
                var queuedEmail = new QueuedEmail
                {
                    PriorityId = 5,
                    From = emailAccount.Email,
                    FromName = emailAccount.DisplayName,
                    To = customer.Email,
                    ToName = customerName,
                    Subject = subject,
                    Body = body,
                    CreatedOnUtc = DateTime.UtcNow,
                    EmailAccountId = emailAccount.Id
                };

                await _queuedEmailService.InsertQueuedEmailAsync(queuedEmail);

                // Insert log record
                var log = new AbandonedCartLog
                {
                    CustomerId = customerId,
                    CustomerEmail = customer.Email,
                    CartItemCount = group.Sum(ci => ci.Quantity),
                    SentOnUtc = DateTime.UtcNow,
                    LastCartItemUpdatedOnUtc = lastUpdatedUtc
                };

                await InsertLogAsync(log);
            }
        }

        public async Task<IPagedList<AbandonedCartLog>> GetLogsAsync(string email = null, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            try
            {
                var query = _logRepository.Table;

                if (!string.IsNullOrWhiteSpace(email))
                    query = query.Where(l => l.CustomerEmail.Contains(email.Trim()));

                query = query.OrderByDescending(l => l.SentOnUtc);

                return await query.ToPagedListAsync(pageIndex, pageSize);
            }
            catch
            {
                return new PagedList<AbandonedCartLog>(new List<AbandonedCartLog>(), pageIndex, pageSize);
            }
        }

        public async Task InsertLogAsync(AbandonedCartLog log)
        {
            if (log == null)
                throw new ArgumentNullException(nameof(log));

            try
            {
                await _logRepository.InsertAsync(log);
            }
            catch
            {
                // Table might not exist yet if plugin was enabled manually
            }
        }

        #endregion
    }
}
