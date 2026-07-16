using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using NUnit.Framework;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.Exceptions;
using Shrooms.DataLayer.EntityModels.Models.Emoji;
using Shrooms.Domain.ServiceValidators.Validators.Emoji;
using Shrooms.Tests.Extensions;

namespace Shrooms.Tests.DomainService
{
    [TestFixture]
    public class CustomEmojiValidatorTests
    {
        private DbSet<CustomEmoji> _customEmojisDbSet;
        private ICustomEmojiValidator _validator;

        [SetUp]
        public void TestInitializer()
        {
            var uow = Substitute.For<IUnitOfWork2>();

            _customEmojisDbSet = uow.MockDbSetForAsync<CustomEmoji>();

            _validator = new CustomEmojiValidator(uow);
        }

        [TestCase("party-parrot")]
        [TestCase("smile_2")]
        [TestCase("a")]
        public void Should_Not_Throw_For_Valid_Name(string name)
        {
            Assert.DoesNotThrow(() => _validator.CheckNameFormat(name));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("UPPER")]
        [TestCase("has space")]
        [TestCase(":smile:")]
        [TestCase("ąžuolas")]
        [TestCase("123456789012345678901234567890123456789012345678901")]
        public void Should_Throw_If_Name_Format_Is_Invalid(string name)
        {
            var ex = Assert.Throws<ValidationException>(() => _validator.CheckNameFormat(name));

            Assert.That(ex.ErrorCode, Is.EqualTo(ErrorCodes.InvalidCustomEmojiName));
        }

        [Test]
        public void Should_Throw_If_Name_Is_Taken()
        {
            var emojis = new List<CustomEmoji>
            {
                new()
                    { Id = 1, Name = "party-parrot", OrganizationId = 2, IsDeleted = false }
            };
            _customEmojisDbSet.SetDbSetDataForAsync(emojis.AsQueryable());

            var ex = Assert.ThrowsAsync<ValidationException>(async () =>
                await _validator.CheckIfNameIsTakenAsync("party-parrot", 2));

            Assert.That(ex.ErrorCode, Is.EqualTo(ErrorCodes.DuplicatesIntolerable));
        }

        [Test]
        public void Should_Not_Throw_If_Name_Was_Soft_Deleted()
        {
            var emojis = new List<CustomEmoji>
            {
                new()
                    { Id = 1, Name = "party-parrot", OrganizationId = 2, IsDeleted = true }
            };
            _customEmojisDbSet.SetDbSetDataForAsync(emojis.AsQueryable());

            Assert.DoesNotThrowAsync(async () => await _validator.CheckIfNameIsTakenAsync("party-parrot", 2));
        }

        [Test]
        public void Should_Not_Throw_If_Name_Is_Taken_In_Other_Organization()
        {
            var emojis = new List<CustomEmoji>
            {
                new()
                    { Id = 1, Name = "party-parrot", OrganizationId = 3, IsDeleted = false }
            };
            _customEmojisDbSet.SetDbSetDataForAsync(emojis.AsQueryable());

            Assert.DoesNotThrowAsync(async () => await _validator.CheckIfNameIsTakenAsync("party-parrot", 2));
        }

    }
}
