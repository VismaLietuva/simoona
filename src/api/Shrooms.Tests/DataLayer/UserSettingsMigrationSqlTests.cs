using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Storage;
using NUnit.Framework;
using Shrooms.DataLayer.EFCoreMigrations;

namespace Shrooms.Tests.DataLayer
{
    /// <summary>
    /// Runs the AddUserSettings migration and UserSettingsCatchUp on a real SQL Server
    /// (LocalDB), in a throwaway database that holds only the AspNetUsers columns they
    /// touch. The mocked and in-memory tests never execute this SQL. Ignored where
    /// LocalDB is not available.
    /// </summary>
    [TestFixture]
    [NonParallelizable]
    public class UserSettingsMigrationSqlTests
    {
        private const string Server = @"Server=(localdb)\MSSQLLocalDB;Integrated Security=true;Connect Timeout=15;Pooling=false";

        private string _database;
        private SqlConnection _connection;
        private SqlServerContext _context;

        [SetUp]
        public void CreateDatabase()
        {
            var database = "UserSettingsMigrationTests_" + Guid.NewGuid().ToString("N");

            try
            {
                using var master = new SqlConnection(Server + ";Database=master");
                master.Open();
                Execute(master, $"CREATE DATABASE [{database}]");
            }
            catch (Exception e) when (e is SqlException or PlatformNotSupportedException or InvalidOperationException)
            {
                Assert.Ignore("Needs SQL Server LocalDB: " + e.Message);
            }

            _database = database;
            _connection = new SqlConnection(Server + ";Database=" + database);
            _connection.Open();
            _context = new SqlServerContext(new DbContextOptionsBuilder<SqlServerContext>().UseSqlServer(_connection).Options);

            Execute(@"
CREATE TABLE dbo.AspNetUsers (
    [Id] nvarchar(128) NOT NULL CONSTRAINT PK_AspNetUsers PRIMARY KEY,
    [CultureCode] nvarchar(max) NULL,
    [TimeZone] nvarchar(max) NULL,
    [DailyMailingHour] time NULL,
    [IsTutorialComplete] bit NOT NULL,
    [WallWidgetPreferences] nvarchar(max) NULL,
    [IsAnonymized] bit NOT NULL);");
        }

        [TearDown]
        public void DropDatabase()
        {
            _context?.Dispose();
            _connection?.Dispose();

            if (_database == null)
            {
                return;
            }

            using var master = new SqlConnection(Server + ";Database=master");
            master.Open();
            Execute(master, $"ALTER DATABASE [{_database}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [{_database}]");
            _database = null;
        }

        [Test]
        public void Up_Backfills_A_Row_Per_User()
        {
            AddUser("user", culture: "lt-LT", timeZone: "FLE Standard Time", mailingHour: TimeSpan.FromHours(8), tutorial: true, preferences: "[1]");
            AddUser("anonymized", culture: "en-US", preferences: "[2]", anonymized: true);

            Run(new AddUserSettings().UpOperations);

            Assert.That(Settings("user"), Is.EqualTo(new SettingsRow("lt-LT", "FLE Standard Time", TimeSpan.FromHours(8), true, "[1]")));
            Assert.That(Settings("anonymized"), Is.EqualTo(new SettingsRow("en-US", null, null, false, null)));
            Assert.That(OldPreferences("user"), Is.EqualTo("[1]"));
            Assert.That(OldPreferences("anonymized"), Is.Null, "the old copy of an anonymized user's preferences is cleared too");
        }

        [Test]
        public void Up_Gives_Every_Backfill_Row_The_Same_Created_And_Modified()
        {
            for (var i = 0; i < 50; i++)
            {
                AddUser("user" + i);
            }

            Run(new AddUserSettings().UpOperations);

            // UserSettingsCatchUp finds the backfill by MIN(Created) and an untouched row by Modified = Created.
            Assert.That(Scalar<int>("SELECT COUNT(*) FROM dbo.UserSettings WHERE [Modified] <> [Created]"), Is.Zero);
            Assert.That(Scalar<int>("SELECT COUNT(DISTINCT [Created]) FROM dbo.UserSettings"), Is.EqualTo(1));
        }

        [Test]
        public void Up_Refuses_A_Value_Too_Long_For_The_New_Column()
        {
            AddUser("user", culture: new string('x', 21));

            var ex = Assert.Throws<SqlException>(() => Run(new AddUserSettings().UpOperations));

            Assert.That(ex.Message, Does.Contain("AddUserSettings"));
            Assert.That(Scalar<object>("SELECT OBJECT_ID('dbo.UserSettings')"), Is.EqualTo(DBNull.Value));
        }

        [Test]
        public void Down_Copies_The_Settings_Back_And_Drops_The_Table()
        {
            AddUser("user", culture: "en-US", timeZone: "UTC");
            Run(new AddUserSettings().UpOperations);
            Execute("UPDATE dbo.UserSettings SET [CultureCode] = 'lt-LT', [IsTutorialComplete] = 1, [Preferences] = '[9]' WHERE [UserId] = 'user'");

            Run(new AddUserSettings().DownOperations);

            Assert.That(Scalar<string>("SELECT [CultureCode] FROM dbo.AspNetUsers WHERE [Id] = 'user'"), Is.EqualTo("lt-LT"));
            Assert.That(Scalar<bool>("SELECT [IsTutorialComplete] FROM dbo.AspNetUsers WHERE [Id] = 'user'"), Is.True);
            Assert.That(OldPreferences("user"), Is.EqualTo("[9]"));
            Assert.That(Scalar<object>("SELECT OBJECT_ID('dbo.UserSettings')"), Is.EqualTo(DBNull.Value));
        }

        [Test]
        public void CatchUp_Copies_What_An_Old_Instance_Changed_On_A_Row_The_New_Code_Never_Touched()
        {
            AddUser("user", culture: "en-US", timeZone: "UTC", preferences: "[1]");
            Run(new AddUserSettings().UpOperations);
            Execute("UPDATE dbo.AspNetUsers SET [TimeZone] = 'FLE Standard Time', [IsTutorialComplete] = 1, [WallWidgetPreferences] = '[1,2]' WHERE [Id] = 'user'");

            RunCatchUp();

            Assert.That(Settings("user"), Is.EqualTo(new SettingsRow("en-US", "FLE Standard Time", null, true, "[1,2]")));
        }

        [Test]
        public void CatchUp_Keeps_A_Row_The_New_Code_Changed()
        {
            AddUser("user", culture: "en-US", timeZone: "UTC");
            Run(new AddUserSettings().UpOperations);
            Execute("UPDATE dbo.UserSettings SET [CultureCode] = 'lt-LT', [Modified] = DATEADD(minute, 1, [Modified]) WHERE [UserId] = 'user'");
            Execute("UPDATE dbo.AspNetUsers SET [CultureCode] = 'de-DE' WHERE [Id] = 'user'");

            RunCatchUp();

            Assert.That(Settings("user"), Is.EqualTo(new SettingsRow("lt-LT", "UTC", null, false, null)));
        }

        [Test]
        public void CatchUp_Clears_The_Preferences_Of_A_User_An_Old_Instance_Anonymized()
        {
            AddUser("user", culture: "en-US", preferences: "[1]");
            Run(new AddUserSettings().UpOperations);
            Execute("UPDATE dbo.AspNetUsers SET [IsAnonymized] = 1 WHERE [Id] = 'user'");

            RunCatchUp();

            Assert.That(Settings("user"), Is.EqualTo(new SettingsRow("en-US", null, null, false, null)));
        }

        [Test]
        public void CatchUp_Adds_The_Row_Of_A_User_An_Old_Instance_Created()
        {
            AddUser("existing", culture: "en-US");
            Run(new AddUserSettings().UpOperations);
            AddUser("created", culture: "lt-LT", timeZone: "FLE Standard Time", mailingHour: TimeSpan.FromHours(7), tutorial: true, preferences: "[3]");

            RunCatchUp();

            Assert.That(Settings("created"), Is.EqualTo(new SettingsRow("lt-LT", "FLE Standard Time", TimeSpan.FromHours(7), true, "[3]")));
        }

        [Test]
        public void CatchUp_Fills_An_On_Demand_Row_From_What_The_Old_Instance_Wrote()
        {
            AddUser("existing", culture: "en-US");
            Run(new AddUserSettings().UpOperations);
            AddUser("created", culture: "lt-LT", timeZone: "FLE Standard Time", mailingHour: TimeSpan.FromHours(7), tutorial: true, preferences: "[3]");
            AddRowTheNewCodeCreated("created");

            RunCatchUp();

            Assert.That(Settings("created"), Is.EqualTo(new SettingsRow("lt-LT", "FLE Standard Time", TimeSpan.FromHours(7), true, "[3]")));
        }

        [Test]
        public void CatchUp_Keeps_What_The_New_Code_Set_On_An_On_Demand_Row()
        {
            AddUser("existing", culture: "en-US");
            Run(new AddUserSettings().UpOperations);
            AddUser("created", culture: "lt-LT", timeZone: "FLE Standard Time", tutorial: true, preferences: "[3]");
            AddRowTheNewCodeCreated("created", culture: "en-GB", preferences: "{\"sidebarFavorites\":[\"wall-7\"]}");

            RunCatchUp();

            Assert.That(Settings("created"), Is.EqualTo(new SettingsRow("en-GB", "FLE Standard Time", null, true, "{\"sidebarFavorites\":[\"wall-7\"]}")));
        }

        [Test]
        public void CatchUp_Leaves_A_User_The_New_Code_Created_Alone()
        {
            AddUser("existing", culture: "en-US");
            Run(new AddUserSettings().UpOperations);
            AddUser("created");
            AddRowTheNewCodeCreated("created", culture: "lt-LT", timeZone: "FLE Standard Time");

            RunCatchUp();

            Assert.That(Settings("created"), Is.EqualTo(new SettingsRow("lt-LT", "FLE Standard Time", null, false, null)));
        }

        [Test]
        public void CatchUp_Does_Nothing_More_When_Run_Again()
        {
            AddUser("untouched", culture: "en-US", timeZone: "UTC");
            AddUser("changed", culture: "en-US");
            Run(new AddUserSettings().UpOperations);
            Execute("UPDATE dbo.AspNetUsers SET [TimeZone] = 'FLE Standard Time' WHERE [Id] = 'untouched'");
            Execute("UPDATE dbo.UserSettings SET [CultureCode] = 'lt-LT', [Modified] = DATEADD(minute, 1, [Modified]) WHERE [UserId] = 'changed'");
            AddUser("created", culture: "lt-LT", tutorial: true);
            AddUser("onDemand", culture: "lt-LT", preferences: "[4]");
            AddRowTheNewCodeCreated("onDemand");

            RunCatchUp();
            var afterFirstRun = AllSettings();
            RunCatchUp();

            Assert.That(AllSettings(), Is.EqualTo(afterFirstRun));
        }

        private void RunCatchUp() => Execute(UserSettingsCatchUp.Sql);

        // Executes the migration's operations the way the migrator does.
        private void Run(IReadOnlyList<MigrationOperation> operations)
        {
            var connection = _context.GetService<IRelationalConnection>();

            foreach (var command in _context.GetService<IMigrationsSqlGenerator>().Generate(operations))
            {
                command.ExecuteNonQuery(connection);
            }
        }

        private void AddUser(string id, string culture = null, string timeZone = null, TimeSpan? mailingHour = null, bool tutorial = false, string preferences = null, bool anonymized = false)
        {
            Execute(
                @"INSERT dbo.AspNetUsers ([Id], [CultureCode], [TimeZone], [DailyMailingHour], [IsTutorialComplete], [WallWidgetPreferences], [IsAnonymized])
                  VALUES (@id, @culture, @timeZone, @mailingHour, @tutorial, @preferences, @anonymized)",
                ("@id", id), ("@culture", culture), ("@timeZone", timeZone), ("@mailingHour", mailingHour), ("@tutorial", tutorial), ("@preferences", preferences), ("@anonymized", anonymized));
        }

        // A row added after the backfill, the way GetOrCreateSettings or user creation adds one.
        private void AddRowTheNewCodeCreated(string userId, string culture = null, string timeZone = null, string preferences = null)
        {
            Execute(
                @"INSERT dbo.UserSettings ([UserId], [CultureCode], [TimeZone], [IsTutorialComplete], [Preferences], [Created], [Modified])
                  SELECT @userId, @culture, @timeZone, 0, @preferences, DATEADD(minute, 1, MIN([Created])), DATEADD(minute, 1, MIN([Created]))
                  FROM dbo.UserSettings",
                ("@userId", userId), ("@culture", culture), ("@timeZone", timeZone), ("@preferences", preferences));
        }

        private SettingsRow Settings(string userId)
        {
            using var command = new SqlCommand("SELECT [CultureCode], [TimeZone], [DailyMailingHour], [IsTutorialComplete], [Preferences] FROM dbo.UserSettings WHERE [UserId] = @userId", _connection);
            command.Parameters.AddWithValue("@userId", userId);
            using var reader = command.ExecuteReader();

            return reader.Read() ? ReadSettings(reader, 0) : null;
        }

        private List<(string UserId, SettingsRow Settings, DateTime Modified)> AllSettings()
        {
            using var command = new SqlCommand("SELECT [UserId], [CultureCode], [TimeZone], [DailyMailingHour], [IsTutorialComplete], [Preferences], [Modified] FROM dbo.UserSettings ORDER BY [UserId]", _connection);
            using var reader = command.ExecuteReader();
            var rows = new List<(string, SettingsRow, DateTime)>();

            while (reader.Read())
            {
                rows.Add((reader.GetString(0), ReadSettings(reader, 1), reader.GetDateTime(6)));
            }

            return rows;
        }

        private static SettingsRow ReadSettings(SqlDataReader reader, int first)
        {
            return new SettingsRow(
                reader.IsDBNull(first) ? null : reader.GetString(first),
                reader.IsDBNull(first + 1) ? null : reader.GetString(first + 1),
                reader.IsDBNull(first + 2) ? null : reader.GetTimeSpan(first + 2),
                reader.GetBoolean(first + 3),
                reader.IsDBNull(first + 4) ? null : reader.GetString(first + 4));
        }

        private string OldPreferences(string userId)
        {
            var value = Scalar<object>($"SELECT [WallWidgetPreferences] FROM dbo.AspNetUsers WHERE [Id] = '{userId}'");

            return value == DBNull.Value ? null : (string)value;
        }

        private T Scalar<T>(string sql)
        {
            using var command = new SqlCommand(sql, _connection);

            return (T)command.ExecuteScalar();
        }

        private void Execute(string sql, params (string Name, object Value)[] parameters) => Execute(_connection, sql, parameters);

        private static void Execute(SqlConnection connection, string sql, params (string Name, object Value)[] parameters)
        {
            using var command = new SqlCommand(sql, connection);

            foreach (var (name, value) in parameters)
            {
                command.Parameters.AddWithValue(name, value ?? DBNull.Value);
            }

            command.ExecuteNonQuery();
        }

        private sealed record SettingsRow(string CultureCode, string TimeZone, TimeSpan? DailyMailingHour, bool IsTutorialComplete, string Preferences);

        private sealed class SqlServerContext : DbContext
        {
            public SqlServerContext(DbContextOptions<SqlServerContext> options)
                : base(options)
            {
            }
        }
    }
}
