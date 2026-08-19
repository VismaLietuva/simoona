using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shrooms.DataLayer.EFCoreMigrations
{
    /// <inheritdoc />
    public partial class AddKudosLogSentTo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DECLARE @length int = (
                    SELECT c.max_length / 2
                    FROM sys.columns c
                    WHERE c.object_id = OBJECT_ID('AspNetUsers') AND c.name = 'Id');

                IF @length IS NULL OR @length <= 0
                    THROW 50000, 'Cannot size KudosLogs.SentToId: AspNetUsers.Id is missing or not a fixed-length nvarchar.', 1;

                DECLARE @current int = (
                    SELECT c.max_length / 2
                    FROM sys.columns c
                    WHERE c.object_id = OBJECT_ID('KudosLogs') AND c.name = 'SentToId');

                IF @current IS NULL
                BEGIN
                    EXEC('ALTER TABLE KudosLogs ADD SentToId NVARCHAR(' + @length + ') NULL');
                END
                ELSE IF @current <> @length
                BEGIN
                    IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_KudosLogs_AspNetUsers_SentToId')
                        ALTER TABLE KudosLogs DROP CONSTRAINT FK_KudosLogs_AspNetUsers_SentToId;

                    IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_KudosLogs_SentToId' AND object_id = OBJECT_ID('KudosLogs'))
                        DROP INDEX IX_KudosLogs_SentToId ON KudosLogs;

                    EXEC('ALTER TABLE KudosLogs ALTER COLUMN SentToId NVARCHAR(' + @length + ') NULL');
                END
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_KudosLogs_SentToId' AND object_id = OBJECT_ID('KudosLogs'))
                    CREATE INDEX IX_KudosLogs_SentToId ON KudosLogs (SentToId);

                IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_KudosLogs_AspNetUsers_SentToId')
                    ALTER TABLE KudosLogs ADD CONSTRAINT FK_KudosLogs_AspNetUsers_SentToId
                        FOREIGN KEY (SentToId) REFERENCES AspNetUsers (Id);
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_KudosLogs_AspNetUsers_SentToId')
                    ALTER TABLE KudosLogs DROP CONSTRAINT FK_KudosLogs_AspNetUsers_SentToId;

                IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_KudosLogs_SentToId' AND object_id = OBJECT_ID('KudosLogs'))
                    DROP INDEX IX_KudosLogs_SentToId ON KudosLogs;

                IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'KudosLogs' AND COLUMN_NAME = 'SentToId')
                    ALTER TABLE KudosLogs DROP COLUMN SentToId;
            ");
        }
    }
}
