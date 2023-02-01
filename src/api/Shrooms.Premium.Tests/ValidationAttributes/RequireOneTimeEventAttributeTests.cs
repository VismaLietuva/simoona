using NUnit.Framework;
using Shrooms.DataLayer.EntityModels.Models.Events;
using Shrooms.Premium.Presentation.WebViewModels.ValidationAttributes.Events;
using System;
using System.ComponentModel.DataAnnotations;

namespace Shrooms.Premium.Tests.ValidationAttributes
{
    [TestFixture]
    public class RequireOneTimeEventAttributeTests
    {
        private class Mock
        {
            public int? Number { get; set; }

            public EventRecurrenceOptions Recurrence { get; set; }
        }

        [Test]
        public void IsValid_RecurrenceIsNoneAndNumberIsSet_ReturnsTrue()
        {
            // Arrange
            var model = new Mock
            {
                Number = 1,
                Recurrence = EventRecurrenceOptions.None
            };
            var context = new ValidationContext(model);
            var attribute = new RequireOneTimeEventForCollectionAttribute(nameof(Mock.Recurrence));

            // Act
            var result = attribute.GetValidationResult(model.Number, context);

            // Assert
            Assert.AreEqual(ValidationResult.Success, result);
        }

        [Test]
        public void IsValid_RecurrenceIsNoneAndNumberIsNotSet_ReturnsTrue()
        {
            // Arrange
            var model = new Mock
            {
                Number = null,
                Recurrence = EventRecurrenceOptions.None
            };
            var context = new ValidationContext(model);
            var attribute = new RequireOneTimeEventForCollectionAttribute(nameof(Mock.Recurrence));

            // Act
            var result = attribute.GetValidationResult(model.Number, context);

            // Assert
            Assert.AreEqual(ValidationResult.Success, result);
        }

        [Test]
        public void IsValid_RecurrenceIsNotNoneAndNumberIsNotSet_ReturnsTrue()
        {
            // Arrange
            var model = new Mock
            {
                Number = null,
                Recurrence = EventRecurrenceOptions.EveryTwoWeeks
            };
            var context = new ValidationContext(model);
            var attribute = new RequireOneTimeEventForCollectionAttribute(nameof(Mock.Recurrence));

            // Act
            var result = attribute.GetValidationResult(model.Number, context);

            // Assert
            Assert.AreEqual(ValidationResult.Success, result);
        }

        [Test]
        public void IsValid_AttributeCannotFindSpecifiedValue_ThrowsException()
        {
            // Arrange
            var model = new Mock
            {
                Number = null,
                Recurrence = EventRecurrenceOptions.None
            };
            var context = new ValidationContext(model);
            var attribute = new RequireOneTimeEventForCollectionAttribute("Random");

            // Assert
            Assert.Throws<ArgumentException>(() => attribute.GetValidationResult(model.Number, context));
        }

        [Test]
        public void IsValid_AttributeIsNotEventRecurrenceOption_ThrowsException()
        {
            // Arrange
            var model = new Mock
            {
                Number = null,
                Recurrence = EventRecurrenceOptions.None
            };
            var context = new ValidationContext(model);
            var attribute = new RequireOneTimeEventForCollectionAttribute(nameof(Mock.Number));

            // Assert
            Assert.Throws<ArgumentException>(() => attribute.GetValidationResult(model.Number, context));
        }
    }
}
