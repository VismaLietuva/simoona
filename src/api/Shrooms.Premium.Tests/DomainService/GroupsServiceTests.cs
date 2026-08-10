using System;
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
using Shrooms.Domain.Services.Permissions;
using Shrooms.DataLayer.EntityModels.Models.Group;
using Shrooms.DataLayer.EntityModels.Models.Kudos;
using Shrooms.Premium.DataTransferObjects.Models.Groups;
using Shrooms.Premium.Domain.Services.Groups;
using Shrooms.Tests.Extensions;
using GroupEntity = Shrooms.DataLayer.EntityModels.Models.Group.Group;

namespace Shrooms.Premium.Tests.DomainService
{
    public class GroupsServiceTests
    {
        private IGroupsService _service;
        private IUnitOfWork2 _uow;
        private IPermissionService _permissionService;
        private DbSet<GroupEntity> _groupsDbSet;
        private DbSet<GroupType> _groupTypesDbSet;
        private DbSet<ApplicationUser> _usersDbSet;
        private DbSet<KudosType> _kudosTypesDbSet;

        [SetUp]
        public void TestInitializer()
        {
            _uow = Substitute.For<IUnitOfWork2>();
            _permissionService = Substitute.For<IPermissionService>();
            _permissionService.UserHasPermissionAsync(Arg.Any<UserAndOrganizationDto>(), Arg.Any<string>())
                .Returns(true);

            _groupTypesDbSet = Substitute.For<DbSet<GroupType>, IQueryable<GroupType>, IAsyncEnumerable<GroupType>>();
            _groupTypesDbSet.SetDbSetDataForAsync(MockGroupTypes());
            _uow.GetDbSet<GroupType>().Returns(_groupTypesDbSet);

            _groupsDbSet = Substitute.For<DbSet<GroupEntity>, IQueryable<GroupEntity>, IAsyncEnumerable<GroupEntity>>();
            _groupsDbSet.SetDbSetDataForAsync(MockGroups());
            _uow.GetDbSet<GroupEntity>().Returns(_groupsDbSet);

            _usersDbSet = Substitute.For<DbSet<ApplicationUser>, IQueryable<ApplicationUser>, IAsyncEnumerable<ApplicationUser>>();
            _usersDbSet.SetDbSetDataForAsync(new List<ApplicationUser>
            {
                new ApplicationUser { Id = "user1", OrganizationId = 1 },
                new ApplicationUser { Id = "user2", OrganizationId = 1 }
            });
            _uow.GetDbSet<ApplicationUser>().Returns(_usersDbSet);

            _kudosTypesDbSet = Substitute.For<DbSet<KudosType>, IQueryable<KudosType>, IAsyncEnumerable<KudosType>>();
            _kudosTypesDbSet.SetDbSetDataForAsync(new List<KudosType>
            {
                new KudosType { Id = 3, Name = "Monthly", Value = 1 }
            });
            _uow.GetDbSet<KudosType>().Returns(_kudosTypesDbSet);

            _service = new GroupsService(_uow, _permissionService);
        }

        // Type 1: open. 2: temporary. 3: kudos + tag. 4: admin-only. 5: needs approval.
        private static IList<GroupType> MockGroupTypes() => new List<GroupType>
        {
            new GroupType { Id = 1, OrganizationId = 1, Name = "Other", CreationPolicy = GroupCreationPolicy.Open },
            new GroupType { Id = 2, OrganizationId = 1, Name = "TaskForce", IsTemporary = true },
            new GroupType { Id = 3, OrganizationId = 1, Name = "FoodMaster", HasGroupTag = true, KudosTypeId = 3 },
            new GroupType { Id = 4, OrganizationId = 1, Name = "Committee", CreationPolicy = GroupCreationPolicy.AdminOnly },
            new GroupType
            {
                Id = 5,
                OrganizationId = 1,
                Name = "TaskForce",
                CreationPolicy = GroupCreationPolicy.RequiresApproval,
                ApprovalQuestions = "List the goals of the taskforce:"
            }
        };

        private static IList<GroupEntity> MockGroups() => new List<GroupEntity>
        {
            new GroupEntity
            {
                Id = 1,
                OrganizationId = 1,
                Name = "Existing",
                GroupTypeId = 1,
                Members = new List<GroupMember> { new GroupMember { Id = 40, UserId = "user1" } }
            }
        };

        // The editing member is carried through the payload - dropping an open-ended
        // membership is a separate operation with its own rule.
        private static List<GroupMemberPostDto> KeepingMember(int membershipId, string userId) =>
            new List<GroupMemberPostDto> { new GroupMemberPostDto { MembershipId = membershipId, Id = userId } };

        private static GroupPostDto ValidPost(int groupTypeId) => new GroupPostDto
        {
            Name = "New group",
            Description = "Description",
            GroupTypeId = groupTypeId,
            OrganizationId = 1,
            UserId = "user1",
            Members = new List<GroupMemberPostDto>()
        };

        [Test]
        public void Should_Throw_When_A_Regular_User_Creates_An_Admin_Only_Type()
        {
            _permissionService
                .UserHasPermissionAsync(Arg.Any<UserAndOrganizationDto>(), AdministrationPermissions.Groups)
                .Returns(false);

            var ex = Assert.ThrowsAsync<ValidationException>(async () => await _service.CreateAsync(ValidPost(4)));

            Assert.That(ex.ErrorCode, Is.EqualTo(ErrorCodes.GroupCreationNotAllowed));
        }

        [Test]
        public async Task Should_Let_A_Regular_User_Create_An_Open_Type()
        {
            _permissionService
                .UserHasPermissionAsync(Arg.Any<UserAndOrganizationDto>(), AdministrationPermissions.Groups)
                .Returns(false);

            await _service.CreateAsync(ValidPost(1));

            _groupsDbSet.Received(1).Add(Arg.Is<GroupEntity>(g => g.Status == GroupStatus.Approved));
        }

        [Test]
        public void Should_Require_Approval_Answers_On_A_Type_That_Asks_Questions()
        {
            _permissionService
                .UserHasPermissionAsync(Arg.Any<UserAndOrganizationDto>(), AdministrationPermissions.Groups)
                .Returns(false);

            var ex = Assert.ThrowsAsync<ValidationException>(async () => await _service.CreateAsync(ValidPost(5)));

            Assert.That(ex.ErrorCode, Is.EqualTo(ErrorCodes.GroupApprovalAnswersRequired));
        }

        [Test]
        public async Task Should_Create_An_Approval_Type_As_Pending_With_The_Creator_As_A_Member()
        {
            _permissionService
                .UserHasPermissionAsync(Arg.Any<UserAndOrganizationDto>(), AdministrationPermissions.Groups)
                .Returns(false);

            var dto = ValidPost(5);
            dto.ApprovalAnswers = "Answered";

            await _service.CreateAsync(dto);

            _groupsDbSet.Received(1).Add(Arg.Is<GroupEntity>(g =>
                g.Status == GroupStatus.Pending &&
                g.ApprovalAnswers == "Answered" &&
                g.Members.Any(m => m.UserId == "user1")));
        }

        [Test]
        public async Task Should_Approve_Outright_When_An_Administrator_Creates_An_Approval_Type()
        {
            // An administrator approves their own group, so the questions are not asked.
            var dto = ValidPost(5);

            await _service.CreateAsync(dto);

            _groupsDbSet.Received(1).Add(Arg.Is<GroupEntity>(g => g.Status == GroupStatus.Approved));
        }

        [Test]
        public async Task Should_Keep_The_Approval_Answers_When_An_Edit_Leaves_Them_Out()
        {
            var group = new GroupEntity
            {
                Id = 1, OrganizationId = 1, Name = "Existing", GroupTypeId = 5,
                Status = GroupStatus.Approved,
                ApprovalAnswers = "Answered at creation",
                Members = new List<GroupMember> { new GroupMember { Id = 40, UserId = "user1" } }
            };

            _groupsDbSet.SetDbSetDataForAsync(new List<GroupEntity> { group });

            var dto = ValidPost(5);
            dto.Id = 1;
            dto.Name = "Existing";
            dto.Members = KeepingMember(40, "user1");

            await _service.UpdateAsync(dto);

            Assert.That(group.ApprovalAnswers, Is.EqualTo("Answered at creation"));
        }

        [Test]
        public async Task Should_Let_An_Edit_Replace_The_Approval_Answers()
        {
            var group = new GroupEntity
            {
                Id = 1, OrganizationId = 1, Name = "Existing", GroupTypeId = 5,
                Status = GroupStatus.Approved,
                ApprovalAnswers = "Answered at creation",
                Members = new List<GroupMember> { new GroupMember { Id = 40, UserId = "user1" } }
            };

            _groupsDbSet.SetDbSetDataForAsync(new List<GroupEntity> { group });

            var dto = ValidPost(5);
            dto.Id = 1;
            dto.Name = "Existing";
            dto.Members = KeepingMember(40, "user1");
            dto.ApprovalAnswers = "Revised answers";

            await _service.UpdateAsync(dto);

            Assert.That(group.ApprovalAnswers, Is.EqualTo("Revised answers"));
        }

        [Test]
        public async Task Should_Hide_Pending_Groups_From_Non_Members()
        {
            _permissionService
                .UserHasPermissionAsync(Arg.Any<UserAndOrganizationDto>(), AdministrationPermissions.Groups)
                .Returns(false);

            _groupsDbSet.SetDbSetDataForAsync(new List<GroupEntity>
            {
                new GroupEntity
                {
                    Id = 8, OrganizationId = 1, Name = "Pending force", GroupTypeId = 5,
                    Status = GroupStatus.Pending,
                    Members = new List<GroupMember> { new GroupMember { UserId = "creator" } }
                }
            });

            var forOutsider = await _service.GetAllAsync(
                new UserAndOrganizationDto { OrganizationId = 1, UserId = "outsider" });
            var forCreator = await _service.GetAllAsync(
                new UserAndOrganizationDto { OrganizationId = 1, UserId = "creator" });

            Assert.Multiple(() =>
            {
                Assert.That(forOutsider, Is.Empty);
                Assert.That(forCreator.Single().IsPending, Is.True);
            });
        }

        [Test]
        public void Should_Hide_A_Pending_Group_From_A_Non_Member_Fetching_It_By_Id()
        {
            _permissionService
                .UserHasPermissionAsync(Arg.Any<UserAndOrganizationDto>(), AdministrationPermissions.Groups)
                .Returns(false);

            _groupsDbSet.SetDbSetDataForAsync(new List<GroupEntity>
            {
                new GroupEntity
                {
                    Id = 8, OrganizationId = 1, Name = "Pending force", GroupTypeId = 5,
                    Status = GroupStatus.Pending,
                    ApprovalAnswers = "Answered",
                    Members = new List<GroupMember> { new GroupMember { UserId = "creator" } }
                }
            });

            var ex = Assert.ThrowsAsync<ValidationException>(async () =>
                await _service.GetAsync(new UserAndOrganizationDto { OrganizationId = 1, UserId = "outsider" }, 8));

            // Reads as missing, so fetching by id does not confirm the group exists.
            Assert.That(ex.ErrorCode, Is.EqualTo(ErrorCodes.GroupNotFound));
        }

        [Test]
        public async Task Should_Show_A_Pending_Group_By_Id_To_Its_Members_And_To_Administrators()
        {
            _groupsDbSet.SetDbSetDataForAsync(new List<GroupEntity>
            {
                new GroupEntity
                {
                    Id = 8, OrganizationId = 1, Name = "Pending force", GroupTypeId = 5,
                    Status = GroupStatus.Pending,
                    Members = new List<GroupMember> { new GroupMember { UserId = "creator" } }
                }
            });

            var forAdmin = await _service.GetAsync(
                new UserAndOrganizationDto { OrganizationId = 1, UserId = "admin" }, 8);

            _permissionService
                .UserHasPermissionAsync(Arg.Any<UserAndOrganizationDto>(), AdministrationPermissions.Groups)
                .Returns(false);

            var forCreator = await _service.GetAsync(
                new UserAndOrganizationDto { OrganizationId = 1, UserId = "creator" }, 8);

            Assert.Multiple(() =>
            {
                Assert.That(forAdmin.IsPending, Is.True);
                Assert.That(forCreator.IsPending, Is.True);
            });
        }

        [Test]
        public async Task Should_Approve_A_Pending_Group()
        {
            var pending = new GroupEntity
            {
                Id = 8, OrganizationId = 1, Name = "Pending force", GroupTypeId = 5,
                Status = GroupStatus.Pending
            };

            _groupsDbSet.SetDbSetDataForAsync(new List<GroupEntity> { pending });

            await _service.ApproveAsync(8, new UserAndOrganizationDto { OrganizationId = 1, UserId = "admin" });

            Assert.That(pending.Status, Is.EqualTo(GroupStatus.Approved));
        }

        [Test]
        public async Task Should_Let_The_Creator_Delete_Their_Own_Pending_Group()
        {
            _permissionService
                .UserHasPermissionAsync(Arg.Any<UserAndOrganizationDto>(), AdministrationPermissions.Groups)
                .Returns(false);

            var pending = new GroupEntity
            {
                Id = 8, OrganizationId = 1, Name = "Pending force", GroupTypeId = 5,
                Status = GroupStatus.Pending, CreatedBy = "creator"
            };

            _groupsDbSet.SetDbSetDataForAsync(new List<GroupEntity> { pending });

            await _service.DeleteAsync(8, new UserAndOrganizationDto { OrganizationId = 1, UserId = "creator" });

            _groupsDbSet.Received(1).Remove(pending);
        }

        [Test]
        public void Should_Throw_When_Someone_Else_Deletes_A_Pending_Group()
        {
            _permissionService
                .UserHasPermissionAsync(Arg.Any<UserAndOrganizationDto>(), AdministrationPermissions.Groups)
                .Returns(false);

            _groupsDbSet.SetDbSetDataForAsync(new List<GroupEntity>
            {
                new GroupEntity
                {
                    Id = 8, OrganizationId = 1, Name = "Pending force", GroupTypeId = 5,
                    Status = GroupStatus.Pending, CreatedBy = "creator"
                }
            });

            var ex = Assert.ThrowsAsync<ValidationException>(async () =>
                await _service.DeleteAsync(8, new UserAndOrganizationDto { OrganizationId = 1, UserId = "someone-else" }));

            Assert.That(ex.ErrorCode, Is.EqualTo(ErrorCodes.GroupDeleteNotAllowed));
        }

        [Test]
        public void Should_Throw_When_The_Creator_Deletes_Their_Group_After_Approval()
        {
            _permissionService
                .UserHasPermissionAsync(Arg.Any<UserAndOrganizationDto>(), AdministrationPermissions.Groups)
                .Returns(false);

            _groupsDbSet.SetDbSetDataForAsync(new List<GroupEntity>
            {
                new GroupEntity
                {
                    Id = 8, OrganizationId = 1, Name = "Approved force", GroupTypeId = 5,
                    Status = GroupStatus.Approved, CreatedBy = "creator"
                }
            });

            var ex = Assert.ThrowsAsync<ValidationException>(async () =>
                await _service.DeleteAsync(8, new UserAndOrganizationDto { OrganizationId = 1, UserId = "creator" }));

            Assert.That(ex.ErrorCode, Is.EqualTo(ErrorCodes.GroupDeleteNotAllowed));
        }

        [Test]
        public async Task Should_Keep_Name_And_Type_When_A_Member_Edits_An_Approved_Group()
        {
            _permissionService
                .UserHasPermissionAsync(Arg.Any<UserAndOrganizationDto>(), AdministrationPermissions.Groups)
                .Returns(false);

            var approved = new GroupEntity
            {
                Id = 1, OrganizationId = 1, Name = "Existing", GroupTypeId = 1,
                Status = GroupStatus.Approved,
                Members = new List<GroupMember> { new GroupMember { Id = 40, UserId = "user1" } }
            };

            _groupsDbSet.SetDbSetDataForAsync(new List<GroupEntity> { approved });

            var dto = ValidPost(2);
            dto.Id = 1;
            dto.Name = "Renamed by a member";
            dto.UserId = "user1";
            dto.Members = KeepingMember(40, "user1");

            await _service.UpdateAsync(dto);

            Assert.Multiple(() =>
            {
                Assert.That(approved.Name, Is.EqualTo("Existing"));
                Assert.That(approved.GroupTypeId, Is.EqualTo(1));
            });
        }

        [Test]
        public async Task Should_Let_A_Member_Rename_Their_Group_While_It_Is_Pending()
        {
            _permissionService
                .UserHasPermissionAsync(Arg.Any<UserAndOrganizationDto>(), AdministrationPermissions.Groups)
                .Returns(false);

            var pending = new GroupEntity
            {
                Id = 1, OrganizationId = 1, Name = "Draft", GroupTypeId = 1,
                Status = GroupStatus.Pending,
                Members = new List<GroupMember> { new GroupMember { Id = 40, UserId = "user1" } }
            };

            _groupsDbSet.SetDbSetDataForAsync(new List<GroupEntity> { pending });

            var dto = ValidPost(1);
            dto.Id = 1;
            dto.Name = "Reshaped draft";
            dto.UserId = "user1";
            dto.Members = KeepingMember(40, "user1");

            await _service.UpdateAsync(dto);

            Assert.That(pending.Name, Is.EqualTo("Reshaped draft"));
        }

        [Test]
        public async Task Should_Let_An_Administrator_Rename_An_Approved_Group()
        {
            var approved = new GroupEntity
            {
                Id = 1, OrganizationId = 1, Name = "Existing", GroupTypeId = 1,
                Status = GroupStatus.Approved,
                Members = new List<GroupMember>()
            };

            _groupsDbSet.SetDbSetDataForAsync(new List<GroupEntity> { approved });

            var dto = ValidPost(2);
            dto.Id = 1;
            dto.Name = "Renamed by an admin";

            await _service.UpdateAsync(dto);

            Assert.Multiple(() =>
            {
                Assert.That(approved.Name, Is.EqualTo("Renamed by an admin"));
                Assert.That(approved.GroupTypeId, Is.EqualTo(2));
            });
        }

        private GroupEntity GroupWithMember(GroupMember member) => new GroupEntity
        {
            Id = 1,
            OrganizationId = 1,
            Name = "Existing",
            GroupTypeId = 1,
            Status = GroupStatus.Approved,
            Members = new List<GroupMember> { member }
        };

        private GroupPostDto EditPostFor(GroupEntity group)
        {
            var dto = ValidPost(1);
            dto.Id = group.Id;
            dto.Name = group.Name;
            dto.UserId = "user1";
            return dto;
        }

        [Test]
        public async Task Should_Keep_An_Existing_Member_Start_Date_When_A_Member_Edits()
        {
            _permissionService
                .UserHasPermissionAsync(Arg.Any<UserAndOrganizationDto>(), AdministrationPermissions.Groups)
                .Returns(false);

            var membership = new GroupMember
            {
                Id = 40, UserId = "user1", StartDate = new DateTime(2026, 1, 1)
            };

            var group = GroupWithMember(membership);
            _groupsDbSet.SetDbSetDataForAsync(new List<GroupEntity> { group });

            var dto = EditPostFor(group);
            dto.Members = new List<GroupMemberPostDto>
            {
                new GroupMemberPostDto
                {
                    MembershipId = 40, Id = "user1",
                    StartDate = new DateTime(2026, 7, 1),
                    Description = "Edited"
                }
            };

            await _service.UpdateAsync(dto);

            Assert.Multiple(() =>
            {
                Assert.That(membership.StartDate, Is.EqualTo(new DateTime(2026, 1, 1)));
                // Everything else on the row is still theirs to edit.
                Assert.That(membership.Description, Is.EqualTo("Edited"));
            });
        }

        [Test]
        public async Task Should_Let_An_Administrator_Change_A_Member_Start_Date()
        {
            var membership = new GroupMember
            {
                Id = 40, UserId = "user1", StartDate = new DateTime(2026, 1, 1)
            };

            var group = GroupWithMember(membership);
            _groupsDbSet.SetDbSetDataForAsync(new List<GroupEntity> { group });

            var dto = EditPostFor(group);
            dto.Members = new List<GroupMemberPostDto>
            {
                new GroupMemberPostDto { MembershipId = 40, Id = "user1", StartDate = new DateTime(2026, 7, 1) }
            };

            await _service.UpdateAsync(dto);

            Assert.That(membership.StartDate, Is.EqualTo(new DateTime(2026, 7, 1)));
        }

        [Test]
        public async Task Should_Let_A_Member_Set_A_Start_Date_That_Was_Not_There_Before()
        {
            _permissionService
                .UserHasPermissionAsync(Arg.Any<UserAndOrganizationDto>(), AdministrationPermissions.Groups)
                .Returns(false);

            var membership = new GroupMember { Id = 40, UserId = "user1" };

            var group = GroupWithMember(membership);
            _groupsDbSet.SetDbSetDataForAsync(new List<GroupEntity> { group });

            var dto = EditPostFor(group);
            dto.Members = new List<GroupMemberPostDto>
            {
                new GroupMemberPostDto { MembershipId = 40, Id = "user1", StartDate = new DateTime(2026, 7, 1) }
            };

            await _service.UpdateAsync(dto);

            Assert.That(membership.StartDate, Is.EqualTo(new DateTime(2026, 7, 1)));
        }

        [Test]
        public void Should_Throw_When_A_Member_Drops_A_Membership_That_Has_Started()
        {
            _permissionService
                .UserHasPermissionAsync(Arg.Any<UserAndOrganizationDto>(), AdministrationPermissions.Groups)
                .Returns(false);

            var group = GroupWithMember(new GroupMember
            {
                Id = 40, UserId = "user1", StartDate = DateTime.UtcNow.Date.AddMonths(-1)
            });

            _groupsDbSet.SetDbSetDataForAsync(new List<GroupEntity> { group });

            var dto = EditPostFor(group);
            dto.Members = new List<GroupMemberPostDto>();

            var ex = Assert.ThrowsAsync<ValidationException>(async () => await _service.UpdateAsync(dto));

            Assert.That(ex.ErrorCode, Is.EqualTo(ErrorCodes.GroupMemberCannotBeRemoved));
        }

        [Test]
        public async Task Should_Let_A_Member_Drop_A_Membership_That_Has_Not_Started_Yet()
        {
            _permissionService
                .UserHasPermissionAsync(Arg.Any<UserAndOrganizationDto>(), AdministrationPermissions.Groups)
                .Returns(false);

            // user1 is the editor and must be an active member; user2 has not started yet.
            var group = GroupWithMember(new GroupMember { Id = 40, UserId = "user1" });
            group.Members.Add(new GroupMember
            {
                Id = 41, UserId = "user2", StartDate = DateTime.UtcNow.Date.AddMonths(1)
            });

            _groupsDbSet.SetDbSetDataForAsync(new List<GroupEntity> { group });

            var dto = EditPostFor(group);
            dto.Members = new List<GroupMemberPostDto>
            {
                new GroupMemberPostDto { MembershipId = 40, Id = "user1" }
            };

            await _service.UpdateAsync(dto);

            Assert.That(group.Members.Select(m => m.UserId), Is.EquivalentTo(new[] { "user1" }));
        }

        [Test]
        public void Should_Throw_When_A_Member_Drops_An_Open_Ended_Membership()
        {
            _permissionService
                .UserHasPermissionAsync(Arg.Any<UserAndOrganizationDto>(), AdministrationPermissions.Groups)
                .Returns(false);

            // A membership with no start date is open-ended - it counts as a current member
            // everywhere else, so it cannot be dropped without a trace either.
            var group = GroupWithMember(new GroupMember { Id = 40, UserId = "user1" });
            group.Members.Add(new GroupMember { Id = 41, UserId = "user2" });

            _groupsDbSet.SetDbSetDataForAsync(new List<GroupEntity> { group });

            var dto = EditPostFor(group);
            dto.Members = new List<GroupMemberPostDto>
            {
                new GroupMemberPostDto { MembershipId = 40, Id = "user1" }
            };

            var ex = Assert.ThrowsAsync<ValidationException>(async () => await _service.UpdateAsync(dto));

            Assert.That(ex.ErrorCode, Is.EqualTo(ErrorCodes.GroupMemberCannotBeRemoved));
        }

        [Test]
        public async Task Should_Let_An_Administrator_Drop_A_Membership_That_Has_Started()
        {
            var group = GroupWithMember(new GroupMember
            {
                Id = 40, UserId = "user1", StartDate = DateTime.UtcNow.Date.AddMonths(-1)
            });

            _groupsDbSet.SetDbSetDataForAsync(new List<GroupEntity> { group });

            var dto = EditPostFor(group);
            dto.Members = new List<GroupMemberPostDto>();

            await _service.UpdateAsync(dto);

            Assert.That(group.Members, Is.Empty);
        }

        [Test]
        public async Task Should_Keep_The_Same_Membership_Row_Across_A_Save()
        {
            var membership = new GroupMember { Id = 40, UserId = "user1", Description = "Before" };

            var group = GroupWithMember(membership);
            _groupsDbSet.SetDbSetDataForAsync(new List<GroupEntity> { group });

            var dto = EditPostFor(group);
            dto.Members = new List<GroupMemberPostDto>
            {
                new GroupMemberPostDto { MembershipId = 40, Id = "user1", Description = "After" }
            };

            await _service.UpdateAsync(dto);

            // The stored row is updated in place rather than replaced, so its id survives.
            Assert.That(group.Members.Single(), Is.SameAs(membership));
            Assert.That(membership.Description, Is.EqualTo("After"));
        }

        [Test]
        public void Should_Throw_When_Editor_Is_Neither_Administrator_Nor_Member()
        {
            _permissionService
                .UserHasPermissionAsync(Arg.Any<UserAndOrganizationDto>(), AdministrationPermissions.Groups)
                .Returns(false);

            var dto = ValidPost(1);
            dto.Id = 1;
            dto.Name = "Existing";
            dto.UserId = "outsider";

            var ex = Assert.ThrowsAsync<ValidationException>(async () => await _service.UpdateAsync(dto));

            Assert.That(ex.ErrorCode, Is.EqualTo(ErrorCodes.GroupEditNotAllowed));
        }

        [Test]
        public async Task Should_Let_A_Group_Member_Edit_Their_Group()
        {
            _permissionService
                .UserHasPermissionAsync(Arg.Any<UserAndOrganizationDto>(), AdministrationPermissions.Groups)
                .Returns(false);

            var dto = ValidPost(1);
            dto.Id = 1;
            dto.Name = "Existing";
            dto.UserId = "user1";
            dto.Description = "Edited by a member";
            dto.Members = KeepingMember(40, "user1");

            await _service.UpdateAsync(dto);

            await _uow.Received(1).SaveChangesAsync("user1");
        }


        [Test]
        public async Task Should_Allow_A_Temporary_Group_Without_An_End_Date()
        {
            var dto = ValidPost(2);
            dto.StartDate = new DateTime(2026, 1, 1);

            await _service.CreateAsync(dto);

            _groupsDbSet.Received(1).Add(Arg.Is<GroupEntity>(g => g.EndDate == null));
        }

        [Test]
        public void Should_Throw_When_End_Date_Is_Before_Start_Date()
        {
            var dto = ValidPost(2);
            dto.StartDate = new DateTime(2026, 6, 1);
            dto.EndDate = new DateTime(2026, 1, 1);

            var ex = Assert.ThrowsAsync<ValidationException>(async () => await _service.CreateAsync(dto));

            Assert.That(ex.ErrorCode, Is.EqualTo(ErrorCodes.GroupEndDateBeforeStartDate));
        }

        [Test]
        public void Should_Throw_When_Member_End_Date_Is_Before_Their_Start_Date()
        {
            var dto = ValidPost(1);
            dto.Members = new List<GroupMemberPostDto>
            {
                new GroupMemberPostDto
                {
                    Id = "user1",
                    StartDate = new DateTime(2026, 6, 1),
                    EndDate = new DateTime(2026, 1, 1)
                }
            };

            var ex = Assert.ThrowsAsync<ValidationException>(async () => await _service.CreateAsync(dto));

            Assert.That(ex.ErrorCode, Is.EqualTo(ErrorCodes.GroupEndDateBeforeStartDate));
        }

        [Test]
        public async Task Should_Allow_Several_Memberships_For_The_Same_Person()
        {
            var dto = ValidPost(1);
            dto.Members = new List<GroupMemberPostDto>
            {
                new GroupMemberPostDto
                {
                    Id = "user1",
                    Description = "First stint",
                    EndDate = new DateTime(2026, 3, 31)
                },
                new GroupMemberPostDto
                {
                    Id = "user1",
                    Description = "Came back",
                    StartDate = new DateTime(2026, 9, 1)
                }
            };

            await _service.CreateAsync(dto);

            _groupsDbSet.Received(1).Add(Arg.Is<GroupEntity>(g =>
                g.Members.Count == 2 &&
                g.Members.All(m => m.UserId == "user1") &&
                g.Members.Any(m => m.Description == "First stint") &&
                g.Members.Any(m => m.Description == "Came back")));
        }

        [Test]
        public async Task Should_Return_Non_Public_References_To_A_Member()
        {
            _groupsDbSet.SetDbSetDataForAsync(new List<GroupEntity>
            {
                new GroupEntity
                {
                    Id = 7,
                    OrganizationId = 1,
                    Name = "Team",
                    GroupTypeId = 1,
                    Members = new List<GroupMember> { new GroupMember { UserId = "user1" } },
                    References = new List<GroupReference>
                    {
                        new GroupReference { Id = 1, Url = "https://public", IsPubliclyVisible = true },
                        new GroupReference { Id = 2, Url = "https://private", IsPubliclyVisible = false }
                    }
                }
            });

            var groups = await _service.GetAllAsync(new UserAndOrganizationDto { OrganizationId = 1, UserId = "user1" });

            Assert.That(groups.Single().References, Has.Count.EqualTo(2));
        }

        [Test]
        public async Task Should_Hide_Non_Public_References_From_Non_Members()
        {
            _groupsDbSet.SetDbSetDataForAsync(new List<GroupEntity>
            {
                new GroupEntity
                {
                    Id = 7,
                    OrganizationId = 1,
                    Name = "Team",
                    GroupTypeId = 1,
                    Members = new List<GroupMember> { new GroupMember { UserId = "user1" } },
                    References = new List<GroupReference>
                    {
                        new GroupReference { Id = 1, Url = "https://public", IsPubliclyVisible = true },
                        new GroupReference { Id = 2, Url = "https://private", IsPubliclyVisible = false }
                    }
                }
            });

            var groups = await _service.GetAllAsync(new UserAndOrganizationDto { OrganizationId = 1, UserId = "outsider" });

            Assert.That(groups.Single().References.Single().Url, Is.EqualTo("https://public"));
        }

        [Test]
        public async Task Should_Keep_Non_Public_References_When_A_Non_Member_Saves()
        {
            var group = new GroupEntity
            {
                Id = 7,
                OrganizationId = 1,
                Name = "Team",
                GroupTypeId = 1,
                Members = new List<GroupMember> { new GroupMember { UserId = "someone-else" } },
                References = new List<GroupReference>
                {
                    new GroupReference { Id = 1, Url = "https://public", IsPubliclyVisible = true },
                    new GroupReference { Id = 2, Url = "https://private", IsPubliclyVisible = false }
                }
            };

            _groupsDbSet.SetDbSetDataForAsync(new List<GroupEntity> { group });

            // An administrator who is not a member only ever saw the public reference,
            // so that is all their payload can contain.
            var dto = ValidPost(1);
            dto.Id = 7;
            dto.Name = "Team";
            dto.UserId = "admin";
            dto.References = new List<GroupReferenceDto>
            {
                new GroupReferenceDto { Url = "https://public", IsPubliclyVisible = true }
            };

            await _service.UpdateAsync(dto);

            Assert.That(group.References.Any(r => r.Url == "https://private"), Is.True);
        }

        [Test]
        public async Task Should_Persist_Group_References()
        {
            var dto = ValidPost(1);
            dto.References = new List<GroupReferenceDto>
            {
                new GroupReferenceDto { Url = "https://wiki/team", Name = "Wiki", IsPubliclyVisible = true },
                new GroupReferenceDto { Url = "https://internal/notes", IsPubliclyVisible = false }
            };

            await _service.CreateAsync(dto);

            _groupsDbSet.Received(1).Add(Arg.Is<GroupEntity>(g =>
                g.References.Count == 2 &&
                g.References.Any(r => r.Name == "Wiki" && r.IsPubliclyVisible) &&
                // A reference with no name falls back to its URL as the label.
                g.References.Any(r => r.Name == "https://internal/notes" && !r.IsPubliclyVisible)));
        }

        [Test]
        public async Task Should_Persist_Member_Start_And_End_Dates()
        {
            var dto = ValidPost(1);
            dto.Members = new List<GroupMemberPostDto>
            {
                new GroupMemberPostDto
                {
                    Id = "user1",
                    StartDate = new DateTime(2026, 1, 1),
                    EndDate = new DateTime(2026, 6, 30)
                }
            };

            await _service.CreateAsync(dto);

            _groupsDbSet.Received(1).Add(Arg.Is<GroupEntity>(g =>
                g.Members.Count == 1 &&
                g.Members.First().UserId == "user1" &&
                g.Members.First().StartDate == new DateTime(2026, 1, 1) &&
                g.Members.First().EndDate == new DateTime(2026, 6, 30)));
        }

        [Test]
        public async Task Should_Allow_Group_Without_Description()
        {
            var dto = ValidPost(1);
            dto.Description = null;

            await _service.CreateAsync(dto);

            _groupsDbSet.Received(1).Add(Arg.Is<GroupEntity>(g => g.Description == null));
        }

        [Test]
        public void Should_Throw_When_Name_Already_Exists_In_Organization()
        {
            var dto = ValidPost(1);
            dto.Name = "Existing";

            var ex = Assert.ThrowsAsync<ValidationException>(async () => await _service.CreateAsync(dto));

            Assert.That(ex.ErrorCode, Is.EqualTo(ErrorCodes.GroupNameAlreadyExists));
        }

    }
}
