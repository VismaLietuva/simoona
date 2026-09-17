using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shrooms.DataLayer.EFCoreMigrations
{
    /// <inheritdoc />
    public partial class AddKudosLogsQueryIndexes : Migration
    {
        // Every tenant database predating InitialBaseline was restored from the EF6 schema, where
        // this index is called IX_OrganizationId rather than the IX_KudosLogs_OrganizationId that
        // EF Core scaffolds. A plain DropIndex fails with error 3701 on those, so both steps below
        // are written as guarded SQL: they locate the redundant index by shape instead of by name
        // and do nothing when it is already gone.
        private const string CompositeIndexName = "IX_KudosLogs_OrganizationId_Status_Created";

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Create the replacement first. The composite leads with OrganizationId, so it serves
            // every seek the single-column index served; dropping first would leave the table with
            // no index on OrganizationId while the new one builds.
            migrationBuilder.Sql($@"
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.KudosLogs') AND name = N'{CompositeIndexName}')
BEGIN
    CREATE NONCLUSTERED INDEX [{CompositeIndexName}] ON [dbo].[KudosLogs] ([OrganizationId], [Status], [Created])
        INCLUDE ([KudosSystemType], [KudosBasketId], [EmployeeId], [Points], [CreatedBy]);
END
");

            migrationBuilder.Sql(DropRedundantOrganizationIdIndexSql);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Restore a single-column index before removing the composite one currently covering
            // those seeks. It comes back under the name EF Core expects, not the EF6 name, because
            // that is what the model describes once this migration is rolled back.
            migrationBuilder.Sql(@"
IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes i
    WHERE i.object_id = OBJECT_ID(N'dbo.KudosLogs')
      AND i.type = 2
      AND i.is_unique = 0
      AND i.is_primary_key = 0
      AND i.is_unique_constraint = 0
      AND (SELECT COUNT(*) FROM sys.index_columns ic WHERE ic.object_id = i.object_id AND ic.index_id = i.index_id) = 1
      AND EXISTS (
          SELECT 1
          FROM sys.index_columns ic
          JOIN sys.columns c ON c.object_id = ic.object_id AND c.column_id = ic.column_id
          WHERE ic.object_id = i.object_id AND ic.index_id = i.index_id AND c.name = N'OrganizationId')
)
BEGIN
    CREATE NONCLUSTERED INDEX [IX_KudosLogs_OrganizationId] ON [dbo].[KudosLogs] ([OrganizationId]);
END
");

            migrationBuilder.Sql($@"
IF EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.KudosLogs') AND name = N'{CompositeIndexName}')
BEGIN
    DROP INDEX [{CompositeIndexName}] ON [dbo].[KudosLogs];
END
");
        }

        // Matches a nonclustered, non-unique index on KudosLogs whose single key column is
        // OrganizationId and which carries no included columns. The composite index created above
        // has three key columns, so it can never match.
        private const string DropRedundantOrganizationIdIndexSql = @"
DECLARE @redundantIndex sysname = (
    SELECT TOP 1 i.name
    FROM sys.indexes i
    WHERE i.object_id = OBJECT_ID(N'dbo.KudosLogs')
      AND i.type = 2
      AND i.is_unique = 0
      AND i.is_primary_key = 0
      AND i.is_unique_constraint = 0
      AND i.has_filter = 0
      AND (SELECT COUNT(*) FROM sys.index_columns ic WHERE ic.object_id = i.object_id AND ic.index_id = i.index_id) = 1
      AND EXISTS (
          SELECT 1
          FROM sys.index_columns ic
          JOIN sys.columns c ON c.object_id = ic.object_id AND c.column_id = ic.column_id
          WHERE ic.object_id = i.object_id
            AND ic.index_id = i.index_id
            AND ic.is_included_column = 0
            AND c.name = N'OrganizationId')
    ORDER BY i.name);

IF @redundantIndex IS NOT NULL
BEGIN
    DECLARE @sql nvarchar(max) = N'DROP INDEX ' + QUOTENAME(@redundantIndex) + N' ON [dbo].[KudosLogs];';
    EXEC sp_executesql @sql;
END
";
    }
}
