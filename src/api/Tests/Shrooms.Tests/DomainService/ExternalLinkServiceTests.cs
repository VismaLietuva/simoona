using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using NSubstitute;
using NUnit.Framework;
using Shrooms.DataTransferObjects.Models.ExternalLinks;
using Shrooms.Domain.Services.ExternalLinks;
using Shrooms.DomainExceptions.Exceptions;
using Shrooms.EntityModels.Models;
using Shrooms.Host.Contracts.Constants;
using Shrooms.Host.Contracts.DAL;
using Shrooms.UnitTests.Extensions;

namespace Shrooms.UnitTests.DomainService
{
    [TestFixture]
    public class ExternalLinkServiceTests
    {
        private IExternalLinkService _externalLinkService;
        private IDbSet<ExternalLink> _externalLinkDbSet;

        [SetUp]
        public void TestInitializer()
        {
            _externalLinkDbSet = Substitute.For<IDbSet<ExternalLink>>();

            var uow = Substitute.For<IUnitOfWork2>();

            uow.GetDbSet<ExternalLink>().Returns(_externalLinkDbSet);

            _externalLinkService = new ExternalLinkService(uow);
        }

        [Test]
        public void Should_Return_All_External_Link_Depending_On_Organization()
        {
            MockExternalLinks();

            var result = _externalLinkService.GetAll(2);
            Assert.AreEqual(2, result.Count());
            Assert.AreEqual("Test1", result.First().Name);
        }

        [Test]
        public void Should_Add_Update_Delete_Links_Successfully()
        {
            MockExternalLinksForCrud();
            var updateDto = new AddEditDeleteExternalLinkDTO
            {
                LinksToDelete = new[] { 1, 2 },
                LinksToCreate = new List<NewExternalLinkDTO>
                {
                    new NewExternalLinkDTO
                    {
                        Name = "newLink1",
                        Url = "newLink2"
                    }
                },
                LinksToUpdate = new List<ExternalLinkDTO>
                {
                    new ExternalLinkDTO
                    {
                        Id = 3,
                        Name = "modifiedLink3",
                        Url = "http://link3modified.com"
                    }
                },
                OrganizationId = 2,
                UserId = "testUser"
            };

            _externalLinkService.UpdateLinks(updateDto);

            _externalLinkDbSet.Received(2).Remove(Arg.Any<ExternalLink>());
            _externalLinkDbSet.Received(1).Add(Arg.Any<ExternalLink>());
            Assert.AreEqual("modifiedLink3", _externalLinkDbSet.First(x => x.Id == 3).Name);
            Assert.AreEqual("http://link3modified.com", _externalLinkDbSet.First(x => x.Id == 3).Url);
        }

        [Test]
        public void Should_Throw_If_Trying_To_Add_Duplicate_Link()
        {
            MockExternalLinks();
            var updateDto = new AddEditDeleteExternalLinkDTO
            {
                LinksToDelete = new[] { 1, 2 },
                LinksToCreate = new List<NewExternalLinkDTO>
                {
                    new NewExternalLinkDTO
                    {
                        Name = "Test1",
                        Url = "UrlTest1"
                    }
                },
                LinksToUpdate = new List<ExternalLinkDTO>(),
                OrganizationId = 2,
                UserId = "testUser"
            };

            var ex = Assert.Throws<ValidationException>(() => _externalLinkService.UpdateLinks(updateDto));
            Assert.AreEqual(ErrorCodes.DuplicatesIntolerable, ex.ErrorCode);
        }

        [Test]
        public void Should_Throw_If_Trying_To_Update_To_Duplicate_Link()
        {
            MockExternalLinks();
            var updateDto = new AddEditDeleteExternalLinkDTO
            {
                LinksToDelete = new[] { 1, 2 },
                LinksToCreate = new List<NewExternalLinkDTO>(),
                LinksToUpdate = new List<ExternalLinkDTO>
                {
                    new ExternalLinkDTO
                    {
                        Id = 3,
                        Name = "Test1",
                        Url = "UrlTest1"
                    }
                },
                OrganizationId = 2,
                UserId = "testUser"
            };

            var ex = Assert.Throws<ValidationException>(() => _externalLinkService.UpdateLinks(updateDto));
            Assert.AreEqual(ErrorCodes.DuplicatesIntolerable, ex.ErrorCode);
        }

        private void MockExternalLinksForCrud()
        {
            var links = new List<ExternalLink>
            {
                new ExternalLink
                {
                    Id = 1,
                    Modified = DateTime.UtcNow.AddHours(-2),
                    Name = "link1",
                    Url = "http://link1.com",
                    OrganizationId = 2
                },
                new ExternalLink
                {
                    Id = 2,
                    Modified = DateTime.UtcNow.AddHours(-2),
                    Name = "link2",
                    Url = "http://link2.com",
                    OrganizationId = 2
                },
                new ExternalLink
                {
                    Id = 3,
                    Modified = DateTime.UtcNow.AddHours(-2),
                    Name = "link3",
                    Url = "http://link3.com",
                    OrganizationId = 2
                },
                new ExternalLink
                {
                    Id = 4,
                    Modified = DateTime.UtcNow.AddHours(-2),
                    Name = "link4",
                    Url = "http://link4.com",
                    OrganizationId = 2
                }
            };

            _externalLinkDbSet.SetDbSetData(links.AsQueryable());
        }

        private void MockExternalLinks()
        {
            var externalLinks = new List<ExternalLink>()
            {
                new ExternalLink
                {
                    Id = 1,
                    Name = "Test1",
                    Url = "UrlTest1",
                    OrganizationId = 2
                },
                new ExternalLink
                {
                    Id = 2,
                    Name = "Test2",
                    Url = "UrlTest2",
                    OrganizationId = 2
                },
                new ExternalLink
                {
                    Id = 3,
                    Name = "Test3",
                    Url = "UrlTest3",
                    OrganizationId = 1
                }
            }.AsQueryable();

            _externalLinkDbSet.SetDbSetData(externalLinks);
        }
    }
}
