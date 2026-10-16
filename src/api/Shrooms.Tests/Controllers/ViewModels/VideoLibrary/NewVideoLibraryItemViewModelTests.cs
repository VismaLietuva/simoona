using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NUnit.Framework;
using Shrooms.Presentation.WebViewModels.Models.VideoLibrary;

namespace Shrooms.Tests.Controllers.ViewModels.VideoLibrary
{
    [TestFixture]
    public class NewVideoLibraryItemViewModelTests
    {
        private static readonly JsonSerializerSettings ApiSettings = new()
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        [Test]
        public void Should_Read_A_Recording_Date_Sent_As_A_Calendar_Day()
        {
            var viewModel = Deserialize("\"2024-03-12\"");

            Assert.That(viewModel.RecordedOn, Is.EqualTo(new DateOnly(2024, 3, 12)));
        }

        [Test]
        public void Should_Leave_The_Recording_Date_Empty_When_It_Is_Not_Sent()
        {
            Assert.That(Deserialize("null").RecordedOn, Is.Null);
        }

        [TestCase("\"2024-03-11T21:00:00.000Z\"")]
        [TestCase("\"2024-03-12T00:00:00\"")]
        public void Should_Reject_A_Recording_Date_With_A_Time_Rather_Than_Shift_Its_Day(string recordedOn)
        {
            Assert.Throws<JsonSerializationException>(() => Deserialize(recordedOn));
        }

        [Test]
        public void Should_Write_The_Recording_Date_As_A_Calendar_Day()
        {
            var json = JsonConvert.SerializeObject(
                new VideoLibraryItemViewModel { RecordedOn = new DateOnly(2024, 3, 12) },
                ApiSettings);

            Assert.That(json, Does.Contain("\"recordedOn\":\"2024-03-12\""));
        }

        private static NewVideoLibraryItemViewModel Deserialize(string recordedOn)
        {
            return JsonConvert.DeserializeObject<NewVideoLibraryItemViewModel>(
                "{\"title\":\"t\",\"url\":\"https://drive.google.com/file/d/x/view\",\"recordedOn\":" + recordedOn + "}",
                ApiSettings);
        }
    }
}
