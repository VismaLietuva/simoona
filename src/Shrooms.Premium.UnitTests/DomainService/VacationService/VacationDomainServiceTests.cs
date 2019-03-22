using NUnit.Framework;
using Shrooms.EntityModels.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Shrooms.Domain.Services.Vacations;

namespace Shrooms.UnitTests.DomainService.VacationService
{
    [TestFixture]
    public class VacationDomainServiceTests
    {
        private IVacationDomainService _vacationDomainService;

        [SetUp]
        public void Init()
        {
            _vacationDomainService = new VacationDomainService();
        }

        [Test]
        public void FilterUsersByNamesQuery_ShouldBuildPredicate()
        {
            string fullName = "x y z";

            IList<ApplicationUser> users = new List<ApplicationUser> {
                new ApplicationUser { Id = "1", FirstName = "x", LastName = "x" },
                new ApplicationUser { Id = "2", FirstName = "y", LastName = "y" },
                new ApplicationUser { Id = "3", FirstName = "z", LastName = "z" },
                new ApplicationUser { Id = "4", FirstName = "x", LastName = "y" },
                new ApplicationUser { Id = "5", FirstName = "x", LastName = "z" },
                new ApplicationUser { Id = "6", FirstName = "y", LastName = "x" },
                new ApplicationUser { Id = "7", FirstName = "y", LastName = "z" },
                new ApplicationUser { Id = "8", FirstName = "z", LastName = "x" },
                new ApplicationUser { Id = "9", FirstName = "z", LastName = "y" },
                new ApplicationUser { Id = "10", FirstName = "Real", LastName = "Name" }
            };

            Expression<Func<ApplicationUser, bool>> filter = _vacationDomainService.UsersByNamesFilter(fullName);

            var result = users.Where(filter.Compile()).ToList();

            Assert.That(result.Count(), Is.EqualTo(9));
        }

        [Test]
        public void FindUser_ShouldReturnUser()
        {
            string fullName = "LastName von FirstName";

            IList<ApplicationUser> users = new List<ApplicationUser> {
                new ApplicationUser { Id = "1", FirstName = "John", LastName = "Armstrong" },
                new ApplicationUser { Id = "2", FirstName = "John", LastName = "Lennon" },
                new ApplicationUser { Id = "3", FirstName = "FirstName", LastName = "von LastName" },
                new ApplicationUser { Id = "4", FirstName = "FirstName von", LastName = "LastName" }
            };

            ApplicationUser user = _vacationDomainService.FindUser(users, fullName);

            Assert.That(user, Is.Not.Null);
            Assert.That(user.Id, Is.EqualTo("3"));
        }

        [Test]
        public void FindUser_ShouldReturnNull()
        {
            string fullName = "LastName von FirstName";

            IList<ApplicationUser> users = new List<ApplicationUser> {
                new ApplicationUser { Id = "1", FirstName = "John", LastName = "Armstrong" },
                new ApplicationUser { Id = "2", FirstName = "John", LastName = "Lennon" },
                new ApplicationUser { Id = "3", FirstName = "FirstName", LastName = "LastName" },
            };

            ApplicationUser user = _vacationDomainService.FindUser(users, fullName);

            Assert.That(user, Is.Null);
        }
    }
}
