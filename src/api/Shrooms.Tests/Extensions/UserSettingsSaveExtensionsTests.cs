using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.Exceptions;
using Shrooms.Domain.Extensions;

namespace Shrooms.Tests.Extensions
{
    [TestFixture]
    public class UserSettingsSaveExtensionsTests
    {
        private IUnitOfWork2 _uow;

        [SetUp]
        public void TestInitializer()
        {
            _uow = Substitute.For<IUnitOfWork2>();
        }

        [Test]
        public async Task SaveUserSettingsAsync_Saves_As_The_User()
        {
            await _uow.SaveUserSettingsAsync("user");

            await _uow.Received(1).SaveChangesAsync("user");
        }

        [Test]
        public void SaveUserSettingsAsync_Duplicate_Settings_Row_Reports_A_Concurrency_Error()
        {
            var duplicate = SqlExceptions.WithNumber(2627, "Violation of PRIMARY KEY constraint 'PK_UserSettings'. Cannot insert duplicate key in object 'dbo.UserSettings'.");
            _uow.SaveChangesAsync("user").ThrowsAsync(new DbUpdateException("Duplicate", duplicate));

            var ex = Assert.ThrowsAsync<ValidationException>(() => _uow.SaveUserSettingsAsync("user"));

            Assert.That(ex.ErrorCode, Is.EqualTo(ErrorCodes.ConcurrencyError));
        }

        [TestCase(2627, "Violation of PRIMARY KEY constraint 'PK_WallMembers'. Cannot insert duplicate key in object 'dbo.WallMembers'.")]
        [TestCase(2601, "Cannot insert duplicate key row in object 'dbo.KudosLogs' with unique index 'IX_KudosLogs'.")]
        public void SaveUserSettingsAsync_Duplicate_In_Another_Table_Is_Rethrown(int sqlErrorNumber, string message)
        {
            var duplicate = SqlExceptions.WithNumber(sqlErrorNumber, message);
            _uow.SaveChangesAsync("user").ThrowsAsync(new DbUpdateException("Duplicate", duplicate));

            Assert.ThrowsAsync<DbUpdateException>(() => _uow.SaveUserSettingsAsync("user"));
        }

        [Test]
        public void SaveUserSettingsAsync_Other_Save_Errors_Are_Rethrown()
        {
            var deadlock = SqlExceptions.WithNumber(1205, "Transaction was deadlocked");
            _uow.SaveChangesAsync("user").ThrowsAsync(new DbUpdateException("Deadlock", deadlock));

            Assert.ThrowsAsync<DbUpdateException>(() => _uow.SaveUserSettingsAsync("user"));
        }
    }
}
