using AutoMapper;
using NUnit.Framework;
using Shrooms.Premium.Presentation.ModelMappings.Profiles;

namespace Shrooms.Premium.Tests.ModelMappings
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
        public void Mapping_Event_Models()
        {
            _mapper.ConfigurationProvider.AssertConfigurationIsValid();
        }

        [Test]
        public void Mapping_Kudos_Shop_Models()
        {
            _mapper.ConfigurationProvider.AssertConfigurationIsValid();
        }

        [Test]
        public void Mapping_Organizational_Structure_Models()
        {
            _mapper.ConfigurationProvider.AssertConfigurationIsValid();
        }

        [Test]
        public void Mapping_Lotteries_Models()
        {
            _mapper.ConfigurationProvider.AssertConfigurationIsValid();
        }
    }
}
