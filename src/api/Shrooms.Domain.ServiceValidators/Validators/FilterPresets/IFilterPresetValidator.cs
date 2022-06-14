using Shrooms.Contracts.DataTransferObjects.FilterPresets;
using Shrooms.Contracts.Enums;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Shrooms.Domain.ServiceValidators.Validators.FilterPresets
{
    public interface IFilterPresetValidator
    {
        void CheckIfFilterTypesAreValid(FilterType[] filterTypes);

        void CheckIfFilterTypesContainsDuplicates(FilterType[] filterTypes);

        void CheckIfPageTypeExists(PageType page);

        void CheckIfCountsAreEqual<T1, T2>(T1 firstCollection, T2 secondCollection)
            where T1 : ICollection
            where T2 : ICollection;

        void CheckIfMoreThanOneDefaultPresetExists(AddEditDeleteFilterPresetDto updateDto);

        Task CheckIfUniqueNamesAsync(AddEditDeleteFilterPresetDto updateDto, IEnumerable<FilterPresetDto> removedPresets);
    }
}
