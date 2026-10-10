using System;
using NUnit.Framework;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.DataLayer.EntityModels.Models.Kudos;

namespace Shrooms.Tests.DomainService
{
    internal class ApplicationUserTests
    {
        [Test]
        public void Should_Return_That_An_Employee_Works_For_2_Years()
        {
            var employee = new ApplicationUser
            {
                EmploymentDate = DateTime.UtcNow.AddYears(-2).AddDays(-1)
            };

            Assert.That(employee.YearsEmployed, Is.EqualTo(2));
        }

        [Test]
        public void Should_Return_That_An_Employee_Works_For_0_Years()
        {
            var employee = new ApplicationUser
            {
                EmploymentDate = DateTime.UtcNow.AddDays(-360)
            };

            Assert.That(employee.YearsEmployed, Is.EqualTo(0));
        }

        [Test]
        public void Should_Return_That_An_Employee_Works_For_0_Years_2()
        {
            var employee = new ApplicationUser
            {
                EmploymentDate = DateTime.UtcNow.AddMonths(-1)
            };

            Assert.That(employee.YearsEmployed, Is.EqualTo(0));
        }

        [Test]
        public void Should_Keep_Kudos_Whole_When_Receiving_Fractional_Points()
        {
            var employee = new ApplicationUser
            {
                TotalKudos = 10,
                RemainingKudos = 4
            };

            employee.ReceiveKudos(new KudosLog { Points = 2.5m });

            Assert.Multiple(() =>
            {
                Assert.That(employee.TotalKudos, Is.EqualTo(13));
                Assert.That(employee.RemainingKudos, Is.EqualTo(7));
            });
        }
    }
}
