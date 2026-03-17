using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;
using Shrooms.Contracts.Exceptions;
using Shrooms.Contracts.Infrastructure;
using Shrooms.Premium.Constants;
using Shrooms.Premium.Infrastructure.VacationBot;

namespace Shrooms.Premium.Tests.DomainService.VacationService
{
    [TestFixture]
    public class VacationBotServiceTests
    {
        private IApplicationSettings _appSettings;
        private ILogger<VacationBotService> _logger;
        private FakeHttpMessageHandler _handler;
        private HttpClient _httpClient;
        private VacationBotService _vacationBotService;

        private const string TestEmail = "user@example.com";
        private const string TestAuthToken = "dGVzdDp0ZXN0";
        private const string TestHistoryUrlTemplate = "http://vacationbot.test/{0}/history";

        [SetUp]
        public void SetUp()
        {
            _appSettings = Substitute.For<IApplicationSettings>();
            _logger = Substitute.For<ILogger<VacationBotService>>();

            _appSettings.VacationsBotAuthToken.Returns(TestAuthToken);
            _appSettings.VacationsBotHistoryUrl.Returns(TestHistoryUrlTemplate);

            _handler = new FakeHttpMessageHandler();
            _httpClient = new HttpClient(_handler);
            _vacationBotService = new VacationBotService(_httpClient, _appSettings, _logger);
        }

        [TearDown]
        public void TearDown()
        {
            _httpClient.Dispose();
        }

        [Test]
        public async Task GetVacationHistory_WhenResponseIsSuccessful_ReturnsDeserializedVacationInfo()
        {
            var expected = new[]
            {
                new VacationInfo { DateFrom = new DateTime(2024, 1, 1), DateTo = new DateTime(2024, 1, 10) },
                new VacationInfo { DateFrom = new DateTime(2024, 6, 1), DateTo = new DateTime(2024, 6, 5) }
            };

            _handler.SetResponse(HttpStatusCode.OK, JsonSerializer.Serialize(expected));

            var result = await _vacationBotService.GetVacationHistory(TestEmail);

            Assert.That(result, Has.Length.EqualTo(2));
            Assert.That(result[0].DateFrom, Is.EqualTo(expected[0].DateFrom));
            Assert.That(result[0].DateTo, Is.EqualTo(expected[0].DateTo));
            Assert.That(result[1].DateFrom, Is.EqualTo(expected[1].DateFrom));
            Assert.That(result[1].DateTo, Is.EqualTo(expected[1].DateTo));
        }

        [Test]
        public async Task GetVacationHistory_WhenResponseIsSuccessfulWithEmptyArray_ReturnsEmptyArray()
        {
            _handler.SetResponse(HttpStatusCode.OK, "[]");

            var result = await _vacationBotService.GetVacationHistory(TestEmail);

            Assert.That(result, Is.Empty);
        }

        [Test]
        public void GetVacationHistory_WhenResponseIsNotSuccess_ThrowsValidationException()
        {
            _handler.SetResponse(HttpStatusCode.InternalServerError, "error");

            var ex = Assert.ThrowsAsync<ValidationException>(
                () => _vacationBotService.GetVacationHistory(TestEmail));

            Assert.That(ex.ErrorCode, Is.EqualTo(PremiumErrorCodes.VacationBotError));
        }

        [Test]
        public async Task GetVacationHistory_WhenResponseIsNotSuccess_LogsError()
        {
            _handler.SetResponse(HttpStatusCode.BadRequest, "bad request");

            try { await _vacationBotService.GetVacationHistory(TestEmail); }
            catch (ValidationException) { }

            _logger.Received(1).Log(
                LogLevel.Error,
                Arg.Any<EventId>(),
                Arg.Any<object>(),
                Arg.Any<Exception>(),
                Arg.Any<Func<object, Exception, string>>());
        }

        [Test]
        public void GetVacationHistory_WhenHttpRequestExceptionIsThrown_ThrowsValidationException()
        {
            _handler.SetException(new HttpRequestException("Network failure"));

            var ex = Assert.ThrowsAsync<ValidationException>(
                () => _vacationBotService.GetVacationHistory(TestEmail));

            Assert.That(ex.ErrorCode, Is.EqualTo(PremiumErrorCodes.VacationBotError));
        }

        [Test]
        public async Task GetVacationHistory_WhenHttpRequestExceptionIsThrown_LogsError()
        {
            var networkException = new HttpRequestException("Network failure");
            _handler.SetException(networkException);

            try { await _vacationBotService.GetVacationHistory(TestEmail); }
            catch (ValidationException) { }

            _logger.Received(1).Log(
                LogLevel.Error,
                Arg.Any<EventId>(),
                Arg.Any<object>(),
                networkException,
                Arg.Any<Func<object, Exception, string>>());
        }

        [Test]
        public async Task GetVacationHistory_SetsCorrectBasicAuthorizationHeader()
        {
            _handler.SetResponse(HttpStatusCode.OK, "[]");

            await _vacationBotService.GetVacationHistory(TestEmail);

            Assert.That(_handler.LastRequest.Headers.Authorization, Is.Not.Null);
            Assert.That(_handler.LastRequest.Headers.Authorization.Scheme, Is.EqualTo("Basic"));
            Assert.That(_handler.LastRequest.Headers.Authorization.Parameter, Is.EqualTo(TestAuthToken));
        }

        [Test]
        public async Task GetVacationHistory_SendsPostRequestToUrlFormattedWithEmail()
        {
            _handler.SetResponse(HttpStatusCode.OK, "[]");

            await _vacationBotService.GetVacationHistory(TestEmail);

            var expectedUrl = string.Format(TestHistoryUrlTemplate, TestEmail);
            Assert.That(_handler.LastRequest.Method, Is.EqualTo(HttpMethod.Post));
            Assert.That(_handler.LastRequest.RequestUri.ToString(), Is.EqualTo(expectedUrl));
        }

        private sealed class FakeHttpMessageHandler : HttpMessageHandler
        {
            private HttpResponseMessage _response;
            private Exception _exception;

            public HttpRequestMessage LastRequest { get; private set; }

            public void SetResponse(HttpStatusCode statusCode, string body)
            {
                _exception = null;
                _response?.Dispose();
                _response = new HttpResponseMessage(statusCode)
                {
                    Content = new StringContent(body, Encoding.UTF8, "application/json")
                };
            }

            public void SetException(Exception exception)
            {
                _response?.Dispose();
                _response = null;
                _exception = exception;
            }

            protected override Task<HttpResponseMessage> SendAsync(
                HttpRequestMessage request, CancellationToken cancellationToken)
            {
                LastRequest = request;

                if (_exception != null)
                {
                    throw _exception;
                }

                return Task.FromResult(_response);
            }

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    _response?.Dispose();
                }

                base.Dispose(disposing);
            }
        }
    }
}
