using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;
using Shrooms.Contracts.Constants;
using Shrooms.Premium.Presentation.Api.Controllers;
using Shrooms.Premium.Presentation.Api.Controllers.Lotteries;
using Shrooms.Presentation.Api.Controllers;
using Shrooms.Presentation.Api.Filters;
using Shrooms.Presentation.Common.Controllers;
using Shrooms.Presentation.Common.Filters;

namespace Shrooms.Premium.Tests.Controllers.WebApi
{
    /// <summary>
    /// Guards the authorization surface of every API controller. The app registers a deny-by-default
    /// fallback policy, so these tests exist to keep the explicit attributes honest: every action must
    /// declare how it is protected, and admin-only actions must carry the matching permission.
    /// </summary>
    [TestFixture]
    public class ControllerAuthorizationTests
    {
        private static readonly Assembly[] ControllerAssemblies =
        {
            typeof(AccountController).Assembly,   // Shrooms.Presentation.Api
            typeof(BaseController).Assembly,      // Shrooms.Presentation.Common
            typeof(LotteryController).Assembly    // Shrooms.Premium
        };

        private static IEnumerable<Type> ControllerTypes => ControllerAssemblies
            .SelectMany(a => a.GetTypes())
            .Where(t => typeof(ControllerBase).IsAssignableFrom(t) && !t.IsAbstract && t.IsPublic)
            .OrderBy(t => t.FullName);

        // Public instance methods declared on the application's own controller classes (including abstract
        // bases such as AbstractWebApiController), excluding the framework's ControllerBase/Controller members.
        private static IEnumerable<MethodInfo> ActionsOf(Type controller) => controller
            .GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Where(m => typeof(ControllerBase).IsAssignableFrom(m.DeclaringType)
                        && m.DeclaringType != typeof(ControllerBase)
                        && m.DeclaringType != typeof(Controller))
            .Where(m => !m.IsSpecialName && m.GetCustomAttribute<NonActionAttribute>() == null);

        [Test]
        public void EveryControllerAction_DeclaresHowItIsProtected()
        {
            var unprotected = new List<string>();
            var controllers = ControllerTypes.ToList();

            Assert.That(controllers.Count, Is.GreaterThan(30), "controller scan found suspiciously few types");

            foreach (var controller in controllers)
            {
                var classLevel = HasAnyAuthorizationAttribute(controller);

                foreach (var action in ActionsOf(controller))
                {
                    if (!classLevel && !HasAnyAuthorizationAttribute(action))
                    {
                        unprotected.Add($"{controller.Name}.{action.Name}");
                    }
                }
            }

            Assert.That(unprotected, Is.Empty,
                "Actions without [Authorize], [AllowAnonymous], a permission attribute or a webhook filter (class or action level):\n"
                + string.Join("\n", unprotected));
        }

        [TestCase(nameof(LotteryController.CreateLottery))]
        [TestCase(nameof(LotteryController.Abort))]
        [TestCase(nameof(LotteryController.RefundParticipants))]
        [TestCase(nameof(LotteryController.UpdateDrafted))]
        [TestCase(nameof(LotteryController.UpdateStarted))]
        [TestCase(nameof(LotteryController.FinishLottery))]
        public void LotteryAdminActions_RequireLotteryAdministrationPermission(string action)
        {
            AssertRequiresPermission(typeof(LotteryController), action, AdministrationPermissions.Lottery);
        }

        [Test]
        public void EventParticipantReport_RequiresEventAdministrationPermission()
        {
            AssertRequiresPermission(typeof(EventController), nameof(EventController.GetPagedReportParticipants), AdministrationPermissions.Event);
        }

        [Test]
        public void WebhookControllers_OptOutOfFallbackPolicyAndUseBasicAuthentication()
        {
            foreach (var controller in new[] { typeof(ExternalJobsController), typeof(ExternalPremiumJobsController) })
            {
                Assert.That(controller.GetCustomAttribute<AllowAnonymousAttribute>(), Is.Not.Null, controller.Name);
                Assert.That(controller.GetCustomAttribute<IdentityBasicAuthenticationAttribute>(), Is.Not.Null, controller.Name);
            }
        }

        [Test]
        public void AbstractClassifierController_IsRemoved()
        {
            var leftover = ControllerTypes.FirstOrDefault(t => t.Name == "AbstractClassifierController");

            Assert.That(leftover, Is.Null, "AbstractClassifierController exposed unauthenticated generic CRUD and was deleted.");
        }

        private static void AssertRequiresPermission(Type controller, string actionName, string permission)
        {
            var action = controller.GetMethod(actionName, BindingFlags.Public | BindingFlags.Instance);
            Assert.That(action, Is.Not.Null, $"{controller.Name}.{actionName} not found");

            var attribute = action.GetCustomAttribute<PermissionAuthorizeAttribute>();
            Assert.That(attribute, Is.Not.Null, $"{controller.Name}.{actionName} has no [PermissionAuthorize]");
            Assert.That(attribute.Permission, Is.EqualTo(permission));
        }

        private static bool HasAnyAuthorizationAttribute(MemberInfo member)
        {
            return member.GetCustomAttributes(inherit: true).Any(a =>
                a is IAuthorizeData
                || a is IAllowAnonymous
                || a is PermissionAuthorizeAttribute
                || a is PermissionAnyOfAuthorizeAttribute
                || a is BasicAuthenticationAttribute
                || a is HmacAuthenticationAttribute);
        }
    }
}
