using AutoMapper;
using NUnit.Framework;
using Shrooms.Presentation.ModelMappings.Profiles;

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

        [Test]
        public void Mapping_User_Models()
        {
            _mapper.ConfigurationProvider.AssertConfigurationIsValid<Users>();
        }

        [Test]
        public void Mapping_Wall_Models()
        {
            _mapper.ConfigurationProvider.AssertConfigurationIsValid<Walls>();
        }

        [Test]
        public void Mapping_External_Link_Models()
        {
            _mapper.ConfigurationProvider.AssertConfigurationIsValid<ExternalLinks>();
        }

        [Test]
        public void Mapping_Monitor_Models()
        {
            _mapper.ConfigurationProvider.AssertConfigurationIsValid<Monitors>();
        }

        [Test]
        public void Mapping_Role_Models()
        {
            _mapper.ConfigurationProvider.AssertConfigurationIsValid<Roles>();
        }

        [Test]
        public void Mapping_Permission_Models()
        {
            _mapper.ConfigurationProvider.AssertConfigurationIsValid<Permissions>();
        }
    }
}
