using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using NUnit.Framework;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.Enums;
using Shrooms.Contracts.Exceptions;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.DataLayer.EntityModels.Models.Group;
using Shrooms.Domain.Services.Permissions;
using Shrooms.Premium.DataTransferObjects.Models.Groups;
using Shrooms.Premium.Domain.Services.Groups;
using Shrooms.Tests.Extensions;
using GroupEntity = Shrooms.DataLayer.EntityModels.Models.Group.Group;

namespace Shrooms.Premium.Tests.DomainService
{
    public class GroupTypesServiceTests
    {
        private IGroupTypesService _service;
        private IUnitOfWork2 _uow;
        private IPermissionService _permissionService;
        private DbSet<GroupType> _groupTypesDbSet;
        private DbSet<GroupEntity> _groupsDbSet;

        [SetUp]
        public void TestInitializer()
        {
            _uow = Substitute.For<IUnitOfWork2>();

            _groupTypesDbSet = Substitute.For<DbSet<GroupType>, IQueryable<GroupType>, IAsyncEnumerable<GroupType>>();
            _groupTypesDbSet.SetDbSetDataForAsync(MockGroupTypes());
            _uow.GetDbSet<GroupType>().Returns(_groupTypesDbSet);

            _groupsDbSet = Substitute.For<DbSet<GroupEntity>, IQueryable<GroupEntity>, IAsyncEnumerable<GroupEntity>>();
            _groupsDbSet.SetDbSetDataForAsync(MockGroups());
            _uow.GetDbSet<GroupEntity>().Returns(_groupsDbSet);

            _permissionService = Substitute.For<IPermissionService>();
            _permissionService.UserHasPermissionAsync(Arg.Any<UserAndOrganizationDto>(), Arg.Any<string>())
                .Returns(true);

            _service = new GroupTypesService(_uow, _permissionService);
        }

        private static IList<GroupType> MockGroupTypes() => new List<GroupType>
        {
            new GroupType { Id = 1, OrganizationId = 1, Name = "Committee", HasGroupTag = true, KudosTypeId = 3 },
            new GroupType { Id = 2, OrganizationId = 1, Name = "TaskForce", HasGroupTag = true, IsTemporary = true },
            new GroupType { Id = 3, OrganizationId = 2, Name = "Committee" }
        };

        private static IList<GroupEntity> MockGroups() => new List<GroupEntity>
        {
            new GroupEntity { Id = 1, OrganizationId = 1, Name = "Kudos committee", GroupTypeId = 1 }
        };

        [Test]
        public async Task Should_Order_Types_By_Sort_Order_Then_Name()
        {
            _groupTypesDbSet.SetDbSetDataForAsync(new List<GroupType>
            {
                new GroupType { Id = 1, OrganizationId = 1, Name = "Zebra", SortOrder = 1 },
                new GroupType { Id = 2, OrganizationId = 1, Name = "Alpha", SortOrder = 2 },
                // Ties fall back to the name.
                new GroupType { Id = 3, OrganizationId = 1, Name = "Beta", SortOrder = 2 }
            });

            var types = await _service.GetAllAsync(new UserAndOrganizationDto { OrganizationId = 1, UserId = "user" });

            Assert.That(types.Select(t => t.Name), Is.EqualTo(new[] { "Zebra", "Alpha", "Beta" }));
        }

        [Test]
        public async Task Should_Redact_Kudos_Configuration_For_Non_Kudos_Administrators()
        {
            // Everyone can read the list - they need it to create a group - but the
            // kudos configuration is not theirs to see.
            _permissionService
                .UserHasPermissionAsync(Arg.Any<UserAndOrganizationDto>(), AdministrationPermissions.Kudos)
                .Returns(false);

            var types = await _service.GetAllAsync(new UserAndOrganizationDto { OrganizationId = 1, UserId = "user" });

            var committee = types.Single(t => t.Name == "Committee");

            Assert.Multiple(() =>
            {
                Assert.That(committee.KudosTypeId, Is.Null);
                Assert.That(committee.KudosTypeName, Is.Null);
                Assert.That(committee.KudosTypeValue, Is.Null);
                // The rest is what a creator needs, so it stays.
                Assert.That(committee.CreationPolicy, Is.EqualTo(GroupCreationPolicy.AdminOnly));
            });
        }

        [Test]
        public async Task Should_Keep_Kudos_Configuration_For_Kudos_Administrators()
        {
            var types = await _service.GetAllAsync(new UserAndOrganizationDto { OrganizationId = 1, UserId = "admin" });

            Assert.That(types.Single(t => t.Name == "Committee").KudosTypeId, Is.EqualTo(3));
        }

        [Test]
        public void Should_Throw_When_Creating_Type_With_Duplicate_Name_In_Same_Organization()
        {
            var dto = new CreateGroupTypeDto { Name = "Committee", OrganizationId = 1, UserId = "user" };

            var ex = Assert.ThrowsAsync<ValidationException>(async () => await _service.CreateAsync(dto));

            Assert.That(ex.ErrorCode, Is.EqualTo(ErrorCodes.GroupTypeNameAlreadyExists));
        }

        [Test]
        public async Task Should_Allow_Same_Type_Name_In_Different_Organization()
        {
            var dto = new CreateGroupTypeDto { Name = "TaskForce", OrganizationId = 2, UserId = "user" };

            await _service.CreateAsync(dto);

            _groupTypesDbSet.Received(1).Add(Arg.Is<GroupType>(t => t.Name == "TaskForce" && t.OrganizationId == 2));
        }

        [Test]
        public async Task Should_Allow_A_Temporary_Type_That_Receives_Kudos()
        {
            // Temporary groups do receive kudos - once at the end of their term,
            // which is why the monthly run skips them rather than the type being invalid.
            var dto = new CreateGroupTypeDto
            {
                Name = "TaskForceWithKudos",
                OrganizationId = 1,
                UserId = "user",
                IsTemporary = true,
                KudosTypeId = 3
            };

            await _service.CreateAsync(dto);

            _groupTypesDbSet.Received(1).Add(Arg.Is<GroupType>(t => t.IsTemporary && t.KudosTypeId == 3));
        }

        [Test]
        public void Should_Throw_When_Deleting_Type_That_Has_Groups()
        {
            var userAndOrg = new UserAndOrganizationDto { OrganizationId = 1, UserId = "user" };

            var ex = Assert.ThrowsAsync<ValidationException>(async () => await _service.DeleteAsync(1, userAndOrg));

            Assert.That(ex.ErrorCode, Is.EqualTo(ErrorCodes.GroupTypeHasGroups));
        }

        [Test]
        public async Task Should_Delete_Type_With_No_Groups()
        {
            var userAndOrg = new UserAndOrganizationDto { OrganizationId = 1, UserId = "user" };

            await _service.DeleteAsync(2, userAndOrg);

            _groupTypesDbSet.Received(1).Remove(Arg.Is<GroupType>(t => t.Id == 2));
        }

        [Test]
        public async Task Should_Clear_Group_Dates_When_Temporary_Turned_Off()
        {
            var group = new GroupEntity
            {
                Id = 5,
                OrganizationId = 1,
                GroupTypeId = 2,
                StartDate = new System.DateTime(2026, 1, 1),
                EndDate = new System.DateTime(2026, 6, 30),
                Members = new List<GroupMember>()
            };

            _groupsDbSet.SetDbSetDataForAsync(new List<GroupEntity> { group });

            await _service.UpdateAsync(new UpdateGroupTypeDto
            {
                Id = 2,
                Name = "TaskForce",
                OrganizationId = 1,
                UserId = "user",
                HasGroupTag = true,
                IsTemporary = false
            });

            Assert.Multiple(() =>
            {
                Assert.That(group.StartDate, Is.Null);
                Assert.That(group.EndDate, Is.Null);
            });
        }

        [Test]
        public async Task Should_Preserve_Kudos_Type_When_Editor_Is_Not_A_Kudos_Administrator()
        {
            _permissionService
                .UserHasPermissionAsync(Arg.Any<UserAndOrganizationDto>(), AdministrationPermissions.Kudos)
                .Returns(false);

            var committee = new GroupType
            {
                Id = 1,
                OrganizationId = 1,
                Name = "Committee",
                HasGroupTag = true,
                KudosTypeId = 3
            };

            _groupTypesDbSet.SetDbSetDataForAsync(new List<GroupType> { committee });

            await _service.UpdateAsync(new UpdateGroupTypeDto
            {
                Id = 1,
                Name = "Committee",
                OrganizationId = 1,
                UserId = "user",
                HasGroupTag = true,
                KudosTypeId = null
            });

            Assert.That(committee.KudosTypeId, Is.EqualTo(3));
        }
    }
}
