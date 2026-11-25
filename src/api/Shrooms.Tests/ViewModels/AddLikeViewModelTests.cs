using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using NUnit.Framework;
using Shrooms.Contracts.Enums;
using Shrooms.Contracts.ViewModels.Wall.Likes;

namespace Shrooms.Tests.ViewModels
{
    [TestFixture]
    public class AddLikeViewModelTests
    {
        [Test]
        public void Should_Leave_Overlong_Emoji_Name_For_The_Service_To_Reject()
        {
            var model = new AddLikeViewModel { Id = 1, Type = LikeTypeEnum.Emoji, EmojiName = new string('a', 61) };

            var isValid = Validator.TryValidateObject(model, new ValidationContext(model), new List<ValidationResult>(), validateAllProperties: true);

            Assert.That(isValid, Is.True);
        }
    }
}
