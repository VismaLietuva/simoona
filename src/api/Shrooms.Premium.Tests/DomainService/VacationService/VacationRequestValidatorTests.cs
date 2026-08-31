using System;
using System.Collections.Generic;
using NUnit.Framework;
using Shrooms.Contracts.Enums;
using Shrooms.DataLayer.EntityModels.Models.Vacations;
using Shrooms.Premium.Domain.DomainExceptions.Vacation;
using Shrooms.Premium.Domain.Services.Vacations;

namespace Shrooms.Premium.Tests.DomainService.VacationService
{
    [TestFixture]
    public class VacationRequestValidatorTests
    {
        // A Monday, so the ranges below are made of plain working days.
        private static readonly DateTime Today = new DateTime(2026, 8, 17);

        [Test]
        public void CanEdit_AllowsAnApprovalThatHasAlreadyStarted()
        {
            var started = Request(VacationRequestStatus.Approved, Today.AddDays(-3), Today.AddDays(4));

            Assert.That(VacationRequestValidator.CanEdit(started, Today), Is.True);
        }

        [Test]
        public void CanEdit_AllowsPendingRejectedAndFutureApprovals()
        {
            foreach (var status in new[]
                     {
                         VacationRequestStatus.Pending,
                         VacationRequestStatus.Rejected,
                         VacationRequestStatus.Approved
                     })
            {
                Assert.That(
                    VacationRequestValidator.CanEdit(Request(status, Today.AddDays(7), Today.AddDays(11)), Today),
                    Is.True,
                    status.ToString());
            }
        }

        [Test]
        public void CanEdit_RefusesAWithdrawnRequest()
        {
            var cancelled = Request(VacationRequestStatus.Cancelled, Today.AddDays(7), Today.AddDays(11));

            Assert.That(VacationRequestValidator.CanEdit(cancelled, Today), Is.False);
        }

        [Test]
        public void ValidateDraft_RefusesAStartMovedIntoThePast()
        {
            var error = Assert.Throws<VacationValidationException>(() =>
                VacationRequestValidator.ValidateDraft(
                    VacationRequestType.Annual,
                    Today.AddDays(-5),
                    Today.AddDays(2),
                    null,
                    Today,
                    NoOtherRequests,
                    originalDateFrom: Today.AddDays(-3)));

            Assert.That(error.Code, Is.EqualTo("startInPast"));
        }

        [Test]
        public void ValidateDraft_LetsLeaveUnderWayKeepItsStartSoItCanBeCutShort()
        {
            // The start is in the past but unchanged; only the end moves in.
            Assert.DoesNotThrow(() =>
                VacationRequestValidator.ValidateDraft(
                    VacationRequestType.Annual,
                    Today.AddDays(-3),
                    Today.AddDays(1),
                    null,
                    Today,
                    NoOtherRequests,
                    originalDateFrom: Today.AddDays(-3)));
        }

        [Test]
        public void ValidateDraft_StillRefusesAPastStartOnANewRequest()
        {
            var error = Assert.Throws<VacationValidationException>(() =>
                VacationRequestValidator.ValidateDraft(
                    VacationRequestType.Annual,
                    Today.AddDays(-5),
                    Today.AddDays(2),
                    null,
                    Today,
                    NoOtherRequests));

            Assert.That(error.Code, Is.EqualTo("startInPast"));
        }

        private static IEnumerable<VacationRequest> NoOtherRequests => new List<VacationRequest>();

        private static VacationRequest Request(VacationRequestStatus status, DateTime from, DateTime to)
        {
            return new VacationRequest
            {
                Id = 1,
                Type = VacationRequestType.Annual,
                Status = status,
                DateFrom = from,
                DateTo = to
            };
        }
    }
}
