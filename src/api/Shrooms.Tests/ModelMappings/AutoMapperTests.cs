using System;
using System.Linq;
using System.Reflection;
using AutoMapper;
using NUnit.Framework;
using Shrooms.Contracts.DataTransferObjects.Models.Users;
using Shrooms.Presentation.ModelMappings.Profiles;
using Shrooms.Presentation.WebViewModels.Models.User;

namespace Shrooms.Tests.ModelMappings
{
    [TestFixture]
    public class AutoMapperTests
    {
        private IMapper _mapper;

        [SetUp]
        public void TestInitialize()
        {
            _mapper = ModelMapper.Create();
        }

        // The two shapes are mapped with MemberList.None, which validates nothing:
        // a switch added to one and not the other is silently dropped on the way in,
        // and the user sees a setting that will not stick.
        [Test]
        public void NotificationSettings_ViewModelAndDto_CarryTheSameSwitches()
        {
            Assert.That(SwitchNames(typeof(UserNotificationsSettingsViewModel)),
                Is.EquivalentTo(SwitchNames(typeof(UserNotificationsSettingsDto))));
        }

        [Test]
        public void Mapping_User_Models()
        {
            _mapper.ConfigurationProvider.AssertConfigurationIsValid();
        }

        [Test]
        public void Mapping_Wall_Models()
        {
            _mapper.ConfigurationProvider.AssertConfigurationIsValid();
        }

        [Test]
        public void Mapping_External_Link_Models()
        {
            _mapper.ConfigurationProvider.AssertConfigurationIsValid();
        }

        [Test]
        public void Mapping_Monitor_Models()
        {
            _mapper.ConfigurationProvider.AssertConfigurationIsValid();
        }

        [Test]
        public void Mapping_Role_Models()
        {
            _mapper.ConfigurationProvider.AssertConfigurationIsValid();
        }

        [Test]
        public void Mapping_Permission_Models()
        {
            _mapper.ConfigurationProvider.AssertConfigurationIsValid();
        }

        [Test]
        public void Mapping_Like_Models()
        {
            _mapper.ConfigurationProvider.AssertConfigurationIsValid();
        }

        [Test]
        public void Mapping_VacationPage_Models()
        {
            _mapper.ConfigurationProvider.AssertConfigurationIsValid();
        }

        [Test]
        public void Mapping_FilterPreset_Models()
        {
            _mapper.ConfigurationProvider.AssertConfigurationIsValid();
        }

        [Test]
        public void Mapping_Employee_Models()
        {
            _mapper.ConfigurationProvider.AssertConfigurationIsValid();
        }

        [Test]
        public void Mapping_BlacklistUsers_Models()
        {
            _mapper.ConfigurationProvider.AssertConfigurationIsValid();
        }

        private static string[] SwitchNames(Type type)
        {
            return type
                .GetFields(BindingFlags.Public | BindingFlags.Instance)
                .Where(field => field.FieldType == typeof(bool))
                .Select(field => field.Name)
                .ToArray();
        }
    }
}
