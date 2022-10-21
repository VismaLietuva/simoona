using NSubstitute;
using NUnit.Framework;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.Enums;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.Domain.ServiceValidators.Validators.FilterPresets;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using Shrooms.Contracts.Exceptions;
using Shrooms.Contracts.DataTransferObjects.FilterPresets;
using Shrooms.Tests.Extensions;
using System.Threading.Tasks;
using Shrooms.DataLayer.EntityModels.Models.Events;
using Shrooms.DataLayer.EntityModels.Models.Kudos;

namespace Shrooms.Tests.DomainService.Validators
{
    [TestFixture]
    public class FilterPresetValidatorTests
    {
        private IFilterPresetValidator _filterPresetValidator;

        private DbSet<FilterPreset> _filterPresetDbSet;
        private DbSet<Office> _officeDbSet;
        private DbSet<EventType> _eventTypeDbSet;
        private DbSet<KudosType> _kudosTypeDbSet;

        [SetUp]
        public void TestInitializer()
        {
            var uow = Substitute.For<IUnitOfWork2>();

            // ReSharper disable once CollectionNeverUpdated.Local
            var mockPresetData = new List<FilterPreset>();

            _officeDbSet = uow.MockDbSetForAsync<Office>();
            _eventTypeDbSet = uow.MockDbSetForAsync<EventType>();
            _kudosTypeDbSet = uow.MockDbSetForAsync<KudosType>();
            _filterPresetDbSet = uow.MockDbSetForAsync<FilterPreset>();

            _filterPresetDbSet.SetDbSetDataForAsync(mockPresetData);

            uow.GetDbSet<FilterPreset>().Returns(_filterPresetDbSet);
            uow.GetDbSet<EventType>().Returns(_eventTypeDbSet);
            uow.GetDbSet<KudosType>().Returns(_kudosTypeDbSet);
            uow.GetDbSet<Office>().Returns(_officeDbSet);

            _filterPresetValidator = new FilterPresetValidator(uow);
        }

        [Test]
        public void Should_Throw_If_PageType_Does_Not_Exist()
        {
            // Arrange
            const PageType invalidPageType = (PageType)int.MaxValue;

            // Assert
            Assert.Throws<ValidationException>(() => _filterPresetValidator.CheckIfPageTypeExists(invalidPageType));
        }

        [Test]
        public void Should_Not_Throw_If_PageType_Exists()
        {
            // Arrange
            const PageType validPageType = PageType.EventReport;

            // Assert
            Assert.DoesNotThrow(() => _filterPresetValidator.CheckIfPageTypeExists(validPageType));
        }

        [Test]
        public void Should_Throw_If_FilterTypes_Contains_Duplicates()
        {
            // Arrange
            var filterTypesWithDuplicates = new[]
            {
                FilterType.Offices,
                FilterType.Offices,
                FilterType.Kudos
            };

            // Assert
            Assert.Throws<ValidationException>(() => _filterPresetValidator.CheckIfFilterTypesContainDuplicates(filterTypesWithDuplicates));
        }

        [Test]
        public void Should_Not_Throw_If_FilterTypes_Does_Not_Contain_Duplicates()
        {
            // Arrange
            var filterTypesWithoutDuplicates = new[]
            {
                FilterType.Offices,
                FilterType.Kudos,
                FilterType.Events
            };

            // Assert
            Assert.DoesNotThrow(() => _filterPresetValidator.CheckIfFilterTypesContainDuplicates(filterTypesWithoutDuplicates));
        }

        [Test]
        public void Should_Not_Throw_If_FilterTypes_Are_Empty()
        {
            // Arrange
            var filterTypesEmpty = Array.Empty<FilterType>();

            // Assert
            Assert.DoesNotThrow(() => _filterPresetValidator.CheckIfFilterTypesContainDuplicates(filterTypesEmpty));
        }

        [Test]
        public void Should_Throw_If_More_Than_One_Default_Preset_Exists_In_Presets_That_Are_Going_To_Be_Added()
        {
            // Arrange
            var presetsToAddWithMoreThanOneDefault = new List<CreateFilterPresetDto>
            {
                new CreateFilterPresetDto
                {
                    IsDefault = true
                },
                new CreateFilterPresetDto
                {
                    IsDefault = true
                }
            };

            var updateDtoWithMoreThanOneDefaultPreset = new ManageFilterPresetDto
            {
                PresetsToCreate = presetsToAddWithMoreThanOneDefault,
                PresetsToUpdate = Enumerable.Empty<UpdateFilterPresetDto>()
            };

            // Assert
            Assert.Throws<ValidationException>(() => _filterPresetValidator.CheckIfMoreThanOneDefaultPresetExists(updateDtoWithMoreThanOneDefaultPreset));
        }

        [Test]
        public void Should_Throw_If_More_Than_One_Default_Preset_Exists_In_Presets_That_Are_Going_To_Be_Updated()
        {
            // Arrange
            var presetsToUpdateWithMoreThanOneDefault = new List<UpdateFilterPresetDto>
            {
                new UpdateFilterPresetDto
                {
                    IsDefault = true
                },
                new UpdateFilterPresetDto
                {
                    IsDefault = true
                }
            };

            var updateDtoWithMoreThanOneDefaultPreset = new ManageFilterPresetDto
            {
                PresetsToCreate = Enumerable.Empty<CreateFilterPresetDto>(),
                PresetsToUpdate = presetsToUpdateWithMoreThanOneDefault
            };

            // Assert
            Assert.Throws<ValidationException>(() => _filterPresetValidator.CheckIfMoreThanOneDefaultPresetExists(updateDtoWithMoreThanOneDefaultPreset));
        }

        [Test]
        public void Should_Throw_If_More_Than_One_Default_Preset_Exists_In_Create_And_Update_Collections()
        {
            // Arrange
            var presetsToAddWithOneDefaultPreset = new List<CreateFilterPresetDto>
            {
                new CreateFilterPresetDto
                {
                    IsDefault = true
                }
            };

            var presetsToUpdateWithOneDefaultPreset = new List<UpdateFilterPresetDto>
            {
                new UpdateFilterPresetDto
                {
                    IsDefault = true
                }
            };

            var manageDtoWithMoreThanOneDefaultPreset = new ManageFilterPresetDto
            {
                PresetsToCreate = presetsToAddWithOneDefaultPreset,
                PresetsToUpdate = presetsToUpdateWithOneDefaultPreset
            };

            // Assert
            Assert.Throws<ValidationException>(() => _filterPresetValidator.CheckIfMoreThanOneDefaultPresetExists(manageDtoWithMoreThanOneDefaultPreset));
        }

        [Test]
        public void Should_Not_Throw_If_One_Default_Preset_Exists()
        {
            // Arrange
            var presetsToAddWithOneDefaultPreset = new List<CreateFilterPresetDto>
            {
                new CreateFilterPresetDto
                {
                    IsDefault = true
                }
            };

            var presetsToUpdateWithoutDefaultPreset = new List<UpdateFilterPresetDto>
            {
                new UpdateFilterPresetDto
                {
                    IsDefault = false
                }
            };

            var manageDtoWithOneDefaultPreset = new ManageFilterPresetDto
            {
                PresetsToCreate = presetsToAddWithOneDefaultPreset,
                PresetsToUpdate = presetsToUpdateWithoutDefaultPreset
            };

            // Assert
            Assert.DoesNotThrow(() => _filterPresetValidator.CheckIfMoreThanOneDefaultPresetExists(manageDtoWithOneDefaultPreset));
        }

        [Test]
        public void Should_Not_Throw_If_Default_Preset_Does_Not_Exist()
        {
            // Arrange
            var presetsToAddWithoutDefaultPreset = new List<CreateFilterPresetDto>
            {
                new CreateFilterPresetDto
                {
                    IsDefault = false
                }
            };

            var presetsToUpdateWithoutDefaultPreset = new List<UpdateFilterPresetDto>
            {
                new UpdateFilterPresetDto
                {
                    IsDefault = false
                }
            };

            var manageDtoWithoutDefaultPreset = new ManageFilterPresetDto
            {
                PresetsToCreate = presetsToAddWithoutDefaultPreset,
                PresetsToUpdate = presetsToUpdateWithoutDefaultPreset
            };

            // Assert
            Assert.DoesNotThrow(() => _filterPresetValidator.CheckIfMoreThanOneDefaultPresetExists(manageDtoWithoutDefaultPreset));
        }

        [Test]
        public void Should_Not_Throw_If_Given_Type_Exists()
        {
            // Arrange
            var createPresets = new List<CreateFilterPresetDto>
            {
                new CreateFilterPresetDto
                {
                    Filters = new List<FilterPresetItemDto>
                    {
                        new FilterPresetItemDto
                        {
                            FilterType = FilterType.Events,
                            Types = new List<string>() { "1" }
                        },

                        new FilterPresetItemDto
                        {
                            FilterType = FilterType.Offices,
                            Types = new List<string>() { "1" }
                        }
                    }
                }
            };

            var updatePresets = new List<UpdateFilterPresetDto>
            {
                new UpdateFilterPresetDto
                {
                    Filters = new List<FilterPresetItemDto>
                    {
                        new FilterPresetItemDto
                        {
                            FilterType = FilterType.Events,
                            Types = new List<string>() { "1" }
                        },

                        new FilterPresetItemDto
                        {
                            FilterType = FilterType.Kudos,
                            Types = new List<string>() { "1" }
                        }
                    }
                }
            };

            var manageDto = new ManageFilterPresetDto
            {
                PresetsToCreate = createPresets,
                PresetsToUpdate = updatePresets
            };

            MockTypes();

            // Act
            Assert.DoesNotThrowAsync(async () => await _filterPresetValidator.CheckIfProvidedTypesInFiltersAreValidAsync(manageDto));
        }

        [Test]
        public void Should_Throw_If_Given_Type_Does_Not_Exist()
        {
            // Arrange
            var createPresets = new List<CreateFilterPresetDto>
            {
                new CreateFilterPresetDto
                {
                    Filters = new List<FilterPresetItemDto>
                    {
                        new FilterPresetItemDto
                        {
                            FilterType = FilterType.Events,
                            Types = new List<string>() { "121312" }
                        },

                        new FilterPresetItemDto
                        {
                            FilterType = FilterType.Offices,
                            Types = new List<string>() { "23" }
                        }
                    }
                }
            };

            var updatePresets = new List<UpdateFilterPresetDto>
            {
                new UpdateFilterPresetDto
                {
                    Filters = new List<FilterPresetItemDto>
                    {
                        new FilterPresetItemDto
                        {
                            FilterType = FilterType.Events,
                            Types = new List<string>() { "0" }
                        },

                        new FilterPresetItemDto
                        {
                            FilterType = FilterType.Kudos,
                            Types = new List<string>() { "1" }
                        }
                    }
                }
            };

            var manageDto = new ManageFilterPresetDto
            {
                PresetsToCreate = createPresets,
                PresetsToUpdate = updatePresets
            };

            MockTypes();

            // Act
            Assert.ThrowsAsync<ValidationException>(async () => await _filterPresetValidator.CheckIfProvidedTypesInFiltersAreValidAsync(manageDto));
        }

        [Test]
        public void Should_Throw_If_Counts_Are_Not_Equal()
        {
            // Arrange
            var firstCollection = new List<CreateFilterPresetDto>
            {
                new CreateFilterPresetDto()
            };
            var secondCollection = new List<UpdateFilterPresetDto>();

            // Assert
            Assert.Throws<ValidationException>(() => _filterPresetValidator.CheckIfCountsAreEqual(firstCollection, secondCollection));
        }

        [Test]
        public void Should_Not_Throw_If_Counts_Are_Equal()
        {
            // Arrange
            var firstCollection = new List<CreateFilterPresetDto>();
            var secondCollection = new List<UpdateFilterPresetDto>();

            // Assert
            Assert.DoesNotThrow(() => _filterPresetValidator.CheckIfCountsAreEqual(firstCollection, secondCollection));
        }

        [Test]
        public void Should_Throw_If_There_Are_Invalid_FilterTypes()
        {
            // Arrange
            var invalidFilterTypes = new FilterType[]
            {
                (FilterType)int.MaxValue,
                (FilterType)int.MinValue,
                FilterType.Offices
            };

            // Assert
            Assert.Throws<ValidationException>(() => _filterPresetValidator.CheckIfFilterTypesAreValid(invalidFilterTypes));
        }

        [Test]
        public void Should_Not_Throw_If_FilterTypes_Are_Valid()
        {
            // Arrange
            var validFilterTypes = new FilterType[]
            {
                FilterType.Kudos,
                FilterType.Offices,
                FilterType.Events
            };

            // Assert
            Assert.DoesNotThrow(() => _filterPresetValidator.CheckIfFilterTypesAreValid(validFilterTypes));
        }

        [Test]
        public void Should_Throw_If_FilterPresets_Collection_Contains_Duplicate_Names()
        {
            // Arrange
            var collectionWithDuplicateNames = new List<FilterPresetDto>
            {
                new FilterPresetDto
                {
                    Name = "Awesome filter preset"
                },

                new FilterPresetDto
                {
                    Name = "Awesome filter preset"
                }
            };

            // Assert
            Assert.Throws<ValidationException>(() => _filterPresetValidator.CheckIfFilterPresetsContainUniqueNames(collectionWithDuplicateNames));
        }

        [Test]
        public void Should_Not_Throw_If_FilterPresets_Collection_Does_Not_Contain_Duplicate_Names()
        {
            // Arrange
            var collectionWithDuplicateNames = new List<FilterPresetDto>
            {
                new FilterPresetDto
                {
                    Name = "Another awesome filter preset"
                },

                new FilterPresetDto
                {
                    Name = "Awesome filter preset"
                }
            };

            // Assert
            Assert.DoesNotThrow(() => _filterPresetValidator.CheckIfFilterPresetsContainUniqueNames(collectionWithDuplicateNames));
        }

        [Test]
        public void Should_Throw_If_Presets_To_Add_And_Update_Contain_Duplicate_Names()
        {
            // Arrange
            var presetsToAdd = new List<CreateFilterPresetDto>
            {
                new CreateFilterPresetDto
                {
                    Name = "I love writing tests"
                }
            };

            var presetsToUpdate = new List<UpdateFilterPresetDto>
            {
                new UpdateFilterPresetDto
                {
                    Name = "I love writing tests"
                }
            };

            var manageDto = new ManageFilterPresetDto
            {
                PresetsToCreate = presetsToAdd,
                PresetsToUpdate = presetsToUpdate
            };

            var removedPresets = Enumerable.Empty<FilterPresetDto>();

            // Assert
            Assert.ThrowsAsync<ValidationException>(async () => await _filterPresetValidator
                .CheckIfUpdatedAndAddedPresetsHaveUniqueNamesExcludingDeletedPresetsAsync(manageDto, removedPresets));
        }


        [Test]
        public void Should_Not_Throw_If_Presets_To_Add_And_Update_Does_Not_Contain_Duplicate_Names()
        {
            // Arrange
            var presetsToAdd = new List<CreateFilterPresetDto>
            {
                new CreateFilterPresetDto
                {
                    Name = "I hate writing tests"
                }
            };

            var presetsToUpdate = new List<UpdateFilterPresetDto>
            {
                new UpdateFilterPresetDto
                {
                    Name = "I love writing tests"
                }
            };

            var manageDto = new ManageFilterPresetDto
            {
                PresetsToCreate = presetsToAdd,
                PresetsToUpdate = presetsToUpdate
            };

            var removedPresets = Enumerable.Empty<FilterPresetDto>();

            // Assert
            Assert.DoesNotThrowAsync(async () => await _filterPresetValidator
                .CheckIfUpdatedAndAddedPresetsHaveUniqueNamesExcludingDeletedPresetsAsync(manageDto, removedPresets));
        }

        [Test]
        public void Should_Not_Throw_If_Presets_To_Add_Contains_Duplicate_Name_But_That_Preset_Is_About_To_Be_Deleted()
        {
            // Arrange
            var removedPresets = new List<FilterPresetDto>
            {
                new FilterPresetDto
                {
                    Id = 1,
                    Name = "Deleted"
                }
            };

            var presetsToAdd = new List<CreateFilterPresetDto>
            {
                new CreateFilterPresetDto
                {
                    Name = "Deleted"
                }
            };

            var manageDto = new ManageFilterPresetDto
            {
                PresetsToCreate = presetsToAdd,
                PresetsToUpdate = Enumerable.Empty<UpdateFilterPresetDto>()
            };

            // Assert
            Assert.DoesNotThrowAsync(async () =>
                await _filterPresetValidator.CheckIfUpdatedAndAddedPresetsHaveUniqueNamesExcludingDeletedPresetsAsync(manageDto, removedPresets));
        }


        [Test]
        public void Should_Not_Throw_If_Presets_To_Update_Contains_Duplicate_Name_But_That_Preset_Is_About_To_Be_Deleted()
        {
            // Arrange
            var removedPresets = new List<FilterPresetDto>
            {
                new FilterPresetDto
                {
                    Id = 1,
                    Name = "Deleted"
                }
            };

            var presetsToUpdate = new List<UpdateFilterPresetDto>
            {
                new UpdateFilterPresetDto
                {
                    Name = "Deleted"
                }
            };

            var manageDto = new ManageFilterPresetDto
            {
                PresetsToCreate = Enumerable.Empty<CreateFilterPresetDto>(),
                PresetsToUpdate = presetsToUpdate
            };

            // Assert
            Assert.DoesNotThrowAsync(async () =>
                await _filterPresetValidator.CheckIfUpdatedAndAddedPresetsHaveUniqueNamesExcludingDeletedPresetsAsync(manageDto, removedPresets));
        }

        [Test]
        public void Should_Throw_If_Presets_To_Add_Contain_Name_That_Is_Already_Created()
        {
            // Arrange
            _filterPresetDbSet.SetDbSetDataForAsync(new List<FilterPreset>
            {
                new FilterPreset
                {
                    Name = "Name"
                }
            });

            var removedPresets = Enumerable.Empty<FilterPresetDto>();

            var presetsToAdd = new List<CreateFilterPresetDto>
            {
                new CreateFilterPresetDto
                {
                    Name = "Name"
                }
            };

            var updateDto = new ManageFilterPresetDto
            {
                PresetsToCreate = presetsToAdd,
                PresetsToUpdate = Enumerable.Empty<UpdateFilterPresetDto>()
            };

            // Assert
            Assert.ThrowsAsync<ValidationException>(async () =>
                await _filterPresetValidator.CheckIfUpdatedAndAddedPresetsHaveUniqueNamesExcludingDeletedPresetsAsync(updateDto, removedPresets));
        }

        [Test]
        public void Should_Throw_If_Presets_To_Update_Contain_Name_That_Is_Already_Created()
        {
            // Arrange
            _filterPresetDbSet.SetDbSetDataForAsync(new List<FilterPreset>
            {
                new FilterPreset
                {
                    Id = 1,
                    Name = "Name"
                }
            });

            var removedPresets = Enumerable.Empty<FilterPresetDto>();

            var presetsToUpdate = new List<UpdateFilterPresetDto>
            {
                new UpdateFilterPresetDto
                {
                    Id = 2,
                    Name = "Name"
                }
            };

            var manageDto = new ManageFilterPresetDto
            {
                PresetsToCreate = Enumerable.Empty<CreateFilterPresetDto>(),
                PresetsToUpdate = presetsToUpdate
            };

            // Assert
            Assert.ThrowsAsync<ValidationException>(async () =>
                await _filterPresetValidator.CheckIfUpdatedAndAddedPresetsHaveUniqueNamesExcludingDeletedPresetsAsync(manageDto, removedPresets));
        }

        private void MockTypes()
        {
            _eventTypeDbSet.SetDbSetDataForAsync(new List<EventType>
            {
                new EventType
                {
                    Id = 1,
                }
            });

            _kudosTypeDbSet.SetDbSetDataForAsync(new List<KudosType>
            {
                new KudosType
                {
                    Id = 1
                }
            });

            _officeDbSet.SetDbSetDataForAsync(new List<Office>
            {
                new Office
                {
                    Id = 1
                }
            });
        }
    }
}