using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using NUnit.Framework;
using Shrooms.Contracts.Enums;
using Shrooms.Premium.DataTransferObjects.Models.Events;
using Shrooms.Premium.Presentation.ModelMappings.Profiles;
using Shrooms.Premium.Presentation.WebViewModels.Events;

namespace Shrooms.Premium.Tests.Controllers.ViewModels
{
    public class EventQuestionMappingTests
    {
        private IMapper _mapper;

        [SetUp]
        public void TestInitializer()
        {
            var config = new MapperConfiguration(cfg => cfg.AddProfile<EventsProfile>());
            _mapper = config.CreateMapper();
        }

        [Test]
        public void Should_Flatten_A_ClientId_Condition_Onto_The_Dto()
        {
            var viewModel = new EventQuestionViewModel
            {
                Id = null,
                ClientId = "q2",
                Title = "Which pizza?",
                Order = 1,
                SelectType = EventQuestionSelectType.Single,
                IsRequired = true,
                ShowIf = new EventQuestionConditionViewModel { OptionClientId = "o1" },
                Options = new List<EventQuestionOptionViewModel>
                {
                    new EventQuestionOptionViewModel { ClientId = "o3", Name = "Margherita", Order = 0, Rule = OptionRules.Default }
                }
            };

            var dto = _mapper.Map<EventQuestionViewModel, EventQuestionStructureDto>(viewModel);

            Assert.That(dto.ShowIfOptionClientId, Is.EqualTo("o1"));
            Assert.That(dto.ShowIfOptionId, Is.Null);
            Assert.That(dto.Options.Single().Name, Is.EqualTo("Margherita"));
        }

        [Test]
        public void Should_Flatten_A_Real_OptionId_Condition_Onto_The_Dto()
        {
            var viewModel = new EventQuestionViewModel
            {
                Id = 12,
                Title = "Anything we should know?",
                Order = 2,
                SelectType = EventQuestionSelectType.Multi,
                IsRequired = false,
                ShowIf = new EventQuestionConditionViewModel { OptionId = 41 },
                Options = new List<EventQuestionOptionViewModel>()
            };

            var dto = _mapper.Map<EventQuestionViewModel, EventQuestionStructureDto>(viewModel);

            Assert.That(dto.ShowIfOptionId, Is.EqualTo(41));
            Assert.That(dto.ShowIfOptionClientId, Is.Null);
        }

        [Test]
        public void Should_Map_A_Null_Condition_To_An_Always_Shown_Question()
        {
            var viewModel = new EventQuestionViewModel
            {
                ClientId = "q1",
                Title = "Pick your dish",
                Order = 0,
                ShowIf = null,
                Options = new List<EventQuestionOptionViewModel>()
            };

            var dto = _mapper.Map<EventQuestionViewModel, EventQuestionStructureDto>(viewModel);

            Assert.That(dto.ShowIfOptionId, Is.Null);
            Assert.That(dto.ShowIfOptionClientId, Is.Null);
        }
    }
}
