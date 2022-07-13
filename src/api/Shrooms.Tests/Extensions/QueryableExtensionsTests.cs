using NUnit.Framework;
using Shrooms.Tests.Mocks;
using Shrooms.Domain.Extensions;
using System.Data.Entity;
using System.Linq.Dynamic;
using System.Linq;

namespace Shrooms.Tests.Extensions
{
    [TestFixture]
    public class QueryableExtensionsTests
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
            var propertyName = nameof(MockModel.Value);
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
            var propertyName = nameof(MockModel.Value);
            var sortDirection = "asc";

            var expectedQuery = _mockDbSet.OrderBy("Value asc").ToString();

            // Act
            var actualQuery = _mockDbSet.OrderByPropertyName(propertyName, sortDirection).ToString();

            // Assert
            Assert.AreEqual(expectedQuery, actualQuery);
        }

        [TestCase("random")]
        [TestCase("random desc;")]
        [TestCase("Id value; Value id;")]
        [TestCase("")]
        [TestCase(";;")]
        [TestCase("Id desc, Value asc;")]
        public void Should_Return_Query_To_Order_By_First_Property_And_Order_By_Ascending_If_Invalid_String(string sortByProperties)
        {
            // Arrange
            var expectedQuery = _mockDbSet.OrderBy("Id asc").ToString();

            // Act
            var actualQuery = _mockDbSet.OrderByPropertyNames(sortByProperties).ToString();

            // Assert
            Assert.AreEqual(expectedQuery, actualQuery);
        }


        [TestCase()]
        public void Should_Return_Query_To_Order_By_Specified_Multiple_Properties()
        {
            // Arrange
            var sortByProperties = "Value asc;Id desc;";

            var expectedQuery = _mockDbSet.OrderBy("Value asc, Id desc").ToString();

            // Act
            var actualQuery = _mockDbSet.OrderByPropertyNames(sortByProperties).ToString();

            // Assert
            Assert.AreEqual(expectedQuery, actualQuery);
        }
    }
}
