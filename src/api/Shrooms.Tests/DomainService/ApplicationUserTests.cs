using System;
using NUnit.Framework;
using Shrooms.DataLayer.EntityModels.Models;

namespace Shrooms.UnitTests.DomainService
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

            Assert.AreEqual(2, employee.YearsEmployed);
        }

        [Test]
        public void Should_Return_That_An_Employee_Works_For_0_Years()
        {
            var employee = new ApplicationUser
            {
                EmploymentDate = DateTime.UtcNow.AddDays(-360)
            };

            Assert.AreEqual(0, employee.YearsEmployed);
        }

        [Test]
        public void Should_Return_That_An_Employee_Works_For_0_Years_2()
        {
            var employee = new ApplicationUser
            {
                EmploymentDate = DateTime.UtcNow.AddMonths(-1)
            };

            Assert.AreEqual(0, employee.YearsEmployed);
        }
    }
}
