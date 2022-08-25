using NUnit.Framework;
using Shrooms.Presentation.WebViewModels.ValidationAttributes.Walls;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Shrooms.Tests.ValidationAttributes
{
    [TestFixture]
    public class HasImageOrMessageBodyAttributeTests
    {
        private class Mock
        {
            public int Number { get; set; }

            public string MessageBody { get; set; }

            public string PictureId { get; set; }
        }

        [Test]
        public void IsValid_MessageBodyIsNotString_ThrowsArgumentException()
        {
            // Arrange
            var model = new Mock();
            var validationContext = new ValidationContext(model);
            string testValue = null;

            var attribute = new HasImageOrMessageBodyAttribute(nameof(Mock.Number), nameof(Mock.PictureId));

            // Assert
            Assert.Throws<ArgumentException>(() => attribute.GetValidationResult(testValue, validationContext));
        }

        [Test]
        public void IsValid_ImagesIsNotIEnumerable_ThrowsArgumentException()
        {
            // Arrange
            var model = new Mock();
            var validationContext = new ValidationContext(model);
            var testValue = string.Empty;

            var attribute = new HasImageOrMessageBodyAttribute(nameof(Mock.MessageBody), nameof(Mock.PictureId));

            // Assert
            Assert.Throws<ArgumentException>(() => attribute.GetValidationResult(testValue, validationContext));
        }

        [Test]
        public void IsValid_MessageBodyIsNotEmptyAndImagesIsNull_ReturnsTrue()
        {
            // Arrange
            var model = new Mock();

            model.MessageBody = "not empty";

            var validationContext = new ValidationContext(model);
            IEnumerable<string> testValue = null;

            var attribute = new HasImageOrMessageBodyAttribute(nameof(Mock.MessageBody), nameof(Mock.PictureId));

            // Act
            var actual = attribute.GetValidationResult(testValue, validationContext);

            // Assert
            Assert.AreEqual(ValidationResult.Success, actual);
        }

        [Test]
        public void IsValid_MessageBodyIsNullAndImagesContainNullValues_ReturnsFalse()
        {
            // Arrange
            var model = new Mock();
            var validationContext = new ValidationContext(model);
            var testValue = new List<string>
            {
                "image",
                null,
                "image"
            };

            var attribute = new HasImageOrMessageBodyAttribute(nameof(Mock.MessageBody), nameof(Mock.PictureId));

            // Act
            var actual = attribute.GetValidationResult(testValue, validationContext);

            // Assert
            Assert.IsNotNull(actual.ErrorMessage);
        }

        [Test]
        public void IsValid_MessageBodyIsNullAndImagesPropertyIsEmpty_ReturnsFalse()
        {
            // Arrange
            var model = new Mock();
            var validationContext = new ValidationContext(model);
            var testValue = new List<string>();

            var attribute = new HasImageOrMessageBodyAttribute(nameof(Mock.MessageBody), nameof(Mock.PictureId));

            // Act
            var actual = attribute.GetValidationResult(testValue, validationContext);

            // Assert
            Assert.IsNotNull(actual.ErrorMessage);
        }

        [Test]
        public void IsValid_MessageBodyIsNullAndThereAreImages_ReturnsTrue()
        {
            // Arrange
            var model = new Mock();
            var validationContext = new ValidationContext(model);
            var testValue = new List<string>
            {
                "image",
                "image"
            };

            var attribute = new HasImageOrMessageBodyAttribute(nameof(Mock.MessageBody), nameof(Mock.PictureId));

            // Act
            var actual = attribute.GetValidationResult(testValue, validationContext);

            // Assert
            Assert.AreEqual(ValidationResult.Success, actual);
        }

        [Test]
        public void IsValid_MessageBodyIsNotNullAndThereAreImages_ReturnsTrue()
        {
            // Arrange
            var model = new Mock();

            model.MessageBody = "not empty";

            var validationContext = new ValidationContext(model);
            var testValue = new List<string>
            {
                "image",
                "image"
            };

            var attribute = new HasImageOrMessageBodyAttribute(nameof(Mock.MessageBody), nameof(Mock.PictureId));

            // Act
            var actual = attribute.GetValidationResult(testValue, validationContext);

            // Assert
            Assert.AreEqual(ValidationResult.Success, actual);
        }
    }
}
