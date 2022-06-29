using NUnit.Framework;
using Shrooms.Tests.Mocks;
using Shrooms.Domain.Extensions;
using System.Data.Entity;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Shrooms.Tests.Extensions
{
    [TestFixture]
    public class DbSetExtensionsTests
    {
        private class MockWithoutProperties
        {
        }

        private DbSet<MockModel> _mockDbSet;
        private DbSet<MockWithoutProperties> _mockWithoutPropertiesDbSet;

        [SetUp]
        public void TestInitializer()
        {
            var mockDbContext = new MockDbContext();

            _mockDbSet = mockDbContext.Set<MockModel>();
            _mockWithoutPropertiesDbSet = mockDbContext.Set<MockWithoutProperties>();
        }

        [Test]
        public void Should_Return_Query_To_Sort_By_First_Property_If_Property_Name_Is_Not_Provided()
        {
            // Arrange ? linq query and test equality?
            string propertyName = null;
            var sortDirection = "asc";

            // Act
            var result = _mockDbSet.OrderByPropertyName(propertyName, sortDirection);

            // Assert
            Assert.IsInstanceOf<IQueryable<MockModel>>(result);
        }

        [Test]
        public void Should_Return_Query_To_Sort_By_First_Property_If_Sort_Direction_Is_Not_Provided()
        {
            // Arrange
            var propertyName = nameof(MockModel.Id);
            string sortDirection = null;

            // Act
            var result = _mockDbSet.OrderByPropertyName(propertyName, sortDirection);

            // Assert
            Assert.IsInstanceOf<IQueryable<MockModel>>(result);
        }

        [Test]
        public void Should_Return_Query_To_Sort_By_First_Property_If_Model_Does_Not_Have_Specified_Property()
        {
            // Arrange
            string propertyName = null;
            string sortDirection = null;

            // Act
            var result = _mockDbSet.OrderByPropertyName(propertyName, sortDirection);

            // Assert
            Assert.IsInstanceOf<IQueryable<MockModel>>(result);
        }

        [Test]
        public void Should_Return_Query_To_Sort_By_Specified_Property_Name()
        {
            // Arrange
            var propertyName = nameof(MockModel.Id);
            var sortDirection = "asc";

            // Act
            var result = _mockDbSet.OrderByPropertyName(propertyName, sortDirection);

            // Assert
            Assert.IsInstanceOf<IQueryable<MockModel>>(result);
        }

        [Test]
        public void Should_Throw_If_Model_Does_Not_Have_Any_Properties()
        {
            Assert.Throws<ValidationException>(() => _mockWithoutPropertiesDbSet.OrderByPropertyName("Id", "ASC"));
        }

        [Test]
        public void Should_Throw_If_Sort_Direction_Does_Not_Exist()
        {
            // Arrange
            var propertyName = nameof(MockModel.Id);
            var invalidSortDirection = string.Empty;

            // Assert
            Assert.Throws<ValidationException>(() => _mockDbSet.OrderByPropertyName(propertyName, invalidSortDirection));
        }
    }
}
