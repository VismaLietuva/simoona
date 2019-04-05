using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using NSubstitute;
using NUnit.Framework;
using Shrooms.Azure;
using Shrooms.DataLayer.DAL;
using Shrooms.Domain.Services.Picture;
using Shrooms.EntityModels.Models;
using Shrooms.UnitTests.Extensions;

namespace Shrooms.UnitTests.DomainService
{
    [TestFixture]
    public class PictureServiceTests
    {
        private IPictureService _pictureService;
        private IDbSet<Organization> _organizationsDbSet;

        [SetUp]
        public void Init()
        {
            var uow = Substitute.For<IUnitOfWork2>();
            _organizationsDbSet = uow.MockDbSet<Organization>();

            var azureStorage = Substitute.For<IStorage>();
            _pictureService = new PictureService(azureStorage, uow);
        }

        [Test]
        public void UploadFromStream_ShouldReturnCorrectName_WhenJpg()
        {
            // Arrange
            _organizationsDbSet.SetDbSetData(new List<Organization> { new Organization { Id = 2, ShortName = "pictures" } }.AsQueryable());

            // Act
            var result = _pictureService.UploadFromStream(null, null, "test.jpg", 2).Result;

            // Assert
            Assert.That(result, Does.EndWith(".jpg"));
        }

        [Test]
        public void UploadFromStream_ShouldReturnCorrectName_WhenPng()
        {
            // Arrange
            _organizationsDbSet.SetDbSetData(new List<Organization> { new Organization { Id = 2, ShortName = "pictures" } }.AsQueryable());

            // Act
            var result = _pictureService.UploadFromStream(null, null, "test.png", 2).Result;

            // Assert
            Assert.That(result, Does.EndWith(".png"));
        }

        [Test]
        public void UploadFromStream_ShouldReturnCorrectName_WhenGif()
        {
            // Arrange
            _organizationsDbSet.SetDbSetData(new List<Organization> { new Organization { Id = 2, ShortName = "pictures" } }.AsQueryable());

            // Act
            var result = _pictureService.UploadFromStream(null, null, "test.gif", 2).Result;

            // Assert
            Assert.That(result, Does.EndWith(".gif"));
        }
    }
}