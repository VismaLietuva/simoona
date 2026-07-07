using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using NSubstitute;
using NUnit.Framework;
using Shrooms.Presentation.Api.Helpers;

namespace Shrooms.Tests.Helpers
{
    [TestFixture]
    public class CustomUrlHelperTest
    {
        [Test]
        public void RouteFromController_when_prefix_api()
        {
            var helper = Substitute.For<IUrlHelper>();

            helper.RouteUrl(Arg.Any<UrlRouteContext>()).Returns("/api/Account/Action/");

            var result = helper.RouteFromController("ExternalLogin", "Account", null);

            Assert.That(result, Is.EqualTo("/Account/Action/"));
        }

        [Test]
        public void RouteFromController_when_no_prefix()
        {
            var helper = Substitute.For<IUrlHelper>();

            helper.RouteUrl(Arg.Any<UrlRouteContext>()).Returns("/Account/Action/");

            var result = helper.RouteFromController("ExternalLogin", "Account", null);

            Assert.That(result, Is.EqualTo("/Account/Action/"));
        }

        [Test]
        public void RouteFromController_when_controller_name_empty()
        {
            var helper = Substitute.For<IUrlHelper>();

            helper.RouteUrl(Arg.Any<UrlRouteContext>()).Returns("/api/Account/Action/");

            var result = helper.RouteFromController("ExternalLogin", "", null);

            Assert.That(result, Is.EqualTo("/api/Account/Action/"));
        }

        [Test]
        public void RouteFromController_when_controller_name_null()
        {
            var helper = Substitute.For<IUrlHelper>();

            helper.RouteUrl(Arg.Any<UrlRouteContext>()).Returns("/api/Account/Action/");

            var result = helper.RouteFromController("ExternalLogin", null, null);

            Assert.That(result, Is.EqualTo("/api/Account/Action/"));
        }

        [Test]
        public void RouteFromController_when_prefix_is_short()
        {
            var helper = Substitute.For<IUrlHelper>();

            helper.RouteUrl(Arg.Any<UrlRouteContext>()).Returns("/a/Account/Action/");

            var result = helper.RouteFromController("ExternalLogin", "Account", null);

            Assert.That(result, Is.EqualTo("/Account/Action/"));
        }
    }
}
