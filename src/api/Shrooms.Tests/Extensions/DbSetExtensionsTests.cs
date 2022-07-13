using NUnit.Framework;
using Shrooms.Tests.Mocks;
using Shrooms.Domain.Extensions;
using System.Data.Entity;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Dynamic;

namespace Shrooms.Tests.Extensions
{
    [TestFixture]
    [SuppressMessage("ReSharper", "ExpressionIsAlwaysNull")]
    public class DbSetExtensionsTests
    {
        private DbSet<MockModel> _mockDbSet;

        [SetUp]
        public void TestInitializer()
        {
            var mockDbContext = new MockDbContext();

            _mockDbSet = mockDbContext.Set<MockModel>();
        }

        [Test]
        public void Should_Return_Query_To_Order_By_First_Property_And_Order_By_Ascending_If_Property_Name_Is_Not_Provided()
        {
            // Arrange
            string propertyName = null;
            const string sortDirection = "asc";

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
            const string sortDirection = "desc";

            var expectedQuery = _mockDbSet.OrderBy("Id desc").ToString();

            // Act
            var actualQuery = _mockDbSet.OrderByPropertyName(propertyName, sortDirection).ToString();

            // Assert
            Assert.AreEqual(expectedQuery, actualQuery);
        }

        [Test]
        public void Should_Return_Query_To_Order_By_First_Property_And_Order_By_Ascending_If_Property_Name_And_Sort_Direction_Is_Not_Provided()
        {
            // Arrange
            string propertyName = null;
            string sortDirection = null;

            var expectedQuery = _mockDbSet.OrderBy("Id asc").ToString();

            // Act
            var actualQuery = _mockDbSet.OrderByPropertyName(propertyName, sortDirection).ToString();

            // Assert
            Assert.AreEqual(expectedQuery, actualQuery);
        }

        [Test]
        public void Should_Return_Query_To_Order_By_Specified_Property_And_Order_By_Ascending_If_Sort_Direction_Is_Not_Provided()
        {
            // Arrange
            const string propertyName = nameof(MockModel.Value);
            string sortDirection = null;

            var expectedQuery = _mockDbSet.OrderBy("Value asc").ToString();

            // Act
            var actualQuery = _mockDbSet.OrderByPropertyName(propertyName, sortDirection).ToString();

            // Assert
            Assert.AreEqual(expectedQuery, actualQuery);
        }

        [Test]
        public void Should_Return_Query_To_Order_By_Specified_Property_And_Order_By_Provided_Sort_Direction()
        {
            // Arrange
            const string propertyName = nameof(MockModel.Value);
            const string sortDirection = "asc";

            var expectedQuery = _mockDbSet.OrderBy("Value asc").ToString();

            // Act
            var actualQuery = _mockDbSet.OrderByPropertyName(propertyName, sortDirection).ToString();

            // Assert
            Assert.AreEqual(expectedQuery, actualQuery);
        }
    }
}
