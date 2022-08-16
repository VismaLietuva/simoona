using NUnit.Framework;
using Shrooms.Tests.Mocks;
using Shrooms.Domain.Extensions;
using System.Data.Entity;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Dynamic;
using Shrooms.Infrastructure.Sorting;

namespace Shrooms.Tests.Extensions
{
    [TestFixture]
    [SuppressMessage("ReSharper", "ExpressionIsAlwaysNull")]
    public class QueryableExtensionsTests
    {
        private DbSet<MockModel> _mockDbSet;

        [SetUp]
        public void TestInitializer()
        {
            var mockDbContext = new MockDbContext();

            _mockDbSet = mockDbContext.Set<MockModel>();
        }

        [TestCase("random")]
        [TestCase("random desc;")]
        [TestCase("Id value; Value id;")]
        [TestCase("")]
        [TestCase(";;")]
        [TestCase("Id desc, Value asc;")]
        public void OrderByPropertyNames_WhenStringIsInvalidFormat_OrdersByFirstPropertyAndAscendingDirection(string sortByProperties)
        {
            // Arrange
            var sortable = new Sortable
            {
                SortByProperties = sortByProperties
            };

            var expectedQuery = _mockDbSet.OrderBy("Id asc").ToString();

            // Act
            var actualQuery = _mockDbSet.OrderByPropertyNames(sortable).ToString();

            // Assert
            Assert.AreEqual(expectedQuery, actualQuery);
        }


        [TestCase()]
        public void Should_Return_Query_To_Order_By_Specified_Multiple_Properties()
        {
            // Arrange
            var sortable = new Sortable
            {
                SortByProperties = $"{nameof(MockModel.Value)} asc;{nameof(MockModel.Id)} desc;"
            };

            var expectedQuery = _mockDbSet.OrderBy($"{nameof(MockModel.Value)} asc, {nameof(MockModel.Id)} desc").ToString();

            // Act
            var actualQuery = _mockDbSet.OrderByPropertyNames(sortable).ToString();

            // Assert
            Assert.AreEqual(expectedQuery, actualQuery);
        }
    }
}
