using NUnit.Framework;
using Shrooms.Contracts.Enums;
using Shrooms.Premium.Presentation.WebViewModels.ValidationAttributes.Lotteries;
using System;

namespace Shrooms.Premium.Tests.ValidationAttributes
{
    [TestFixture]
    public class ValidLotteryCreationStatusAttributeTests
    {
        [Test]
        public void IsValid_ValueIsNull_ReturnsTrue()
        {
            // Arrange
            var attribute = new ValidLotteryCreationStatusAttribute();
            var testValue = (LotteryStatus?)null;

            // Act
            var actual = attribute.IsValid(testValue);

            // Assert
            Assert.IsTrue(actual);
        }

        [Test]
        public void IsValid_ValueIsNotOfTypeLotteryStatus_Throws()
        {
            // Arrange
            var attribute = new ValidLotteryCreationStatusAttribute();
            var testValue = 10;

            // Assert
            Assert.Throws<ArgumentException>(() => attribute.IsValid(testValue));
        }

        [Test]
        public void IsValid_ValueIsNotDefined_ReturnsFalse()
        {
            // Arrange
            var attribute = new ValidLotteryCreationStatusAttribute();
            var testValue = (LotteryStatus)int.MaxValue;

            // Act
            var actual = attribute.IsValid(testValue);

            // Assert
            Assert.IsFalse(actual);
        }

        [Test]
        public void IsValid_ValueIsNotStartedAndNotDrafted_ReturnsFalse()
        {
            // Arrange
            var attribute = new ValidLotteryCreationStatusAttribute();
            var testValue = LotteryStatus.Ended;

            // Act
            var actual = attribute.IsValid(testValue);

            // Assert
            Assert.IsFalse(actual);
        }

        [Test]
        public void IsValid_ValueIsStartedOrDrafted_ReturnsTrue()
        {
            // Arrange
            var attribute = new ValidLotteryCreationStatusAttribute();
            var testValue = LotteryStatus.Started;

            // Act
            var actual = attribute.IsValid(testValue);

            // Assert
            Assert.IsTrue(actual);
        }
    }
}
