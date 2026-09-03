using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using Shrooms.Infrastructure.Configuration;

namespace Shrooms.Tests.Infrastructure
{
    // Every URL here must resolve against a real Next.js route in the simoona-nextjs
    // repo. The client has no tenant segment - the organization comes from the auth
    // cookie - and its first segment is the locale.
    [TestFixture]
    public class ApplicationSettingsUrlTests
    {
        private const string Tenant = "visma";

        private ApplicationSettings _appSettings;

        [SetUp]
        public void SetUp()
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    ["ClientUrl"] = "https://simoona.example.com/",
                    ["ApiUrl"] = "https://api.simoona.example.com"
                })
                .Build();

            _appSettings = new ApplicationSettings(configuration);
        }

        [Test]
        public void Should_Not_Put_The_Tenant_In_Any_Client_Path()
        {
            var urls = new[]
            {
                _appSettings.WallPostUrl(Tenant, 5),
                _appSettings.UserNotificationSettingsUrl(Tenant),
                _appSettings.UserProfileUrl(Tenant, "user-1"),
                _appSettings.GroupUrl(Tenant, 7),
                _appSettings.BookUrl(Tenant, 3, 9),
                _appSettings.KudosProfileUrl(Tenant, "user-1"),
                _appSettings.EventUrl(Tenant, "event-1"),
                _appSettings.EventListByTypeUrl(Tenant, "4"),
                _appSettings.ProjectUrl(Tenant, "8"),
                _appSettings.CommitteeSugestionUrl(Tenant),
                _appSettings.ServiceRequestUrl(Tenant, 11),
                _appSettings.FeedUrl(Tenant)
            };

            foreach (var url in urls)
            {
                StringAssert.StartsWith("https://simoona.example.com/en/", url);
                ClassicAssert.IsFalse(url.Contains($"/{Tenant}/"), $"{url} still carries the tenant in its path");
            }
        }

        [Test]
        public void Should_Build_Wall_Post_Url()
        {
            ClassicAssert.AreEqual("https://simoona.example.com/en/posts/5", _appSettings.WallPostUrl(Tenant, 5));
        }

        [Test]
        public void Should_Build_User_Notification_Settings_Url()
        {
            ClassicAssert.AreEqual("https://simoona.example.com/en/settings/notifications", _appSettings.UserNotificationSettingsUrl(Tenant));
        }

        [Test]
        public void Should_Build_User_Profile_Url()
        {
            ClassicAssert.AreEqual("https://simoona.example.com/en/profile/user-1", _appSettings.UserProfileUrl(Tenant, "user-1"));
        }

        [Test]
        public void Should_Build_Group_Url()
        {
            ClassicAssert.AreEqual("https://simoona.example.com/en/groups/7", _appSettings.GroupUrl(Tenant, 7));
        }

        [Test]
        public void Should_Build_Book_Url_Without_The_Office_Segment()
        {
            ClassicAssert.AreEqual("https://simoona.example.com/en/books/3", _appSettings.BookUrl(Tenant, 3, 9));
        }

        [Test]
        public void Should_Build_Kudos_Profile_Url_With_The_User_As_A_Query_Param()
        {
            ClassicAssert.AreEqual("https://simoona.example.com/en/kudos?userId=user-1", _appSettings.KudosProfileUrl(Tenant, "user-1"));
        }

        [Test]
        public void Should_Build_Event_Url()
        {
            ClassicAssert.AreEqual("https://simoona.example.com/en/events/event-1", _appSettings.EventUrl(Tenant, "event-1"));
        }

        [Test]
        public void Should_Build_Event_List_By_Type_Url_With_The_Type_As_A_Query_Param()
        {
            ClassicAssert.AreEqual("https://simoona.example.com/en/events?typeId=4", _appSettings.EventListByTypeUrl(Tenant, "4"));
        }

        [Test]
        public void Should_Build_Project_Url()
        {
            ClassicAssert.AreEqual("https://simoona.example.com/en/projects/8", _appSettings.ProjectUrl(Tenant, "8"));
        }

        [Test]
        public void Should_Build_Service_Request_Url_Pointing_At_The_Request_Itself()
        {
            ClassicAssert.AreEqual("https://simoona.example.com/en/service-requests/11/edit", _appSettings.ServiceRequestUrl(Tenant, 11));
        }

        [Test]
        public void Should_Build_Feed_Url_As_The_Client_Root()
        {
            ClassicAssert.AreEqual("https://simoona.example.com/en/", _appSettings.FeedUrl(Tenant));
        }

        [Test]
        public void Should_Build_Reset_Password_Url_With_The_Organization_As_A_Query_Param()
        {
            ClassicAssert.AreEqual(
                "https://simoona.example.com/en/reset-password?UserName=jane%40example.com&Token=a%2Fb%2Bc&org=visma",
                _appSettings.ResetPasswordUrl(Tenant, "jane@example.com", "a/b+c"));
        }

        [Test]
        public void Should_Build_Verify_Email_Url_With_The_Organization_As_A_Query_Param()
        {
            ClassicAssert.AreEqual(
                "https://simoona.example.com/en/verify-email?UserName=jane%40example.com&Token=a%2Fb%2Bc&org=visma",
                _appSettings.VerifyEmailUrl(Tenant, "jane@example.com", "a/b+c"));
        }

        // Pictures are served by the API, not the client, so this one keeps the tenant
        // container and gets no locale.
        [Test]
        public void Should_Build_Picture_Url_Against_The_Api()
        {
            ClassicAssert.AreEqual(
                "https://api.simoona.example.com/storage/visma/pic1.jpg",
                _appSettings.PictureUrl("Visma", "pic1.jpg"));
        }
    }
}
