using NUnit.Framework;
using Shrooms.Infrastructure.Sorting;
using Shrooms.Domain.Extensions;
using Shrooms.Contracts.Enums;
using Shrooms.Contracts.Constants;

namespace Shrooms.Tests.Extensions
{
    [TestFixture]
    public class SortableExtensionsTests
    {
        [Test]
        public void AddSortablePropertiesToStart_ValidValues_ReturnsCorrectlyFormattedString()
        {
            // Arrange
            var sortable = new Sortable();
            const string classPropertyName = "Name";

            var expected = $"{classPropertyName} {SortDirectionConstants.Descending};";

            // Act
            var actual = sortable.AddSortablePropertiesToStart((classPropertyName, SortDirection.Descending));

            // Assert
            Assert.AreEqual(expected, actual.SortByProperties);
        }
    }
}
