using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NUnit.Framework;
using Shrooms.Contracts.Enums;
using Shrooms.DataLayer.EntityModels.Models.Events;
using Shrooms.Premium.Constants;
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

        [Test]
        public void Should_Carry_Questions_And_Their_Options_Through_The_CreateEventViewModel_To_CreateEventDto_Map()
        {
            var viewModel = new CreateEventViewModel
            {
                Name = "Summer BBQ",
                StartDate = new DateTime(2026, 6, 1),
                EndDate = new DateTime(2026, 6, 1),
                RegistrationDeadlineDate = new DateTime(2026, 5, 25),
                Recurrence = EventRecurrenceOptions.None,
                Location = "Office",
                ResponsibleUserId = "user-1",
                Offices = new List<int>(),
                Questions = new List<EventQuestionViewModel>
                {
                    new EventQuestionViewModel
                    {
                        ClientId = "q1",
                        Title = "Which pizza?",
                        Order = 0,
                        SelectType = EventQuestionSelectType.Single,
                        IsRequired = true,
                        Options = new List<EventQuestionOptionViewModel>
                        {
                            new EventQuestionOptionViewModel { ClientId = "o1", Name = "Margherita", Order = 0, Rule = OptionRules.Default }
                        }
                    }
                }
            };

            var dto = _mapper.Map<CreateEventViewModel, CreateEventDto>(viewModel);

            Assert.That(dto.Questions, Is.Not.Empty);
            var question = dto.Questions.First();
            Assert.That(question.Title, Is.EqualTo("Which pizza?"));
            Assert.That(question.Options, Is.Not.Empty);
            Assert.That(question.Options.First().Name, Is.EqualTo("Margherita"));
        }

        [Test]
        public void Should_Carry_Questions_And_Their_Options_Through_The_UpdateEventViewModel_To_EditEventDto_Map()
        {
            var viewModel = new UpdateEventViewModel
            {
                Id = "1",
                Name = "Summer BBQ",
                ImageName = "image.png",
                StartDate = new DateTime(2026, 6, 1),
                EndDate = new DateTime(2026, 6, 1),
                RegistrationDeadlineDate = new DateTime(2026, 5, 25),
                Recurrence = EventRecurrenceOptions.None,
                Location = "Office",
                ResponsibleUserId = "user-1",
                Offices = new List<int>(),
                Questions = new List<EventQuestionViewModel>
                {
                    new EventQuestionViewModel
                    {
                        ClientId = "q1",
                        Title = "Which pizza?",
                        Order = 0,
                        SelectType = EventQuestionSelectType.Single,
                        IsRequired = true,
                        Options = new List<EventQuestionOptionViewModel>
                        {
                            new EventQuestionOptionViewModel { ClientId = "o1", Name = "Margherita", Order = 0, Rule = OptionRules.Default }
                        }
                    }
                }
            };

            var dto = _mapper.Map<UpdateEventViewModel, EditEventDto>(viewModel);

            Assert.That(dto.Questions, Is.Not.Empty);
            var question = dto.Questions.First();
            Assert.That(question.Title, Is.EqualTo("Which pizza?"));
            Assert.That(question.Options, Is.Not.Empty);
            Assert.That(question.Options.First().Name, Is.EqualTo("Margherita"));
        }

        [Test]
        public void Should_Unflatten_A_Real_OptionId_Condition_From_The_Dto()
        {
            var dto = new EventQuestionStructureDto
            {
                ShowIfOptionId = 41,
                ShowIfOptionClientId = null
            };

            var viewModel = _mapper.Map<EventQuestionStructureDto, EventQuestionViewModel>(dto);

            Assert.That(viewModel.ShowIf, Is.Not.Null);
            Assert.That(viewModel.ShowIf.OptionId, Is.EqualTo(41));
        }

        [Test]
        public void Should_Unflatten_A_ClientId_Condition_From_The_Dto()
        {
            var dto = new EventQuestionStructureDto
            {
                ShowIfOptionId = null,
                ShowIfOptionClientId = "o1"
            };

            var viewModel = _mapper.Map<EventQuestionStructureDto, EventQuestionViewModel>(dto);

            Assert.That(viewModel.ShowIf, Is.Not.Null);
            Assert.That(viewModel.ShowIf.OptionClientId, Is.EqualTo("o1"));
        }

        [Test]
        public void Should_Map_Both_Null_Condition_Fields_To_A_Null_ShowIf()
        {
            var dto = new EventQuestionStructureDto
            {
                ShowIfOptionId = null,
                ShowIfOptionClientId = null
            };

            var viewModel = _mapper.Map<EventQuestionStructureDto, EventQuestionViewModel>(dto);

            Assert.That(viewModel.ShowIf, Is.Null);
        }

        [Test]
        public void Should_Map_Question_Structure_To_The_SignUp_Read_Model()
        {
            var dto = new EventQuestionStructureDto
            {
                Id = 7,
                ClientId = "ignored",
                Title = "Pick your dish",
                Order = 2,
                SelectType = EventQuestionSelectType.Multi,
                IsRequired = true,
                ShowIfOptionId = 101,
                ShowIfOptionClientId = "ignored",
                Options = new List<EventQuestionOptionStructureDto>
                {
                    new EventQuestionOptionStructureDto { Id = 11, Name = "Pizza", Order = 0, Rule = OptionRules.Default }
                }
            };

            var result = _mapper.Map<EventQuestionStructureDto, EventSignUpQuestionViewModel>(dto);

            Assert.That(result.Id, Is.EqualTo(7));
            Assert.That(result.Title, Is.EqualTo("Pick your dish"));
            Assert.That(result.Order, Is.EqualTo(2));
            Assert.That(result.SelectType, Is.EqualTo(EventQuestionSelectType.Multi));
            Assert.That(result.IsRequired, Is.True);
            Assert.That(result.ShowIfOptionId, Is.EqualTo(101));
            Assert.That(result.Options, Has.Count.EqualTo(1));
            Assert.That(result.Options[0].Id, Is.EqualTo(11));
            Assert.That(result.Options[0].Name, Is.EqualTo("Pizza"));
            Assert.That(result.Options[0].Order, Is.EqualTo(0));
        }

        [Test]
        public void Should_Serialize_SelectType_As_A_String_For_The_Client()
        {
            var viewModel = new EventSignUpQuestionViewModel
            {
                Id = 1,
                Title = "Pick your dish",
                SelectType = EventQuestionSelectType.Single,
                ShowIfOptionId = null
            };

            var json = JsonConvert.SerializeObject(viewModel, new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            });

            Assert.That(json, Does.Contain("\"selectType\":\"Single\""));
            Assert.That(json, Does.Contain("\"showIfOptionId\":null"));
        }

        // The viewmodel -> DTO map is convention-based (MemberList.None), so nothing would fail
        // loudly if ChosenOptions stopped mapping: the answers would silently never reach the
        // service and every status change into Going on a question event would 400.
        [Test]
        public void Should_Carry_The_Chosen_Options_Of_A_Status_Change_Onto_The_Dto()
        {
            var viewModel = new UpdateAttendStatusViewModel
            {
                EventId = Guid.NewGuid(),
                AttendStatus = AttendingStatus.Attending,
                ChosenOptions = new List<int> { 135, 136 }
            };

            var dto = _mapper.Map<UpdateAttendStatusViewModel, UpdateAttendStatusDto>(viewModel);

            Assert.That(dto.ChosenOptions, Is.EquivalentTo(new[] { 135, 136 }));
        }
    }
}
