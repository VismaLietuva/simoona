using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.DataTransferObjects.FilterPresets;
using Shrooms.Contracts.Enums;
using System.Collections;
using System.Threading.Tasks;

namespace Shrooms.Domain.ServiceValidators.Validators.FilterPresets
{
    public interface IFilterPresetValidator
    {
        //Task CheckIfFilterPresetExistsAsync(FilterPresetDto createDto);
        
        //Task CheckIfFilterPresetExistsAsync(int id, UserAndOrganizationDto userOr);

        //Task CheckIfFilterItemsExistsAsync(FilterPresetDto createDto);

        void CheckIfFilterTypeIsValid(FilterType filterType);

        void CheckIfFilterTypesContainsDuplicates(FilterType[] filterTypes);

        //void CheckIfFilterPresetItemsContainDuplicates(FilterPresetDto createDto);

        void CheckIfPageTypeExists(PageType page);

        void CheckIfCountsAreEqual<T1, T2>(T1 obj1, T2 obj2)
            where T1 : ICollection
            where T2 : ICollection;

        void CheckIfMoreThanOneDefaultPresetExists(AddEditDeleteFilterPresetDto updateDto);

        void CheckIfUniqueNames(AddEditDeleteFilterPresetDto updateDto);
    }
}
