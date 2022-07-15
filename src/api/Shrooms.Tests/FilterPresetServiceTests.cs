using NSubstitute;
using NUnit.Framework;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.Enums;
using Shrooms.Domain.Services.FilterPresets;
using Shrooms.Domain.ServiceValidators.Validators.FilterPresets;
using System.Threading.Tasks;
using System.Data.Entity;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.DataLayer.EntityModels.Models.Events;
using System.Linq;
using Shrooms.Tests.Extensions;
using System.Collections.Generic;
using Shrooms.DataLayer.EntityModels.Models.Kudos;
using Shrooms.Contracts.DataTransferObjects.FilterPresets;
using Shrooms.Contracts.DataTransferObjects;

namespace Shrooms.Tests
{
    [TestFixture]
    public class FilterPresetServiceTests
    {
        private IFilterPresetValidator _filterPresetValidator;
        private DbSet<FilterPreset> _filterPresetDbSet;

        private DbSet<Office> _officeDbSet;
        private DbSet<KudosType> _kudosTypeDbSet;
        private DbSet<EventType> _eventTypeDbSet;

        private IFilterPresetService _filterPresetService;

        [SetUp]
        public void TestInitializer()
        {
            var uow = Substitute.For<IUnitOfWork2>();

            _officeDbSet = uow.MockDbSetForAsync<Office>();
            _kudosTypeDbSet = uow.MockDbSetForAsync<KudosType>();
            _eventTypeDbSet = uow.MockDbSetForAsync<EventType>();

            _filterPresetDbSet = uow.MockDbSetForAsync<FilterPreset>();
            _filterPresetValidator = Substitute.For<IFilterPresetValidator>();

            _filterPresetService = new FilterPresetService(uow, _filterPresetValidator);
        }

        [Test]
        public async Task Should_Get_Presets_For_Page()
        {
            // Arrange
            var mockData = new List<FilterPreset>
            {
                new FilterPreset
                {
                    Id = 1,
                    OrganizationId = 1,
                    ForPage = PageType.EventReportList,
                    Preset = string.Empty
                },

                new FilterPreset
                {
                    Id = 2,
                    OrganizationId = 1,
                    ForPage = PageType.EventReport,
                    Preset = string.Empty
                },

                new FilterPreset
                {
                    Id = 3,
                    OrganizationId = 1,
                    ForPage = PageType.EventReportList,
                    Preset = string.Empty
                }
            };

            _filterPresetDbSet.SetDbSetDataForAsync(mockData);

            const int expectedCount = 2;
            const int expectedFirstItemId = 1;
            const int expectedSecondItemId = 3;

            const PageType pageType = PageType.EventReportList;
            const int organizationId = 1;

            // Act
            var actualResult = (await _filterPresetService.GetPresetsForPageAsync(pageType, organizationId))
                .ToList();

            // Assert
            _filterPresetValidator
                .Received(1)
                .CheckIfPageTypeExists(Arg.Is(pageType));

            Assert.AreEqual(actualResult.Count, expectedCount);
            Assert.AreEqual(actualResult[0].Id, expectedFirstItemId);
            Assert.AreEqual(actualResult[1].Id, expectedSecondItemId);
        }

        [TestCase(new FilterType[] { FilterType.Kudos })]
        [TestCase(new FilterType[] { FilterType.Offices, FilterType.Events, FilterType.Kudos })]
        [TestCase(new FilterType[] { FilterType.Offices, FilterType.Events })]
        [TestCase(new FilterType[] { })]
        public async Task Should_Get_Specified_Filters(IList<FilterType> filterTypes)
        {
            // Arrange
            var mockEventTypes = new List<EventType>
            {
                new EventType
                {
                    Id = 1,
                    Name = "First event type"
                },

                new EventType
                {
                    Id = 2,
                    Name = "Second event type"
                }
            };

            var mockOffices = new List<Office>
            {
                new Office
                {
                    Id = 1,
                    Name = "First office"
                },

                new Office
                {
                    Id = 2,
                    Name = "Second office"
                }
            };

            var mockKudosTypes = new List<KudosType>
            {
                new KudosType
                {
                    Id = 1,
                    Name = "First kudos type"
                },

                new KudosType
                {
                    Id = 2,
                    Name = "Second kudos type"
                }
            };

            _officeDbSet.SetDbSetDataForAsync(mockOffices);
            _kudosTypeDbSet.SetDbSetDataForAsync(mockKudosTypes);
            _eventTypeDbSet.SetDbSetDataForAsync(mockEventTypes);

            // Act
            var result = await _filterPresetService.GetFiltersAsync(filterTypes.ToArray(), 0);

            // Assert
            _filterPresetValidator
                .Received(1)
                .CheckIfFilterTypesContainDuplicates(Arg.Any<FilterType[]>());

            _filterPresetValidator
                .Received(1)
                .CheckIfFilterTypesAreValid(Arg.Any<FilterType[]>());

            Assert.That(result, Is.All.Matches<FiltersDto>(filtersDto => filterTypes.Contains(filtersDto.FilterType)));
        }

        [TestCase(4, 0)]
        [TestCase(8, 1)]
        public async Task Should_Update_FilterPresets(int presetToDeleteId, int organizationId)
        {
            // Arrange
            var mockFilterPresets = new List<FilterPreset>
            {
                new FilterPreset
                {
                    Id = 1,
                    OrganizationId = 0,
                    ForPage = PageType.EventReportList,
                    Preset = string.Empty
                },

                new FilterPreset
                {
                    Id = 2,
                    OrganizationId = 0,
                    ForPage = PageType.EventReportList,
                    Preset = string.Empty
                },

                new FilterPreset
                {
                    Id = 3,
                    OrganizationId = 0,
                    ForPage = PageType.EventReport,
                    Preset = string.Empty
                },

                new FilterPreset
                {
                    Id = 4,
                    OrganizationId = 0,
                    ForPage = PageType.EventReport,
                    Preset = string.Empty
                },
                new FilterPreset
                {
                    Id = 5,
                    OrganizationId = 1,
                    ForPage = PageType.EventReportList,
                    Preset = string.Empty
                },

                new FilterPreset
                {
                    Id = 6,
                    OrganizationId = 1,
                    ForPage = PageType.EventReportList,
                    Preset = string.Empty
                },

                new FilterPreset
                {
                    Id = 7,
                    OrganizationId = 1,
                    ForPage = PageType.EventReport,
                    Preset = string.Empty
                },

                new FilterPreset
                {
                    Id = 8,
                    OrganizationId = 1,
                    ForPage = PageType.EventReport,
                    Preset = string.Empty
                }
            };

            _filterPresetDbSet.SetDbSetDataForAsync(mockFilterPresets);

            var presetToUpdate = new UpdateFilterPresetDto
            {
                Id = 3,
                Name = "Updated preset",
                IsDefault = false,
                Filters = Enumerable.Empty<FilterPresetItemDto>()
            };

            var presetToCreate = new CreateFilterPresetDto
            {
                Name = "Created preset",
                IsDefault = false,
                Filters = Enumerable.Empty<FilterPresetItemDto>()
            };

            var userOrg = new UserAndOrganizationDto
            {
                OrganizationId = organizationId
            };

            var updateDto = new ManageFilterPresetDto
            {
                PageType = PageType.EventReport,
                PresetsToUpdate = new List<UpdateFilterPresetDto>
                {
                    presetToUpdate
                },
                PresetsToCreate = new List<CreateFilterPresetDto>
                {
                    presetToCreate
                },
                PresetsToDelete = new List<int>
                {
                    presetToDeleteId
                },
                UserOrg = userOrg
            };

            // Act
            var result = await _filterPresetService.UpdateAsync(updateDto);

            // Assert
            _filterPresetValidator
                .Received(1)
                .CheckIfCountsAreEqual(Arg.Any<List<int>>(), Arg.Any<Dictionary<int, FilterPreset>>());

            await _filterPresetValidator
                .Received(1)
                .CheckIfUpdatedAndAddedPresetsHaveUniqueNamesExcludingDeletedPresetsAsync(
                    Arg.Any<ManageFilterPresetDto>(),
                    Arg.Any<IEnumerable<FilterPresetDto>>());

            _filterPresetValidator
                .Received(1)
                .CheckIfMoreThanOneDefaultPresetExists(Arg.Any<ManageFilterPresetDto>());

            _filterPresetValidator
                .Received(2)
                .CheckIfFilterPresetsContainUniqueNames(Arg.Any<IEnumerable<FilterPresetDto>>());

            Assert.NotNull(result.CreatedPresets
                .FirstOrDefault(preset => preset.IsDefault == presetToCreate.IsDefault &&
                                          preset.Id == presetToCreate.Id &&
                                          preset.Name == presetToCreate.Name));

            Assert.NotNull(result.UpdatedPresets
                .FirstOrDefault(preset => preset.IsDefault == presetToUpdate.IsDefault &&
                                          preset.Id == presetToUpdate.Id &&
                                          preset.Name == presetToUpdate.Name));

            Assert.NotNull(result.DeletedPresets
                .FirstOrDefault(preset => preset.Id == presetToDeleteId));
        }
    }
}
