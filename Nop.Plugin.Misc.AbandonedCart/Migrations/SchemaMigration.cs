using FluentMigrator;
using Nop.Data.Extensions;
using Nop.Data.Migrations;
using Nop.Plugin.Misc.AbandonedCart.Domain;

namespace Nop.Plugin.Misc.AbandonedCart.Migrations
{
    [NopMigration("2026/08/12 00:00:00", "Misc.AbandonedCart schema", MigrationProcessType.Installation)]
    public class SchemaMigration : AutoReversingMigration
    {
        public override void Up()
        {
            Create.TableFor<AbandonedCartLog>();
        }
    }
}
