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

        [TestCase(2627)]
        [TestCase(2601)]
        public void SaveUserSettingsAsync_Duplicate_Key_Reports_A_Concurrency_Error(int sqlErrorNumber)
        {
            var duplicate = SqlExceptions.WithNumber(sqlErrorNumber, "Violation of PRIMARY KEY constraint 'PK_UserSettings'");
            _uow.SaveChangesAsync("user").ThrowsAsync(new DbUpdateException("Duplicate", duplicate));

            var ex = Assert.ThrowsAsync<ValidationException>(() => _uow.SaveUserSettingsAsync("user"));

            Assert.That(ex.ErrorCode, Is.EqualTo(ErrorCodes.ConcurrencyError));
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
