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
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.Exceptions;
using Shrooms.Contracts.Infrastructure;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.Domain.Services.Permissions;
using Shrooms.Premium.DataTransferObjects.Models.ServiceRequest;
using Shrooms.Premium.Domain.Services.ServiceRequests;
using Shrooms.Tests.Extensions;

namespace Shrooms.Premium.Tests.DomainService
{
    [TestFixture]
    public class ServiceRequestServiceTests
    {
        private IUnitOfWork2 _uow;
        private IServiceRequestService _serviceRequestService;
        private DbSet<ServiceRequest> _serviceRequestsDbSet;
        private DbSet<ServiceRequestComment> _serviceRequestCommentsDbSet;
        private DbSet<ServiceRequestCategory> _serviceRequestCategoryDbSet;
        private DbSet<ServiceRequestPriority> _serviceRequestPriorityDbSet;
        private DbSet<ServiceRequestStatus> _serviceRequestStatusDbSet;
        private DbSet<ApplicationUser> _usersDbSet;
        private IPermissionService _permissionService;

        [SetUp]
        public void Init()
        {
            _uow = Substitute.For<IUnitOfWork2>();

            _usersDbSet = Substitute.For<DbSet<ApplicationUser>, IQueryable<ApplicationUser>, IDbAsyncEnumerable<ApplicationUser>>();
            _uow.GetDbSet<ApplicationUser>().Returns(_usersDbSet);

            _serviceRequestsDbSet = Substitute.For<DbSet<ServiceRequest>, IQueryable<ServiceRequest>, IDbAsyncEnumerable<ServiceRequest>>();
            _uow.GetDbSet<ServiceRequest>().Returns(_serviceRequestsDbSet);

            _serviceRequestCommentsDbSet = Substitute.For<DbSet<ServiceRequestComment>, IQueryable<ServiceRequestComment>, IDbAsyncEnumerable<ServiceRequestComment>>();
            _uow.GetDbSet<ServiceRequestComment>().Returns(_serviceRequestCommentsDbSet);

            _serviceRequestCategoryDbSet = Substitute.For<DbSet<ServiceRequestCategory>, IQueryable<ServiceRequestCategory>, IDbAsyncEnumerable<ServiceRequestCategory>>();
            _uow.GetDbSet<ServiceRequestCategory>().Returns(_serviceRequestCategoryDbSet);

            _serviceRequestPriorityDbSet = Substitute.For<DbSet<ServiceRequestPriority>, IQueryable<ServiceRequestPriority>, IDbAsyncEnumerable<ServiceRequestPriority>>();
            _uow.GetDbSet<ServiceRequestPriority>().Returns(_serviceRequestPriorityDbSet);

            _serviceRequestStatusDbSet = Substitute.For<DbSet<ServiceRequestStatus>, IQueryable<ServiceRequestStatus>, IDbAsyncEnumerable<ServiceRequestStatus>>();
            _uow.GetDbSet<ServiceRequestStatus>().Returns(_serviceRequestStatusDbSet);

            _permissionService = Substitute.For<IPermissionService>();
            var asyncRunner = Substitute.For<IAsyncRunner>();
            _serviceRequestService = new ServiceRequestService(_uow, _permissionService, asyncRunner);
        }

        [Test]
        public async Task Should_Return_Successfully_Created_Service_Request_Comment()
        {
            MockServiceRequests();

            var comment = new ServiceRequestCommentDto
            {
                Content = "test content",
                ServiceRequestId = 1
            };

            var userAndOrg = new UserAndOrganizationDto
            {
                OrganizationId = 1,
                UserId = "UserId"
            };

            await _serviceRequestService.CreateCommentAsync(comment, userAndOrg);
            _serviceRequestCommentsDbSet.Received(1).Add(Arg.Any<ServiceRequestComment>());
            await _uow.Received(1).SaveChangesAsync(false);
        }

        [Test]
        public void Should_Throw_When_Service_Request_Does_Not_Exist()
        {
            MockServiceRequests();

            var comment = new ServiceRequestCommentDto
            {
                Content = "test content",
                ServiceRequestId = 1
            };

            var userAndOrg = new UserAndOrganizationDto
            {
                OrganizationId = 2,
                UserId = "UserId"
            };

            Assert.ThrowsAsync<ValidationException>(async () => await _serviceRequestService.CreateCommentAsync(comment, userAndOrg));
        }

        [Test]
        public async Task Should_Return_Successfully_Created_Service_Request()
        {
            MockServiceRequestCategories();
            MockServiceRequestPriorities();
            MockServiceRequestStatuses();

            var serviceRequestDto = new ServiceRequestDto
            {
                Description = "testDescription",
                PriorityId = 1,
                ServiceRequestCategoryId = 1,
                Title = "tetsTitle"
            };

            var userAndOrg = new UserAndOrganizationDto
            {
                OrganizationId = 2,
                UserId = "UserId"
            };

            await _serviceRequestService.CreateNewServiceRequestAsync(serviceRequestDto, userAndOrg);
            _serviceRequestsDbSet.Received(1).Add(Arg.Any<ServiceRequest>());
            await _uow.Received(1).SaveChangesAsync(false);
        }

        [Test]
        public void Should_Throw_When_Priority_Id_Does_Not_Exist()
        {
            MockServiceRequestCategories();
            MockServiceRequestPriorities();
            MockServiceRequestStatuses();

            var serviceRequestDto = new ServiceRequestDto
            {
                Description = "testDescription",
                PriorityId = 2,
                ServiceRequestCategoryId = 1,
                Title = "tetsTitle"
            };

            var userAndOrg = new UserAndOrganizationDto
            {
                OrganizationId = 2,
                UserId = "UserId"
            };

            Assert.ThrowsAsync<ValidationException>(async () => await _serviceRequestService.CreateNewServiceRequestAsync(serviceRequestDto, userAndOrg));
        }

        [Test]
        public void Should_Throw_When_Category_Id_Does_Not_Exist()
        {
            MockServiceRequestCategories();
            MockServiceRequestPriorities();
            MockServiceRequestStatuses();

            var serviceRequestDto = new ServiceRequestDto
            {
                Description = "testDescription",
                PriorityId = 1,
                ServiceRequestCategoryId = 21,
                Title = "tetsTitle"
            };

            var userAndOrg = new UserAndOrganizationDto
            {
                OrganizationId = 2,
                UserId = "UserId"
            };

            Assert.ThrowsAsync<ValidationException>(async () => await _serviceRequestService.CreateNewServiceRequestAsync(serviceRequestDto, userAndOrg));
        }

        [Test]
        public async Task Should_Return_Successfully_Updated_Service_Request_As_Admin()
        {
            MockServiceRequestForUpdate();
            MockServiceRequestCategories();
            MockServiceRequestPriorities();
            MockServiceRequestStatuses();
            MockPermissioService();

            var serviceRequestDto = new ServiceRequestDto
            {
                Id = 1,
                Description = "testDescription",
                PriorityId = 1,
                ServiceRequestCategoryId = 1,
                Title = "tetsTitle",
                StatusId = 1,
                KudosAmmount = 1
            };

            var userAndOrg = new UserAndOrganizationDto
            {
                OrganizationId = 1,
                UserId = "AdminId"
            };

            await _serviceRequestService.UpdateServiceRequestAsync(serviceRequestDto, userAndOrg);

            var updatedServiceRequest = await _serviceRequestsDbSet.FirstAsync(x => x.Id == serviceRequestDto.Id);

            Assert.AreEqual("test1", updatedServiceRequest.CategoryName);
            Assert.AreEqual(serviceRequestDto.Title, updatedServiceRequest.Title);
            Assert.AreEqual(serviceRequestDto.PriorityId, updatedServiceRequest.PriorityId);
            Assert.AreEqual(serviceRequestDto.KudosAmmount, updatedServiceRequest.KudosAmmount);

            await _uow.Received(1).SaveChangesAsync(false);
        }

        [Test]
        public async Task Should_Return_Successfully_Updated_Service_Request_As_Request_Creator()
        {
            MockServiceRequestForUpdate();
            MockServiceRequestCategories();
            MockServiceRequestPriorities();
            MockServiceRequestStatuses();
            MockPermissioService();

            var serviceRequestDto = new ServiceRequestDto
            {
                Id = 1,
                Description = "testDescription",
                PriorityId = 1,
                ServiceRequestCategoryId = 1,
                Title = "tetsTitle",
                StatusId = 2
            };

            var userAndOrg = new UserAndOrganizationDto
            {
                OrganizationId = 1,
                UserId = "UserId"
            };

            await _serviceRequestService.UpdateServiceRequestAsync(serviceRequestDto, userAndOrg);

            var updatedServiceRequest = await _serviceRequestsDbSet.FirstAsync(x => x.Id == serviceRequestDto.Id);

            Assert.AreEqual(null, updatedServiceRequest.CategoryName);
            Assert.AreEqual(null, updatedServiceRequest.Title);
            Assert.AreEqual(serviceRequestDto.PriorityId, updatedServiceRequest.PriorityId);
            Assert.AreEqual(null, updatedServiceRequest.KudosAmmount);
            Assert.AreEqual(1, updatedServiceRequest.StatusId);

            await _uow.Received(1).SaveChangesAsync(false);
        }

        [Test]
        public void Should_Throw_When_Changing_Category_From_Kudos_To_Others()
        {
            // Arrange
            MockServiceRequestForUpdate();
            MockServiceRequestCategories();
            MockServiceRequestPriorities();
            MockServiceRequestStatuses();
            MockPermissioService();

            var userAndOrg = new UserAndOrganizationDto
            {
                OrganizationId = 1,
                UserId = "AdminId"
            };

            var serviceRequestDto = new ServiceRequestDto
            {
                Id = 2,
                Description = "testDescription",
                PriorityId = 1,
                ServiceRequestCategoryId = 1,
                Title = "tetsTitle",
                StatusId = 2
            };

            // Assert
            Assert.ThrowsAsync<ValidationException>(async () => await _serviceRequestService.UpdateServiceRequestAsync(serviceRequestDto, userAndOrg));
        }

        [Test]
        public void Should_Throw_When_Changing_Category_From_Others_To_Kudos()
        {
            // Arrange
            MockServiceRequestForUpdate();
            MockServiceRequestCategories();
            MockServiceRequestPriorities();
            MockServiceRequestStatuses();
            MockPermissioService();

            var userAndOrg = new UserAndOrganizationDto
            {
                OrganizationId = 1,
                UserId = "AdminId"
            };

            var serviceRequestDto = new ServiceRequestDto
            {
                Id = 1,
                Description = "testDescription",
                PriorityId = 1,
                ServiceRequestCategoryId = 2,
                Title = "tetsTitle",
                StatusId = 2
            };

            // Assert
            Assert.ThrowsAsync<ValidationException>(async () => await _serviceRequestService.UpdateServiceRequestAsync(serviceRequestDto, userAndOrg));
        }

        [Test]
        public async Task Should_Return_Successfully_Updated_Service_Request_Status()
        {
            MockServiceRequestForUpdate();
            MockServiceRequestCategories();
            MockServiceRequestPriorities();
            MockServiceRequestStatuses();
            MockPermissioService();

            var serviceRequestDto = new ServiceRequestDto
            {
                Id = 1,
                Description = "testDescription",
                PriorityId = 1,
                ServiceRequestCategoryId = 1,
                Title = "tetsTitle",
                StatusId = 2
            };

            var userAndOrg = new UserAndOrganizationDto
            {
                OrganizationId = 1,
                UserId = "AdminId"
            };

            await _serviceRequestService.UpdateServiceRequestAsync(serviceRequestDto, userAndOrg);

            var updatedServiceRequest = await _serviceRequestsDbSet.FirstAsync(x => x.Id == serviceRequestDto.Id);

            Assert.AreEqual(serviceRequestDto.StatusId, updatedServiceRequest.StatusId);

            await _uow.Received(1).SaveChangesAsync(false);
        }

        [Test]
        public void Should_Throw_When_User_Has_No_Permission_To_Update_Service_Request()
        {
            MockServiceRequestForUpdate();
            MockServiceRequestCategories();
            MockServiceRequestPriorities();
            MockServiceRequestStatuses();
            MockPermissioService();

            var serviceRequestDto = new ServiceRequestDto
            {
                Id = 1,
                Description = "testDescription",
                PriorityId = 1,
                ServiceRequestCategoryId = 1,
                Title = "tetsTitle",
                StatusId = 2
            };

            var userAndOrg = new UserAndOrganizationDto
            {
                OrganizationId = 1,
                UserId = "AnotherUserId"
            };

            Assert.ThrowsAsync<UnauthorizedAccessException>(async () => await _serviceRequestService.UpdateServiceRequestAsync(serviceRequestDto, userAndOrg));
        }

        [Test]
        public void Should_Throw_When_Editing_Kudos_Finished_Service_Request()
        {
            MockServiceRequestForUpdate();
            MockServiceRequestCategories();
            MockServiceRequestPriorities();
            MockServiceRequestStatuses();
            MockPermissioService();

            var serviceRequestDto = new ServiceRequestDto
            {
                Id = 2,
                Description = "testDescription",
                PriorityId = 1,
                ServiceRequestCategoryId = 1,
                Title = "tetsTitle",
                StatusId = 1
            };

            var userAndOrg = new UserAndOrganizationDto
            {
                OrganizationId = 1,
                UserId = "AdminId"
            };

            Assert.ThrowsAsync<ValidationException>(async () => await _serviceRequestService.UpdateServiceRequestAsync(serviceRequestDto, userAndOrg));
        }

        private void MockPermissioService()
        {
            _permissionService
                .UserHasPermissionAsync(Arg.Is<UserAndOrganizationDto>(x => x.UserId == "AdminId" && x.OrganizationId == 1), AdministrationPermissions.ServiceRequest)
                .Returns(true);

            _permissionService
                .UserHasPermissionAsync(Arg.Is<UserAndOrganizationDto>(x => x.UserId == "UserId" && x.OrganizationId == 1), AdministrationPermissions.ServiceRequest)
                .Returns(false);

            _permissionService
               .UserHasPermissionAsync(Arg.Is<UserAndOrganizationDto>(x => x.UserId == "AnotherUserId" && x.OrganizationId == 1), AdministrationPermissions.ServiceRequest)
               .Returns(false);
        }

        private void MockServiceRequestForUpdate()
        {
            var serviceRequests = new List<ServiceRequest>
            {
                new ServiceRequest
                {
                    Id = 1,
                    OrganizationId = 1,
                    Status = new ServiceRequestStatus
                    {
                        Title = "Open"
                    },
                    StatusId = 1,
                    EmployeeId = "UserId"
                },
                new ServiceRequest
                {
                    Id = 2,
                    OrganizationId = 1,
                    Status = new ServiceRequestStatus
                    {
                        Title = "Done"
                    },
                    CategoryName = "Kudos",
                    StatusId = 1,
                    EmployeeId = "UserId"
                }
            }.AsQueryable();

            _serviceRequestsDbSet.SetDbSetDataForAsync(serviceRequests);
        }

        private void MockServiceRequests()
        {
            var serviceRequests = new List<ServiceRequest>
            {
                new ServiceRequest
                {
                    Id = 1,
                    OrganizationId = 1
                }
            }.AsQueryable();

            _serviceRequestsDbSet.SetDbSetDataForAsync(serviceRequests);
        }

        private void MockServiceRequestCategories()
        {
            var serviceRequestCategories = new List<ServiceRequestCategory>
            {
                new ServiceRequestCategory
                {
                    Id = 1,
                    Name = "test1"
                },
                new ServiceRequestCategory
                {
                    Id = 2,
                    Name = "Kudos"
                }
            }.AsQueryable();

            _serviceRequestCategoryDbSet.SetDbSetDataForAsync(serviceRequestCategories);
        }

        private void MockServiceRequestPriorities()
        {
            var serviceRequestPrarioty = new List<ServiceRequestPriority>
            {
                new ServiceRequestPriority
                {
                    Id = 1,
                    Title = "test1"
                }
            }.AsQueryable();

            _serviceRequestPriorityDbSet.SetDbSetDataForAsync(serviceRequestPrarioty);
        }

        private void MockServiceRequestStatuses()
        {
            var serviceRequestStatuses = new List<ServiceRequestStatus>
            {
                new ServiceRequestStatus
                {
                    Id = 1,
                    Title = "Open"
                },
                new ServiceRequestStatus
                {
                    Id = 2,
                    Title = "InProgress"
                },
                new ServiceRequestStatus
                {
                    Id = 3,
                    Title = "Done"
                }
            }.AsQueryable();

            _serviceRequestStatusDbSet.SetDbSetDataForAsync(serviceRequestStatuses);
        }
    }
}