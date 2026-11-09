using NUnit.Framework;
using Shrooms.Premium.DataTransferObjects.Models.Events;
using Shrooms.Premium.Presentation.WebViewModels.Events;
using Shrooms.Premium.Tests.ModelMappings;

namespace Shrooms.Premium.Tests.Controllers.ViewModels
{
    public class EventWaitlistMappingTests
    {
        [Test]
        public void Should_Map_HasWaitlist_From_Create_ViewModel_To_Dto()
        {
            var mapper = ModelMapper.Create();

            var dto = mapper.Map<CreateEventDto>(new CreateEventViewModel { HasWaitlist = true });

            Assert.That(dto.HasWaitlist, Is.True);
        }

        [Test]
        public void Should_Map_Waitlist_Fields_From_Details_Dto_To_ViewModel()
        {
            var mapper = ModelMapper.Create();

            var model = mapper.Map<EventDetailsViewModel>(new EventDetailsDto
            {
                HasWaitlist = true,
                WaitlistCount = 3,
                WaitlistPosition = 2
            });

            Assert.That(model.HasWaitlist, Is.True);
            Assert.That(model.WaitlistCount, Is.EqualTo(3));
            Assert.That(model.WaitlistPosition, Is.EqualTo(2));
        }
    }
}
