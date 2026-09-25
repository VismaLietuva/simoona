using System.Reflection;
using NUnit.Framework;
using Shrooms.Presentation.Api.Controllers;
using Shrooms.Presentation.Common.Filters;

namespace Shrooms.Tests.Controllers.WebApi
{
    [TestFixture]
    public class UserDeprecatedControllerTests
    {
        [Test]
        public void PutPersonalInfo_Should_Invalidate_The_Wall_Widgets_Cache()
        {
            var action = typeof(UserDeprecatedController).GetMethod(nameof(UserDeprecatedController.PutPersonalInfo));

            Assert.That(action.GetCustomAttribute<InvalidatesWidgetCacheAttribute>(), Is.Not.Null);
        }
    }
}
