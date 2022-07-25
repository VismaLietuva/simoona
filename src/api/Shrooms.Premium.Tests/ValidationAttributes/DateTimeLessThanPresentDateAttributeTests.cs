using NUnit.Framework;
using Shrooms.Premium.Presentation.WebViewModels.ValidationAttributes;
using System;

namespace Shrooms.Premium.Tests.ValidationAttributes
{
    [TestFixture]
    public class DateTimeLessThanPresentDateAttributeTests
    {
        [Test]
        public void IsValid_WhenDateTimeIsLessOrEqualThanDateTimeUtcNow_ReturnsTrue()
        {
            // Arrange
            DateTime testValue = DateTime.UtcNow;
            var attribute = new DateTimeLessThanPresentDateAttribute();

            // Act
            var result = attribute.IsValid(testValue);

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void IsValid_WhenDateTimeIsGreaterThanDateTimeUtcNow_ReturnsFalse()
        {
            // Arrange
            DateTime testValue = DateTime.UtcNow.AddYears(1);
            var attribute = new DateTimeLessThanPresentDateAttribute();

            // Act
            var result = attribute.IsValid(testValue);

            // Assert
            Assert.IsFalse(result);
        }
    }
}
