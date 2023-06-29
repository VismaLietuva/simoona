using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects.Models.ExternalLinks;
using Shrooms.Contracts.Exceptions;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.Domain.Services.ExternalLinks;
using Shrooms.Tests.Extensions;

namespace Shrooms.Tests.DomainService
{
    [TestFixture]
    public class ExternalLinkServiceTests
    {
        private IExternalLinkService _externalLinkService;
        private DbSet<ExternalLink> _externalLinkDbSet;

        [SetUp]
        public void TestInitializer()
        {
            _externalLinkDbSet = Substitute.For<DbSet<ExternalLink>, IQueryable<ExternalLink>, IDbAsyncEnumerable<ExternalLink>>();

            var uow = Substitute.For<IUnitOfWork2>();

            uow.GetDbSet<ExternalLink>().Returns(_externalLinkDbSet);

            _externalLinkService = new ExternalLinkService(uow);
        }

        [Test]
        public async Task Should_Return_All_External_Link_Depending_On_Organization()
        {
            MockExternalLinks();

            var result = (await _externalLinkService.GetAllAsync(2)).ToList();
            Assert.AreEqual(3, result.Count);
            Assert.AreEqual("Test1", result.First().Name);
        }

        [Test]
        public async Task Should_Add_Update_Delete_Links_Successfully()
        {
            MockExternalLinksForCrud();
            var updateDto = new ManageExternalLinkDto
            {
                LinksToDelete = new[] { 1, 2 },
                LinksToCreate = new List<NewExternalLinkDto>
                {
                    new()
                    {
                        Name = "newLink1",
                        Url = "newLink2"
                    }
                },
                LinksToUpdate = new List<ExternalLinkDto>
                {
                    new()
                    {
                        Id = 3,
                        Name = "modifiedLink3",
                        Url = "http://link3modified.com"
                    }
                },
                OrganizationId = 2,
                UserId = "testUser"
            };

            await _externalLinkService.UpdateLinksAsync(updateDto);

            _externalLinkDbSet.Received(2).Remove(Arg.Any<ExternalLink>());
            _externalLinkDbSet.Received(1).Add(Arg.Any<ExternalLink>());
            var externalLink = await _externalLinkDbSet.FirstAsync(x => x.Id == 3);

            Assert.AreEqual("modifiedLink3", externalLink.Name);
            Assert.AreEqual("http://link3modified.com", externalLink.Url);
        }

        [Test]
        public void Should_Throw_If_Trying_To_Add_Duplicate_Link()
        {
            MockExternalLinks();
            var updateDto = new ManageExternalLinkDto
            {
                LinksToDelete = new[] { 1, 2 },
                LinksToCreate = new List<NewExternalLinkDto>
                {
                    new()
                    {
                        Name = "Test1",
                        Url = "UrlTest1"
                    }
                },
                LinksToUpdate = new List<ExternalLinkDto>(),
                OrganizationId = 2,
                UserId = "testUser"
            };

            var ex = Assert.ThrowsAsync<ValidationException>(async () => await _externalLinkService.UpdateLinksAsync(updateDto));
            Assert.AreEqual(ErrorCodes.DuplicatesIntolerable, ex.ErrorCode);
        }

        [Test]
        public void Should_Throw_If_Trying_To_Update_To_Duplicate_Link()
        {
            MockExternalLinks();
            var updateDto = new ManageExternalLinkDto
            {
                LinksToDelete = new[] { 1, 2 },
                LinksToCreate = new List<NewExternalLinkDto>(),
                LinksToUpdate = new List<ExternalLinkDto>
                {
                    new()
                    {
                        Id = 3,
                        Name = "Test1",
                        Url = "UrlTest1"
                    }
                },
                OrganizationId = 2,
                UserId = "testUser"
            };

            var ex = Assert.ThrowsAsync<ValidationException>(async () => await _externalLinkService.UpdateLinksAsync(updateDto));
            Assert.AreEqual(ErrorCodes.DuplicatesIntolerable, ex.ErrorCode);
        }

        [Test]
        public async Task Should_Return_All_External_Links_In_Descending_Order_By_Priority()
        {
            // Arrange
            MockExternalLinks();

            var expectedIdsOrder = new List<int> { 1, 4, 2 };

            // Act
            var result = await _externalLinkService.GetAllAsync(2);

            // Assert
            CollectionAssert.AreEqual(expectedIdsOrder, result.Select(link => link.Id));
        }

        private void MockExternalLinksForCrud()
        {
            var links = new List<ExternalLink>
            {
                new()
                {
                    Id = 1,
                    Modified = DateTime.UtcNow.AddHours(-2),
                    Name = "link1",
                    Url = "http://link1.com",
                    OrganizationId = 2
                },
                new()
                {
                    Id = 2,
                    Modified = DateTime.UtcNow.AddHours(-2),
                    Name = "link2",
                    Url = "http://link2.com",
                    OrganizationId = 2
                },
                new()
                {
                    Id = 3,
                    Modified = DateTime.UtcNow.AddHours(-2),
                    Name = "link3",
                    Url = "http://link3.com",
                    OrganizationId = 2
                },
                new()
                {
                    Id = 4,
                    Modified = DateTime.UtcNow.AddHours(-2),
                    Name = "link4",
                    Url = "http://link4.com",
                    OrganizationId = 2
                }
            };

            _externalLinkDbSet.SetDbSetDataForAsync(links.AsQueryable());
        }

        private void MockExternalLinks()
        {
            var externalLinks = new List<ExternalLink>
            {
                new()
                {
                    Id = 1,
                    Name = "Test1",
                    Url = "UrlTest1",
                    OrganizationId = 2,
                    Priority = 10
                },
                new()
                {
                    Id = 2,
                    Name = "Test2",
                    Url = "UrlTest2",
                    OrganizationId = 2,
                    Priority = 0
                },
                new()
                {
                    Id = 3,
                    Name = "Test3",
                    Url = "UrlTest3",
                    OrganizationId = 1,
                    Priority = 5
                },
                new()
                {
                    Id = 4,
                    Name = "Test4",
                    Url = "UrlTest4",
                    OrganizationId = 2,
                    Priority = 9
                }
            }.AsQueryable();

            _externalLinkDbSet.SetDbSetDataForAsync(externalLinks);
        }
    }
}
