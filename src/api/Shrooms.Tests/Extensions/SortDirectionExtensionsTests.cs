using NUnit.Framework;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.Enums;
using Shrooms.Domain.Extensions;
using System;

namespace Shrooms.Tests.Extensions
{
    [TestFixture]
    public class SortDirectionExtensionsTests
    {
        [Test]
        public void GetString_DirectionAscending_ReturnsCorrectString()
        {
            // Arrange
            const string expected = SortDirectionConstants.Ascending;

            // Act
            var actual = SortDirection.Ascending.GetString();

            // Assert
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void GetString_DirectionDescending_ReturnsCorrectString()
        {
            // Arrange
            const string expected = SortDirectionConstants.Descending;

            // Act
            var actual = SortDirection.Descending.GetString();

            // Assert
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void GetString_InvalidDirection_ThrowsArgumentException()
        {
            // Assert
            Assert.Throws<ArgumentException>(() => ((SortDirection)int.MaxValue).GetString());
        }
    }
}
