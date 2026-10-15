using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.Exceptions;
using Shrooms.DataLayer.EntityModels.Models.TimeRegistration;
using Shrooms.Premium.DataTransferObjects.Models.TimeRegistration;
using Shrooms.Premium.Domain.Services.TimeRegistration;

namespace Shrooms.Premium.Tests.DomainService.TimeRegistration
{
    [TestFixture]
    public class TimeAccountingServiceTests : TimeRegistrationTestBase
    {
        private TimeAccountingService _service;

        [SetUp]
        public void SetUp()
        {
            _service = new TimeAccountingService(Uow);
        }

        [Test]
        public void SaveCustomerAsync_RejectsADuplicateNameIgnoringCase()
        {
            var error = Assert.ThrowsAsync<ValidationException>(() =>
                _service.SaveCustomerAsync(new TimeCustomerInputDto { Name = "  northwind " }, AsBoss));

            Assert.That(error.ErrorCode, Is.EqualTo(ErrorCodes.TimeRegistrationDuplicate));
        }

        [Test]
        public async Task GetCustomersAsync_CountsProjectsAndDistinctMembers()
        {
            var customers = (await _service.GetCustomersAsync(AsBoss)).ToList();

            Assert.That(customers.Select(customer => customer.Name), Is.EqualTo(new[] { "Contoso", "Northwind" }));
            Assert.That(customers[1].ProjectCount, Is.EqualTo(1));
            Assert.That(customers[1].MemberCount, Is.EqualTo(2));
        }

        [Test]
        public void SaveProjectAsync_RequiresExactlyOneDefaultTask()
        {
            var input = new TimeProjectInputDto
            {
                CustomerId = 1,
                Name = "Portal",
                Tasks = new List<TimeTaskInputDto> { new TimeTaskInputDto { Name = "Design" }, new TimeTaskInputDto { Name = "Build" } },
                MemberIds = new List<string>()
            };

            var error = Assert.ThrowsAsync<ValidationException>(() => _service.SaveProjectAsync(input, AsBoss));

            Assert.That(error.ErrorMessage, Is.EqualTo("Exactly one task must be the default"));
        }

        [Test]
        public async Task SaveProjectAsync_SyncsTasksAndMembers()
        {
            var input = new TimeProjectInputDto
            {
                Id = ShopId,
                CustomerId = 1,
                Name = "Shop",
                Tasks = new List<TimeTaskInputDto>
                {
                    new TimeTaskInputDto { Id = DevelopmentId, Name = "Development", IsDefault = true },
                    new TimeTaskInputDto { Name = "Testing" }
                },
                MemberIds = new List<string> { Ona, Boss }
            };

            await _service.SaveProjectAsync(input, AsBoss);

            var tasks = await Db.Set<TimeTask>().Where(task => task.ProjectId == ShopId).Select(task => task.Name).ToListAsync();
            var members = await Db.Set<TimeProjectMember>().Where(member => member.ProjectId == ShopId).Select(member => member.UserId).ToListAsync();
            Assert.That(tasks, Is.EquivalentTo(new[] { "Development", "Testing" }));
            Assert.That(members, Is.EquivalentTo(new[] { Ona, Boss }));
        }

        [Test]
        public void SaveProjectAsync_RefusesToRemoveATaskWithHours()
        {
            AddEntry(Ona, ShopId, ReviewId, new System.DateTime(2026, 10, 1), 60);
            var input = new TimeProjectInputDto
            {
                Id = ShopId,
                CustomerId = 1,
                Name = "Shop",
                Tasks = new List<TimeTaskInputDto> { new TimeTaskInputDto { Id = DevelopmentId, Name = "Development", IsDefault = true } },
                MemberIds = new List<string> { Ona }
            };

            var error = Assert.ThrowsAsync<ValidationException>(() => _service.SaveProjectAsync(input, AsBoss));

            Assert.That(error.ErrorCode, Is.EqualTo(ErrorCodes.TimeRegistrationInUse));
        }

        [Test]
        public void SaveProjectAsync_RejectsTheSameTaskListedTwice()
        {
            var input = new TimeProjectInputDto
            {
                Id = ShopId,
                CustomerId = 1,
                Name = "Shop",
                Tasks = new List<TimeTaskInputDto>
                {
                    new TimeTaskInputDto { Id = DevelopmentId, Name = "Development", IsDefault = true },
                    new TimeTaskInputDto { Id = DevelopmentId, Name = "Coding" }
                },
                MemberIds = new List<string> { Ona }
            };

            var error = Assert.ThrowsAsync<ValidationException>(() => _service.SaveProjectAsync(input, AsBoss));

            Assert.That(error.ErrorMessage, Is.EqualTo("The same task is listed twice"));
        }

        [Test]
        public void SaveProjectAsync_RejectsATooLongTaskComment()
        {
            var input = new TimeProjectInputDto
            {
                CustomerId = 1,
                Name = "Portal",
                Tasks = new List<TimeTaskInputDto> { new TimeTaskInputDto { Name = "Build", Comment = new string('x', 501), IsDefault = true } },
                MemberIds = new List<string>()
            };

            var error = Assert.ThrowsAsync<ValidationException>(() => _service.SaveProjectAsync(input, AsBoss));

            Assert.That(error.ErrorCode, Is.EqualTo(ErrorCodes.TimeRegistrationInvalid));
        }

        [Test]
        public void DeleteProjectAsync_RefusesAProjectWithHours()
        {
            AddEntry(Ona, ShopId, DevelopmentId, new System.DateTime(2026, 10, 1), 60);

            var error = Assert.ThrowsAsync<ValidationException>(() => _service.DeleteProjectAsync(ShopId, AsBoss));

            Assert.That(error.ErrorCode, Is.EqualTo(ErrorCodes.TimeRegistrationInUse));
        }

        [Test]
        public async Task DeleteProjectAsync_SoftDeletesTheProjectAndDropsItsMembers()
        {
            await _service.DeleteProjectAsync(ShopId, AsBoss);

            Assert.That(await Db.Set<TimeProject>().AnyAsync(project => project.Id == ShopId), Is.False);
            Assert.That(await Db.Set<TimeProjectMember>().AnyAsync(member => member.ProjectId == ShopId), Is.False);
        }

        [Test]
        public void DeleteCustomerAsync_RefusesACustomerWithProjects()
        {
            var error = Assert.ThrowsAsync<ValidationException>(() => _service.DeleteCustomerAsync(1, AsBoss));

            Assert.That(error.ErrorCode, Is.EqualTo(ErrorCodes.TimeRegistrationInUse));
        }
    }
}
