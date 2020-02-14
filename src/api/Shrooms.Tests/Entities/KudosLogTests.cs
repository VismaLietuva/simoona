using System;
using NUnit.Framework;
using Shrooms.Contracts.Exceptions;
using Shrooms.DataLayer.EntityModels.Models.Kudos;

namespace Shrooms.UnitTests.Entities
{
    [TestFixture]
    public class KudosLogTests
    {
        private readonly string _userId = Guid.NewGuid().ToString();

        [Test]
        public void Approve_UserIsEmpty()
        {
            var log = new KudosLog();

            Assert.Throws<ValidationException>(() => log.Approve(string.Empty));
        }

        [Test]
        public void Approve_SenderCannotApproveKudos()
        {
            var log = new KudosLog
            {
                CreatedBy = Guid.NewGuid().ToString()
            };

            Assert.Throws<ValidationException>(() => log.Approve(_userId));
        }

        [Test]
        public void Approve_SenderCannotSendKudosToSelf()
        {
            var log = new KudosLog
            {
                EmployeeId = _userId
            };

            Assert.Throws<ValidationException>(() => log.Approve(_userId));
        }

        [Test]
        public void Approve_LogAlreadyProcessed()
        {
            var log = new KudosLog
            {
                Status = KudosStatus.Rejected
            };

            Assert.Throws<ValidationException>(() => log.Approve(_userId));
        }

        [Test]
        public void Approve_ApprovesAndSetsModifiedAttributes()
        {
            var log = new KudosLog
            {
                Status = KudosStatus.Pending,
                EmployeeId = Guid.NewGuid().ToString(),
                CreatedBy = Guid.NewGuid().ToString()
            };

            log.Approve(_userId);

            Assert.That(log.Status, Is.EqualTo(KudosStatus.Approved));
            Assert.That(log.ModifiedBy, Is.EqualTo(_userId));
            Assert.That(log.Modified, Is.InRange(DateTime.UtcNow.AddSeconds(-10), DateTime.UtcNow.AddSeconds(10)));
        }

        [Test]
        public void IsRecipientDeleted_WhenNoEmployeeIsPulledFromDb()
        {
            var log = new KudosLog
            {
                EmployeeId = Guid.NewGuid().ToString()
            };

            Assert.That(log.IsRecipientDeleted(), Is.True);
        }

        [Test]
        public void Reject_UserIsEmpty()
        {
            var log = new KudosLog();

            Assert.Throws<ValidationException>(() => log.Reject(string.Empty, "not_empty"));
        }

        [Test]
        public void Reject_ReasonMessageIsEmpty()
        {
            var log = new KudosLog();

            Assert.Throws<ValidationException>(() => log.Reject(_userId, string.Empty));
        }

        [Test]
        public void Reject_LogAlreadyProcessed()
        {
            var log = new KudosLog
            {
                Status = KudosStatus.Approved
            };

            Assert.Throws<ValidationException>(() => log.Reject(_userId, "reason"));
        }

        [Test]
        public void Reject_RejectsAndSetsModifiedAttributes()
        {
            var log = new KudosLog
            {
                Status = KudosStatus.Pending,
                EmployeeId = Guid.NewGuid().ToString(),
                CreatedBy = Guid.NewGuid().ToString()
            };

            log.Reject(_userId, "reason");

            Assert.That(log.Status, Is.EqualTo(KudosStatus.Rejected));
            Assert.That(log.RejectionMessage, Is.EqualTo("reason"));
            Assert.That(log.ModifiedBy, Is.EqualTo(_userId));
            Assert.That(log.Modified, Is.InRange(DateTime.UtcNow.AddSeconds(-10), DateTime.UtcNow.AddSeconds(10)));
        }
    }
}
