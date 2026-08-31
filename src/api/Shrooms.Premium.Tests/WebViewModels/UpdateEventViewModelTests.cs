using NUnit.Framework;
using Shrooms.DataLayer.EntityModels.Models.Events;
using Shrooms.Premium.Presentation.WebViewModels.Events;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Shrooms.Premium.Tests.WebViewModels
{
    [TestFixture]
    public class UpdateEventViewModelTests
    {
        // ImageName was [Required] on update but optional on create, so an event created without
        // a cover image (ImageName NULL in the database) could not be updated through the API at
        // all — every PUT for it failed model validation before reaching the service. A host
        // editing such an event, e.g. to add sign-up questions, was blocked outright.
        [Test]
        public void Should_Not_Require_ImageName_When_Updating_An_Event()
        {
            var model = ValidUpdate();
            model.ImageName = null;

            var results = new List<ValidationResult>();
            Validator.TryValidateObject(model, new ValidationContext(model), results, validateAllProperties: true);

            Assert.That(
                results.SelectMany(result => result.MemberNames),
                Has.No.Member(nameof(UpdateEventViewModel.ImageName)),
                "an event with no cover image must stay editable");
        }

        [Test]
        public void Should_Accept_An_Update_That_Carries_A_Cover_Image()
        {
            var model = ValidUpdate();

            var results = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(model, new ValidationContext(model), results, validateAllProperties: true);

            Assert.That(isValid, Is.True, string.Join("; ", results.Select(result => result.ErrorMessage)));
        }

        private static UpdateEventViewModel ValidUpdate()
        {
            var startDate = DateTime.UtcNow.AddDays(7);

            return new UpdateEventViewModel
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Autumnfest",
                ImageName = "cover.jpg",
                StartDate = startDate,
                EndDate = startDate.AddHours(3),
                RegistrationDeadlineDate = startDate,
                Recurrence = EventRecurrenceOptions.None,
                Offices = new List<int> { 1 },
                Location = "Kaunas",
                ResponsibleUserId = "responsibleUser1"
            };
        }
    }
}
