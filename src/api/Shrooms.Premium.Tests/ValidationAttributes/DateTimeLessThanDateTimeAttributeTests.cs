using NUnit.Framework;
using Shrooms.Premium.Presentation.WebViewModels.ValidationAttributes;
using System;
using System.ComponentModel.DataAnnotations;

namespace Shrooms.Premium.Tests.ValidationAttributes
{
    [TestFixture]
    public class DateTimeLessThanDateTimeAttributeTests
    {
        private class Mock
        {
            public int NotDate { get; set; }

            public DateTime Date { get; set; }
        }

        [Test]
        public void IsValid_WhenComparisonValueIsNotDateTime_Throws()
        {
            // Arrange
            var model = new Mock();
            var validationContext = new ValidationContext(model);
            var testValue = DateTime.UtcNow;
            var attribute = new DateTimeLessThanDateTimeAttribute("NotDate");

            // Assert
            Assert.Throws<ArgumentException>(() => attribute.GetValidationResult(testValue, validationContext));
        }

        [Test]
        public void IsValid_WhenConsumerValueIsNotDateTime_Throws()
        {
            // Arrange
            const int testValue = 10;
            var attribute = new DateTimeLessThanDateTimeAttribute("Date");

            // Assert
            Assert.Throws<ArgumentException>(() => attribute.IsValid(testValue));
        }

        [Test]
        public void IsValid_WhenComparisonValueIsNotFound_Throws()
        {
            // Arrange
            var model = new Mock();
            var validationContext = new ValidationContext(model);
            var testValue = DateTime.UtcNow;
            var attribute = new DateTimeLessThanDateTimeAttribute("PropertyIsNotFound");

            // Assert
            Assert.Throws<ArgumentException>(() => attribute.GetValidationResult(testValue, validationContext));
        }

        [Test]
        public void IsValid_WhenConsumerIsNull_ReturnsTrue()
        {
            // Arrange
            DateTime? testValue = null;
            var attribute = new DateTimeLessThanDateTimeAttribute("Date");

            // Act
            var result = attribute.IsValid(testValue);

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void IsValid_WhenConsumerDateTimeIsLessThanComparisonValueDateTime_ReturnsTrue()
        {
            // Arrange
            var model = new Mock
            {
                Date = DateTime.UtcNow.AddYears(1),
            };

            var validationContext = new ValidationContext(model);
            var testValue = DateTime.UtcNow;
            var attribute = new DateTimeLessThanDateTimeAttribute("Date");

            // Act
            var result = attribute.GetValidationResult(testValue, validationContext);


            // Assert
            Assert.AreEqual(ValidationResult.Success, result);
        }

        [Test]
        public void IsValid_WhenConsumerDateTimeIsGreaterOrEqualThanComparisonValueDateTime_ReturnsFalse()
        {
            // Arrange
            var model = new Mock
            {
                Date = DateTime.UtcNow,
            };

            var validationContext = new ValidationContext(model);
            var testValue = DateTime.UtcNow.AddYears(1);
            var attribute = new DateTimeLessThanDateTimeAttribute("Date");

            // Act
            var result = attribute.GetValidationResult(testValue, validationContext);
            
            // Assert
            Assert.IsNotNull(result.ErrorMessage);
        }
    }
}
