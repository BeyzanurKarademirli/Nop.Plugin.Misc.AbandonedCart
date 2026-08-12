using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using Nop.Plugin.Misc.AbandonedCart.Domain;

namespace Nop.Plugin.Misc.AbandonedCart.Data
{
    /// <summary>
    /// Represents AbandonedCartLog entity builder
    /// </summary>
    public class AbandonedCartLogBuilder : NopEntityBuilder<AbandonedCartLog>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table.WithColumn(nameof(AbandonedCartLog.Id)).AsInt32().PrimaryKey().Identity()
                 .WithColumn(nameof(AbandonedCartLog.CustomerId)).AsInt32().NotNullable()
                 .WithColumn(nameof(AbandonedCartLog.CustomerEmail)).AsString(255).NotNullable()
                 .WithColumn(nameof(AbandonedCartLog.CartItemCount)).AsInt32().NotNullable()
                 .WithColumn(nameof(AbandonedCartLog.SentOnUtc)).AsDateTime2().NotNullable()
                 .WithColumn(nameof(AbandonedCartLog.LastCartItemUpdatedOnUtc)).AsDateTime2().NotNullable();
        }
    }
}
