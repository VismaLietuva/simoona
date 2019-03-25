using NUnit.Framework;
using Shrooms.ApiTest.BaseSetup;
using Shrooms.WebViewModels.Models;
using Shrooms.WebViewModels.Models.Events;
using Shrooms.WebViewModels.Models.Kudos;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;

namespace Shrooms.ApiTests.Tests
{
    public class UnauthorizedSampleTest : BaseAuthenticatedApiTestFixture
    {
        private string _uri;

        protected override string Uri => _uri;

        [Test]
        public async void Event()
        {
            // Arrange
            _uri = "event/types";

            // Act
            var response = await GetAsync();
            var result = await response.Content.ReadAsAsync<IEnumerable<EventTypeViewModel>>();

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [Test]
        public async void Kudos()
        {
            // Arrange
            _uri = "kudos/GetUserKudosInformationById?id=2d2a0c79-987f-42ea-821b-402dd345f3e6";

            // Act
            var response = await GetAsync();
            var result = await response.Content.ReadAsAsync<PagedViewModel<KudosUserLogViewModel>>();

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }
    }
}
