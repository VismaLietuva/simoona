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
        // Both shapes an image-less event round-trips as: NULL from the database, and the empty
        // string the create form defaults to. [Required] rejected each one, so the event could
        // not be saved at all.
        [TestCase(null)]
        [TestCase("")]
        public void Should_Not_Require_ImageName_When_Updating_An_Event(string imageName)
        {
            var model = ValidUpdate();
            model.ImageName = imageName;

            var results = new List<ValidationResult>();
            Validator.TryValidateObject(model, new ValidationContext(model), results, validateAllProperties: true);

            Assert.That(
                results.SelectMany(result => result.MemberNames),
                Has.No.Member(nameof(UpdateEventViewModel.ImageName)),
                "an event with no cover image must stay editable");
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
