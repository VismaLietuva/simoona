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
                BEGIN
                    DECLARE @found nvarchar(200) = (
                        SELECT t.name + '(' + CASE
                                WHEN c.max_length = -1 THEN 'max'
                                WHEN t.name IN ('nvarchar', 'nchar') THEN CAST(c.max_length / 2 AS nvarchar(10))
                                ELSE CAST(c.max_length AS nvarchar(10))
                            END + ')'
                        FROM sys.columns c
                        JOIN sys.types t ON t.user_type_id = c.user_type_id
                        WHERE c.object_id = OBJECT_ID('AspNetUsers') AND c.name = 'Id');

                    DECLARE @message nvarchar(400) =
                        N'Cannot size KudosLogs.SentToId from AspNetUsers.Id: expected a fixed-length nvarchar, found '
                        + ISNULL(@found, N'no AspNetUsers.Id column') + N'.';

                    THROW 50000, @message, 1;
                END

                DECLARE @current int = (
                    SELECT c.max_length / 2
                    FROM sys.columns c
                    WHERE c.object_id = OBJECT_ID('KudosLogs') AND c.name = 'SentToId');

                IF @current IS NULL
                BEGIN
                    DECLARE @add nvarchar(max) = N'ALTER TABLE KudosLogs ADD SentToId NVARCHAR('
                        + CAST(@length AS nvarchar(10)) + N') NULL';
                    EXEC sp_executesql @add;
                END
                ELSE IF @current <> @length
                BEGIN
                    IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_KudosLogs_AspNetUsers_SentToId')
                        ALTER TABLE KudosLogs DROP CONSTRAINT FK_KudosLogs_AspNetUsers_SentToId;

                    IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_KudosLogs_SentToId' AND object_id = OBJECT_ID('KudosLogs'))
                        DROP INDEX IX_KudosLogs_SentToId ON KudosLogs;

                    DECLARE @alter nvarchar(max) = N'ALTER TABLE KudosLogs ALTER COLUMN SentToId NVARCHAR('
                        + CAST(@length AS nvarchar(10)) + N') NULL';
                    EXEC sp_executesql @alter;
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
