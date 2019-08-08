using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using NSubstitute;
using NUnit.Framework;
using Shrooms.Constants.Authorization.Permissions;
using Shrooms.DataLayer.DAL;
using Shrooms.DataTransferObjects.Models;
using Shrooms.DataTransferObjects.Models.ServiceRequest;
using Shrooms.Domain.Services.Email.ServiceRequest;
using Shrooms.Domain.Services.Permissions;
using Shrooms.Domain.Services.ServiceRequests;
using Shrooms.DomainExceptions.Exceptions;
using Shrooms.EntityModels.Models;
using Shrooms.Infrastructure.Configuration;
using Shrooms.Infrastructure.Email;
using Shrooms.Infrastructure.FireAndForget;
using Shrooms.UnitTests.Extensions;

namespace Shrooms.UnitTests.DomainService
{
    [TestFixture]
    public class ServiceRequestServiceTests
    {
        private IUnitOfWork2 _uow;
        private IServiceRequestService _serviceRequestService;
        private IDbSet<ServiceRequest> _serviceRequestsDbSet;
        private IDbSet<ServiceRequestComment> _serviceRequestCommentsDbSet;
        private IDbSet<ServiceRequestCategory> _serviceRequestCategoryDbSet;
        private IDbSet<ServiceRequestPriority> _serviceRequestPriorityDbSet;
        private IDbSet<ServiceRequestStatus> _serviceRequestStatusDbSet;
        private IDbSet<ApplicationUser> _usersDbSet;
        private IPermissionService _permissionService;
        private IServiceRequestNotificationService _notificationService;

        [SetUp]
        public void Init()
        {
            _uow = Substitute.For<IUnitOfWork2>();

            _usersDbSet = Substitute.For<IDbSet<ApplicationUser>>();
            _uow.GetDbSet<ApplicationUser>().Returns(_usersDbSet);

            _serviceRequestsDbSet = Substitute.For<IDbSet<ServiceRequest>>();
            _uow.GetDbSet<ServiceRequest>().Returns(_serviceRequestsDbSet);

            _serviceRequestCommentsDbSet = Substitute.For<IDbSet<ServiceRequestComment>>();
            _uow.GetDbSet<ServiceRequestComment>().Returns(_serviceRequestCommentsDbSet);

            _serviceRequestCategoryDbSet = Substitute.For<IDbSet<ServiceRequestCategory>>();
            _uow.GetDbSet<ServiceRequestCategory>().Returns(_serviceRequestCategoryDbSet);

            _serviceRequestPriorityDbSet = Substitute.For<IDbSet<ServiceRequestPriority>>();
            _uow.GetDbSet<ServiceRequestPriority>().Returns(_serviceRequestPriorityDbSet);

            _serviceRequestStatusDbSet = Substitute.For<IDbSet<ServiceRequestStatus>>();
            _uow.GetDbSet<ServiceRequestStatus>().Returns(_serviceRequestStatusDbSet);

            var mailingService = Substitute.For<IMailingService>();
            var appSettings = Substitute.For<IApplicationSettings>();
            _notificationService = Substitute.For<IServiceRequestNotificationService>();
            _permissionService = Substitute.For<IPermissionService>();
            var asyncRunner = Substitute.For<IAsyncRunner>();
            _serviceRequestService = new ServiceRequestService(_uow, _permissionService, asyncRunner);
        }

        [Test]
        public void Should_Return_Successfully_Created_Service_Request_Comment()
        {
            MockServiceRequests();

            var comment = new ServiceRequestCommentDTO()
            {
                Content = "test content",
                ServiceRequestId = 1
            };

            var userAndOrg = new UserAndOrganizationDTO()
            {
                OrganizationId = 1,
                UserId = "UserId"
            };

            _serviceRequestService.CreateComment(comment, userAndOrg);
            _serviceRequestCommentsDbSet
                .Received(1)
                .Add(Arg.Any<ServiceRequestComment>());
            _uow.Received(1).SaveChanges(false);
        }

        [Test]
        public void Should_Throw_When_Service_Request_Does_Not_Exist()
        {
            MockServiceRequests();

            var comment = new ServiceRequestCommentDTO()
            {
                Content = "test content",
                ServiceRequestId = 1
            };

            var userAndOrg = new UserAndOrganizationDTO()
            {
                OrganizationId = 2,
                UserId = "UserId"
            };

            Assert.Throws<ValidationException>(() => _serviceRequestService.CreateComment(comment, userAndOrg));
        }

        [Test]
        public void Should_Return_Successfully_Created_Service_Request()
        {
            MockServiceRequestCategories();
            MockServiceRequestPriorities();
            MockServiceRequestStatuses();

            var serviceRequestDTO = new ServiceRequestDTO()
            {
                Description = "testDescription",
                PriorityId = 1,
                ServiceRequestCategoryId = 1,
                Title = "tetsTitle"
            };

            var userAndOrg = new UserAndOrganizationDTO()
            {
                OrganizationId = 2,
                UserId = "UserId"
            };

            _serviceRequestService.CreateNewServiceRequest(serviceRequestDTO, userAndOrg);
            _serviceRequestsDbSet
                .Received(1)
                .Add(Arg.Any<ServiceRequest>());
            _uow.Received(1).SaveChanges(false);
        }

        [Test]
        public void Should_Throw_When_Priority_Id_Does_Not_Exist()
        {
            MockServiceRequestCategories();
            MockServiceRequestPriorities();
            MockServiceRequestStatuses();

            var serviceRequestDTO = new ServiceRequestDTO()
            {
                Description = "testDescription",
                PriorityId = 2,
                ServiceRequestCategoryId = 1,
                Title = "tetsTitle"
            };

            var userAndOrg = new UserAndOrganizationDTO()
            {
                OrganizationId = 2,
                UserId = "UserId"
            };

            Assert.Throws<ValidationException>(() => _serviceRequestService.CreateNewServiceRequest(serviceRequestDTO, userAndOrg));
        }

        [Test]
        public void Should_Throw_When_Category_Id_Does_Not_Exist()
        {
            MockServiceRequestCategories();
            MockServiceRequestPriorities();
            MockServiceRequestStatuses();

            var serviceRequestDTO = new ServiceRequestDTO()
            {
                Description = "testDescription",
                PriorityId = 1,
                ServiceRequestCategoryId = 21,
                Title = "tetsTitle"
            };

            var userAndOrg = new UserAndOrganizationDTO()
            {
                OrganizationId = 2,
                UserId = "UserId"
            };

            Assert.Throws<ValidationException>(() => _serviceRequestService.CreateNewServiceRequest(serviceRequestDTO, userAndOrg));
        }

        [Test]
        public void Should_Return_Successfully_Updated_Service_Request_As_Admin()
        {
            MockServiceRequestForUpdate();
            MockServiceRequestCategories();
            MockServiceRequestPriorities();
            MockServiceRequestStatuses();
            MockPermissioService();

            var serviceRequestDTO = new ServiceRequestDTO()
            {
                Id = 1,
                Description = "testDescription",
                PriorityId = 1,
                ServiceRequestCategoryId = 1,
                Title = "tetsTitle",
                StatusId = 1,
                KudosAmmount = 1
            };

            var userAndOrg = new UserAndOrganizationDTO()
            {
                OrganizationId = 1,
                UserId = "AdminId"
            };

            _serviceRequestService.UpdateServiceRequest(serviceRequestDTO, userAndOrg);

            var updatedServiceRequest = _serviceRequestsDbSet.Where(x => x.Id == serviceRequestDTO.Id).First();

            Assert.AreEqual("test1", updatedServiceRequest.CategoryName);
            Assert.AreEqual(serviceRequestDTO.Title, updatedServiceRequest.Title);
            Assert.AreEqual(serviceRequestDTO.PriorityId, updatedServiceRequest.PriorityId);
            Assert.AreEqual(serviceRequestDTO.KudosAmmount, updatedServiceRequest.KudosAmmount);

            _uow.Received(1).SaveChanges(false);
        }

        [Test]
        public void Should_Return_Successfully_Updated_Service_Request_As_Request_Creator()
        {
            MockServiceRequestForUpdate();
            MockServiceRequestCategories();
            MockServiceRequestPriorities();
            MockServiceRequestStatuses();
            MockPermissioService();

            var serviceRequestDTO = new ServiceRequestDTO()
            {
                Id = 1,
                Description = "testDescription",
                PriorityId = 1,
                ServiceRequestCategoryId = 1,
                Title = "tetsTitle",
                StatusId = 2
            };

            var userAndOrg = new UserAndOrganizationDTO()
            {
                OrganizationId = 1,
                UserId = "UserId"
            };

            _serviceRequestService.UpdateServiceRequest(serviceRequestDTO, userAndOrg);

            var updatedServiceRequest = _serviceRequestsDbSet.Where(x => x.Id == serviceRequestDTO.Id).First();

            Assert.AreEqual(null, updatedServiceRequest.CategoryName);
            Assert.AreEqual(null, updatedServiceRequest.Title);
            Assert.AreEqual(serviceRequestDTO.PriorityId, updatedServiceRequest.PriorityId);
            Assert.AreEqual(null, updatedServiceRequest.KudosAmmount);
            Assert.AreEqual(1, updatedServiceRequest.StatusId);

            _uow.Received(1).SaveChanges(false);
        }

        [Test]
        public void Should_Return_Successfully_Updated_Service_Request_Status()
        {
            MockServiceRequestForUpdate();
            MockServiceRequestCategories();
            MockServiceRequestPriorities();
            MockServiceRequestStatuses();
            MockPermissioService();

            var serviceRequestDTO = new ServiceRequestDTO()
            {
                Id = 1,
                Description = "testDescription",
                PriorityId = 1,
                ServiceRequestCategoryId = 1,
                Title = "tetsTitle",
                StatusId = 2
            };

            var userAndOrg = new UserAndOrganizationDTO()
            {
                OrganizationId = 1,
                UserId = "AdminId"
            };

            _serviceRequestService.UpdateServiceRequest(serviceRequestDTO, userAndOrg);

            var updatedServiceRequest = _serviceRequestsDbSet.Where(x => x.Id == serviceRequestDTO.Id).First();

            Assert.AreEqual(serviceRequestDTO.StatusId, updatedServiceRequest.StatusId);

            _uow.Received(1).SaveChanges(false);
        }

        [Test]
        public void Should_Throw_When_User_Has_No_Permission_To_Update_Service_Request()
        {
            MockServiceRequestForUpdate();
            MockServiceRequestCategories();
            MockServiceRequestPriorities();
            MockServiceRequestStatuses();
            MockPermissioService();

            var serviceRequestDTO = new ServiceRequestDTO()
            {
                Id = 1,
                Description = "testDescription",
                PriorityId = 1,
                ServiceRequestCategoryId = 1,
                Title = "tetsTitle",
                StatusId = 2
            };

            var userAndOrg = new UserAndOrganizationDTO()
            {
                OrganizationId = 1,
                UserId = "AnotherUserId"
            };

            Assert.Throws<UnauthorizedAccessException>(() => _serviceRequestService.UpdateServiceRequest(serviceRequestDTO, userAndOrg));
        }

        [Test]
        public void Should_Throw_When_Editing_Kudos_Finished_Service_Request()
        {
            MockServiceRequestForUpdate();
            MockServiceRequestCategories();
            MockServiceRequestPriorities();
            MockServiceRequestStatuses();
            MockPermissioService();

            var serviceRequestDTO = new ServiceRequestDTO()
            {
                Id = 2,
                Description = "testDescription",
                PriorityId = 1,
                ServiceRequestCategoryId = 1,
                Title = "tetsTitle",
                StatusId = 1
            };

            var userAndOrg = new UserAndOrganizationDTO()
            {
                OrganizationId = 1,
                UserId = "AdminId"
            };

            Assert.Throws<ValidationException>(() => _serviceRequestService.UpdateServiceRequest(serviceRequestDTO, userAndOrg));
        }

        private void MockPermissioService()
        {
            _permissionService
                .UserHasPermission(Arg.Is<UserAndOrganizationDTO>(x => x.UserId == "AdminId" && x.OrganizationId == 1), AdministrationPermissions.ServiceRequest)
                .Returns(true);

            _permissionService
                .UserHasPermission(Arg.Is<UserAndOrganizationDTO>(x => x.UserId == "UserId" && x.OrganizationId == 1), AdministrationPermissions.ServiceRequest)
                .Returns(false);

            _permissionService
               .UserHasPermission(Arg.Is<UserAndOrganizationDTO>(x => x.UserId == "AnotherUserId" && x.OrganizationId == 1), AdministrationPermissions.ServiceRequest)
               .Returns(false);
        }

        private void MockServiceRequestForUpdate()
        {
            var serviceRequests = new List<ServiceRequest>()
            {
                new ServiceRequest()
                {
                    Id = 1,
                    OrganizationId = 1,
                    Status = new ServiceRequestStatus()
                    {
                        Title = "Open"
                    },
                    StatusId = 1,
                    EmployeeId = "UserId"
                },
                new ServiceRequest()
                {
                    Id = 2,
                    OrganizationId = 1,
                    Status = new ServiceRequestStatus()
                    {
                        Title = "Done"
                    },
                    CategoryName = "Kudos",
                    StatusId = 1,
                    EmployeeId = "UserId"
                }
            }.AsQueryable();

            _serviceRequestsDbSet.SetDbSetData(serviceRequests);
        }

        private void MockServiceRequests()
        {
            var serviceRequests = new List<ServiceRequest>()
            {
                new ServiceRequest()
                {
                    Id = 1,
                    OrganizationId = 1
                }
            }.AsQueryable();

            _serviceRequestsDbSet.SetDbSetData(serviceRequests);
        }

        private void MockServiceRequestCategories()
        {
            var serviceRequestCategories = new List<ServiceRequestCategory>()
            {
                new ServiceRequestCategory()
                {
                    Id = 1,
                    Name = "test1"
                },
                new ServiceRequestCategory()
                {
                    Id = 2,
                    Name = "Kudos"
                }
            }.AsQueryable();

            _serviceRequestCategoryDbSet.SetDbSetData(serviceRequestCategories);
        }

        private void MockServiceRequestPriorities()
        {
            var serviceRequestPrarioty = new List<ServiceRequestPriority>()
            {
                new ServiceRequestPriority()
                {
                    Id = 1,
                    Title = "test1"
                }
            }.AsQueryable();

            _serviceRequestPriorityDbSet.SetDbSetData(serviceRequestPrarioty);
        }

        private void MockServiceRequestStatuses()
        {
            var serviceRequestStatuses = new List<ServiceRequestStatus>()
            {
                new ServiceRequestStatus()
                {
                    Id = 1,
                    Title = "Open"
                },
                new ServiceRequestStatus()
                {
                    Id = 2,
                    Title = "InProgress"
                },
                new ServiceRequestStatus()
                {
                    Id = 3,
                    Title = "Done"
                }
            }.AsQueryable();

            _serviceRequestStatusDbSet.SetDbSetData(serviceRequestStatuses);
        }
    }
}