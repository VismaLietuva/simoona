using System.Linq;
using NUnit.Framework;
using Shrooms.Premium.Domain.Services.Vacations;

namespace Shrooms.Premium.Tests.DomainService.VacationService
{
    [TestFixture]
    public class VacationNotificationRecipientsTests
    {
        private const string Employee = "employee";
        private const string Manager = "manager";
        private const string Admin = "admin";

        [Test]
        public void Submitted_TellsTheManagerOnly()
        {
            var recipients = VacationNotificationRecipients.ForSubmitted(Manager, Employee);

            Assert.That(recipients.Single().UserId, Is.EqualTo(Manager));
            Assert.That(recipients.Single().Notice, Is.EqualTo(VacationNotice.Submitted));
        }

        [Test]
        public void Changed_ByTheEmployee_TellsTheManager()
        {
            var recipients = VacationNotificationRecipients.ForChanged(Employee, Manager, Employee);

            Assert.That(recipients.Single().UserId, Is.EqualTo(Manager));
            Assert.That(recipients.Single().Notice, Is.EqualTo(VacationNotice.Changed));
        }

        /// <summary>An administrator correcting a record is news to the employee, not to the manager.</summary>
        [Test]
        public void Changed_ByAnAdministrator_TellsTheEmployee()
        {
            var recipients = VacationNotificationRecipients.ForChanged(Employee, Manager, Admin);

            Assert.That(recipients.Single().UserId, Is.EqualTo(Employee));
        }

        [Test]
        public void Withdrawn_TellsTheManager()
        {
            var recipients = VacationNotificationRecipients.ForWithdrawn(Manager, Employee);

            Assert.That(recipients.Single().UserId, Is.EqualTo(Manager));
            Assert.That(recipients.Single().Notice, Is.EqualTo(VacationNotice.Withdrawn));
        }

        [Test]
        public void Decided_ByTheManager_TellsTheEmployeeOnly()
        {
            var recipients = VacationNotificationRecipients.ForDecided(Employee, Manager, Manager);

            Assert.That(recipients.Single().UserId, Is.EqualTo(Employee));
            Assert.That(recipients.Single().Notice, Is.EqualTo(VacationNotice.Decided));
        }

        /// <summary>A decision taken over the manager's head is exactly what they would not see.</summary>
        [Test]
        public void Decided_ByAnAdministrator_TellsBoth()
        {
            var recipients = VacationNotificationRecipients.ForDecided(Employee, Manager, Admin);

            Assert.That(recipients.Select(r => r.UserId), Is.EquivalentTo(new[] { Employee, Manager }));
            Assert.That(
                recipients.Single(r => r.UserId == Manager).Notice,
                Is.EqualTo(VacationNotice.DecidedByAdmin));
        }

        [Test]
        public void NobodyIsToldAboutTheirOwnDoing()
        {
            Assert.That(VacationNotificationRecipients.ForSubmitted(Manager, Manager), Is.Empty);
            Assert.That(VacationNotificationRecipients.ForWithdrawn(Manager, Manager), Is.Empty);

            // Somebody who manages themselves, editing their own request.
            Assert.That(VacationNotificationRecipients.ForChanged(Employee, Employee, Employee), Is.Empty);
        }

        [Test]
        public void AnEmployeeWithNoManagerLeavesNobodyToTell()
        {
            Assert.That(VacationNotificationRecipients.ForSubmitted(null, Employee), Is.Empty);
            Assert.That(VacationNotificationRecipients.ForChanged(Employee, string.Empty, Employee), Is.Empty);
            Assert.That(VacationNotificationRecipients.ForWithdrawn(null, Employee), Is.Empty);

            var decided = VacationNotificationRecipients.ForDecided(Employee, null, Admin);
            Assert.That(decided.Single().UserId, Is.EqualTo(Employee));
        }

        /// <summary>Somebody who manages themselves gets one notification, not two.</summary>
        [Test]
        public void AnEmployeeWhoIsTheirOwnManagerIsToldOnce()
        {
            var recipients = VacationNotificationRecipients.ForDecided(Employee, Employee, Admin);

            Assert.That(recipients.Single().UserId, Is.EqualTo(Employee));
            Assert.That(recipients.Single().Notice, Is.EqualTo(VacationNotice.Decided));
        }
    }
}
