using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NUnit.Framework;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.DataTransferObjects.Models.Support;
using Shrooms.Domain.Services.Support;
using Shrooms.Presentation.Api.Controllers;
using Shrooms.Presentation.WebViewModels.Models.Support;
using Shrooms.Tests.Extensions;
using Shrooms.Tests.ModelMappings;

namespace Shrooms.Tests.Controllers.WebApi
{
    [TestFixture]
    public class SupportControllerTests
    {
        private ISupportService _supportService;

        private SupportController _supportController;

        [SetUp]
        public void TestInitializer()
        {
            _supportService = Substitute.For<ISupportService>();

            _supportController = new SupportController(ModelMapper.Create(), _supportService);
            _supportController.SetUpControllerForTesting();
        }

        [Test]
        public async Task SubmitTicket_WithoutImages_Succeeds()
        {
            var result = await _supportController.SubmitTicket(Ticket());

            Assert.That(result.GetStatusCode(), Is.EqualTo(HttpStatusCode.Created));
            Assert.That(await CapturedTicketAsync(), Has.Property("Attachments").Empty);
        }

        [Test]
        public async Task SubmitTicket_WithSeveralImages_PassesThemAllOn()
        {
            var ticket = Ticket(Image("one.png"), Image("two.png"), Image("three.png"));

            var result = await _supportController.SubmitTicket(ticket);

            Assert.That(result.GetStatusCode(), Is.EqualTo(HttpStatusCode.Created));
            var submitted = await CapturedTicketAsync();
            Assert.That(submitted.Attachments.Select(attachment => attachment.FileName), Is.EqualTo(new[] { "one.png", "two.png", "three.png" }));
        }

        // Identical names would be indistinguishable once they are all on one email.
        [Test]
        public async Task SubmitTicket_WithRepeatedFileNames_MakesThemUnique()
        {
            var ticket = Ticket(Image("screenshot.jpg"), Image("screenshot.jpg"), Image("screenshot.jpg"));

            await _supportController.SubmitTicket(ticket);

            var submitted = await CapturedTicketAsync();
            Assert.That(
                submitted.Attachments.Select(attachment => attachment.FileName),
                Is.EqualTo(new[] { "screenshot.jpg", "screenshot-2.jpg", "screenshot-3.jpg" }));
        }

        [Test]
        public async Task SubmitTicket_WithMoreImagesThanAllowed_ReturnsBadRequest()
        {
            var images = Enumerable
                .Range(0, WebApiConstants.MaximumSupportImageCount + 1)
                .Select(index => Image($"shot-{index}.png"))
                .ToArray();

            var result = await _supportController.SubmitTicket(Ticket(images));

            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
            await _supportService.DidNotReceive().SubmitTicketAsync(Arg.Any<UserAndOrganizationDto>(), Arg.Any<SupportDto>());
        }

        [Test]
        public async Task SubmitTicket_WithOversizedImage_ReturnsBadRequest()
        {
            var ticket = Ticket(Image("huge.png", length: WebApiConstants.MaximumPictureSizeInBytes));

            var result = await _supportController.SubmitTicket(ticket);

            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        // Each image fits on its own; together they do not.
        [Test]
        public async Task SubmitTicket_WhenImagesTogetherExceedTheTotal_ReturnsBadRequest()
        {
            var half = WebApiConstants.MaximumSupportImagesTotalSizeInBytes / 2;
            var ticket = Ticket(Image("one.png", length: half), Image("two.png", length: half));

            var result = await _supportController.SubmitTicket(ticket);

            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
            await _supportService.DidNotReceive().SubmitTicketAsync(Arg.Any<UserAndOrganizationDto>(), Arg.Any<SupportDto>());
        }

        [Test]
        public async Task SubmitTicket_WithUnsupportedImageType_ReturnsUnsupportedMediaType()
        {
            var ticket = Ticket(Image("notes.pdf", contentType: "application/pdf"));

            var result = await _supportController.SubmitTicket(ticket);

            Assert.That(result.GetStatusCode(), Is.EqualTo(HttpStatusCode.UnsupportedMediaType));
        }

        // One bad image rejects the whole ticket rather than being dropped silently.
        [Test]
        public async Task SubmitTicket_WhenOneImageIsUnsupported_SubmitsNothing()
        {
            var ticket = Ticket(Image("fine.png"), Image("notes.pdf", contentType: "application/pdf"));

            await _supportController.SubmitTicket(ticket);

            await _supportService.DidNotReceive().SubmitTicketAsync(Arg.Any<UserAndOrganizationDto>(), Arg.Any<SupportDto>());
        }

        [Test]
        public async Task SubmitTicket_WithEmptyImage_IgnoresIt()
        {
            var ticket = Ticket(Image("empty.png", length: 0), Image("real.png"));

            await _supportController.SubmitTicket(ticket);

            var submitted = await CapturedTicketAsync();
            Assert.That(submitted.Attachments.Select(attachment => attachment.FileName), Is.EqualTo(new[] { "real.png" }));
        }

        private async Task<SupportDto> CapturedTicketAsync()
        {
            var calls = _supportService.ReceivedCalls().ToList();
            Assert.That(calls, Is.Not.Empty, "the ticket was never submitted");
            await Task.CompletedTask;
            return (SupportDto)calls.Last().GetArguments()[1];
        }

        private static SupportPostViewModel Ticket(params IFormFile[] images) =>
            new SupportPostViewModel
            {
                Subject = "Broken flow",
                Message = "Steps attached",
                Type = (int)SupportType.Bug,
                Images = images.ToList()
            };

        private static IFormFile Image(string fileName, string contentType = "image/png", long? length = null)
        {
            var content = Encoding.UTF8.GetBytes("image-bytes");
            var file = Substitute.For<IFormFile>();

            file.FileName.Returns(fileName);
            file.ContentType.Returns(contentType);
            file.Length.Returns(length ?? content.Length);
            file.CopyToAsync(Arg.Any<Stream>()).Returns(callInfo => callInfo.Arg<Stream>().WriteAsync(content, 0, content.Length));

            return file;
        }
    }
}
