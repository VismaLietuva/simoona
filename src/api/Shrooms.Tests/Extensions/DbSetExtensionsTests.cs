using NUnit.Framework;
using Shrooms.Tests.Mocks;
using Shrooms.Domain.Extensions;
using System.Data.Entity;
using System.ComponentModel.DataAnnotations;
using System.Linq.Dynamic;
using System.Linq;
using System.Data.Entity.Core.Objects;
using System.Text;

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
        public void Should_Return_Query_To_Order_By_First_Property_And_Order_By_Ascending_If_Property_Name_Is_Not_Provided()
        {
            // Arrange
            string propertyName = null;
            var sortDirection = "asc";
            
            var expectedQuery = _mockDbSet.OrderBy("Id asc").ToString();

            // Act
            var actualQuery = _mockDbSet.OrderByPropertyName(propertyName, sortDirection).ToString();

            // Assert
            Assert.AreEqual(expectedQuery, actualQuery);
        }

        [Test]
        public void Should_Return_Query_To_Order_By_First_Property_And_Order_By_Descending_If_Property_Name_Is_Not_Provided()
        {
            // Arrange
            string propertyName = null;
            var sortDirection = "desc";

            var expectedQuery = _mockDbSet.OrderBy("Id desc").ToString();

            // Act
            var actualQuery = _mockDbSet.OrderByPropertyName(propertyName, sortDirection).ToString();

            // Assert
            Assert.AreEqual(expectedQuery, actualQuery);
        }

        [Test]
        public void Should_Return_Query_To_Order_By_First_Property_And_Order_By_Descending_If_Property_Name_And_Sort_Direction_Is_Not_Provided()
        {
            // Arrange
            string propertyName = null;
            string sortDirection = null;

            var expectedQuery = _mockDbSet.OrderBy("Id desc").ToString();

            // Act
            var actualQuery = _mockDbSet.OrderByPropertyName(propertyName, sortDirection).ToString();

            // Assert
            Assert.AreEqual(expectedQuery, actualQuery);
        }

        [Test]
        public void Should_Return_Query_To_Order_By_Specified_Property_And_Order_By_Descending_If_Sort_Direction_Is_Not_Provided()
        {
            // Arrange
            var propertyName = nameof(MockModel.Value);
            string sortDirection = null;

            var expectedQuery = _mockDbSet.OrderBy("Value desc").ToString();

            // Act
            var actualQuery = _mockDbSet.OrderByPropertyName(propertyName, sortDirection).ToString();

            // Assert
            Assert.AreEqual(expectedQuery, actualQuery);
        }

        [Test]
        public void Should_Return_Query_To_Order_By_Specified_Property_And_Order_By_Provided_Sort_Direction()
        {
            // Arrange
            var propertyName = nameof(MockModel.Value);
            var sortDirection = "asc";

            var expectedQuery = _mockDbSet.OrderBy("Value asc").ToString();

            // Act
            var actualQuery = _mockDbSet.OrderByPropertyName(propertyName, sortDirection).ToString();

            // Assert
            Assert.AreEqual(expectedQuery, actualQuery);
        }

        [Test]
        public void Should_Throw_If_Model_Does_Not_Have_Any_Properties()
        {
            Assert.Throws<ValidationException>(() => _mockWithoutPropertiesDbSet.OrderByPropertyName("Id", "ASC"));
        }
    }
}
