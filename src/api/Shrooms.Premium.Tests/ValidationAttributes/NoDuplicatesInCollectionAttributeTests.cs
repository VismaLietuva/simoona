using NUnit.Framework;
using Shrooms.Premium.Presentation.WebViewModels.ValidationAttributes;
using System;
using System.Collections.Generic;

namespace Shrooms.Premium.Tests.ValidationAttributes
{
    [TestFixture]
    public class NoDuplicatesInCollectionAttributeTests
    {
        private class InnerMock
        {
            public int Value { get; set; }
        }

        private class Mock
        {
            public List<int> ValueTypes { get; set; }

            public List<InnerMock> ReferenceTypes { get; set; }

            public int SingleValue { get; set; }
        }

        [Test]
        public void IsValid_PropertyIsNull_ReturnsTrue()
        {
            // Arrange
            var attribute = new NoDuplicatesInCollectionAttribute(nameof(Mock.ValueTypes));
            var testValue = (List<int>)null;

            // Act
            var actual = attribute.IsValid(testValue);

            // Assert
            Assert.IsTrue(actual);
        }

        [Test]
        public void IsValid_PropertyIsNotACollection_Throws()
        {
            // Arrange
            var attribute = new NoDuplicatesInCollectionAttribute(nameof(Mock.SingleValue));
            const int testValue = 10;

            // Assert
            Assert.Throws<ArgumentException>(() => attribute.IsValid(testValue));
        }

        [Test]
        public void IsValid_CollectionIsEmpty_ReturnsTrue()
        {
            // Arrange
            var attribute = new NoDuplicatesInCollectionAttribute(nameof(Mock.ValueTypes));
            var testValue = new List<int>();

            // Act
            var actual = attribute.IsValid(testValue);

            // Assert
            Assert.IsTrue(actual);
        }

        [Test]
        public void IsValid_CollectionContainsReferenceTypes_Throws()
        {
            // Arrange
            var attribute = new NoDuplicatesInCollectionAttribute();

            var testValue = new List<InnerMock>
            {
                new InnerMock()
            };

            // Assert
            Assert.Throws<ArgumentException>(() => attribute.IsValid(testValue));
        }

        [Test]
        public void IsValid_DuplicateValuesWithoutComparisonProperty_ReturnsFalse()
        {
            // Arrange
            var attribute = new NoDuplicatesInCollectionAttribute();

            var testValue = new List<int> { 1, 2, 3, 1 };

            // Act
            var actual = attribute.IsValid(testValue);

            // Assert
            Assert.IsFalse(actual);
        }

        [Test]
        public void IsValid_UniqueValuesWithoutComparisonProperty_ReturnsTrue()
        {
            // Arrange
            var attribute = new NoDuplicatesInCollectionAttribute();

            var testValue = new List<int> { 1, 2, 3 };

            // Act
            var actual = attribute.IsValid(testValue);

            // Assert
            Assert.IsTrue(actual);
        }

        [Test]
        public void IsValid_DuplicateValuesWithComparisonProperty_ReturnsFalse()
        {
            // Arrange
            var attribute = new NoDuplicatesInCollectionAttribute(nameof(InnerMock.Value));
            var testValue = new List<InnerMock>
            {
                new InnerMock
                {
                    Value = 1
                },

                new InnerMock
                {
                    Value = 1
                }
            };

            // Act
            var actual = attribute.IsValid(testValue);

            // Assert
            Assert.IsFalse(actual);
        }

        [Test]
        public void IsValid_UniqueValuesWithComparisonProperty_ReturnsTrue()
        {
            // Arrange
            var attribute = new NoDuplicatesInCollectionAttribute(nameof(InnerMock.Value));
            var testValue = new List<InnerMock>
            {
                new InnerMock
                {
                    Value = 1
                },

                new InnerMock
                {
                    Value = 2
                }
            };

            // Act
            var actual = attribute.IsValid(testValue);

            // Assert
            Assert.IsTrue(actual);
        }
    }
}
