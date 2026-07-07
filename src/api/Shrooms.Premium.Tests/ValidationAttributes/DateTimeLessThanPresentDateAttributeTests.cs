using NUnit.Framework;
using NUnit.Framework.Legacy;
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
            var testValue = DateTime.UtcNow;
            var attribute = new DateTimeLessThanPresentDateAttribute();

            // Act
            var result = attribute.IsValid(testValue);

            // Assert
            ClassicAssert.IsTrue(result);
        }

        [Test]
        public void IsValid_WhenDateTimeIsGreaterThanDateTimeUtcNow_ReturnsFalse()
        {
            // Arrange
            var testValue = DateTime.UtcNow.AddYears(1);
            var attribute = new DateTimeLessThanPresentDateAttribute();

            // Act
            var result = attribute.IsValid(testValue);

            // Assert
            ClassicAssert.IsFalse(result);
        }
    }
}
