using System;
using System.Linq;
using System.Reflection;
using AutoMapper;
using NUnit.Framework;
using System.Collections.Generic;
using Shrooms.Contracts.DataTransferObjects.Models.Users;
using Shrooms.Contracts.DataTransferObjects.Models.VideoLibrary;
using Shrooms.Presentation.ModelMappings.Profiles;
using Shrooms.Presentation.WebViewModels.Models.User;
using Shrooms.Presentation.WebViewModels.Models.VideoLibrary;

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
        public void VideoUpdate_WithoutMaterials_MapsToNull()
        {
            var dto = _mapper.Map<VideoLibraryItemViewModel, VideoLibraryItemDto>(new VideoLibraryItemViewModel { Id = 1 });

            Assert.That(dto.Materials, Is.Null);
        }

        [Test]
        public void VideoUpdate_WithMaterials_MapsThem()
        {
            var viewModel = new VideoLibraryItemViewModel
            {
                Id = 1,
                Materials = new List<VideoLibraryMaterialViewModel>
                {
                    new() { Title = "Slides", Url = "https://docs.google.com/presentation/d/s/edit" }
                }
            };

            var dto = _mapper.Map<VideoLibraryItemViewModel, VideoLibraryItemDto>(viewModel);

            Assert.That(dto.Materials.Single().Title, Is.EqualTo("Slides"));
            Assert.That(dto.Materials.Single().Url, Is.EqualTo("https://docs.google.com/presentation/d/s/edit"));
        }

        [Test]
        public void VideoUpdate_WithEmptyMaterials_StaysEmpty()
        {
            var viewModel = new VideoLibraryItemViewModel { Id = 1, Materials = new List<VideoLibraryMaterialViewModel>() };

            var dto = _mapper.Map<VideoLibraryItemViewModel, VideoLibraryItemDto>(viewModel);

            Assert.That(dto.Materials, Is.Empty);
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
