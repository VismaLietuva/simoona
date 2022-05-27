using Shrooms.Contracts.DataTransferObjects.FilterPresets;
using Shrooms.Contracts.Enums;
using System.Threading.Tasks;

namespace Shrooms.Domain.ServiceValidators.Validators.FilterPresets
{
    public interface IFilterPresetValidator
    {
        Task CheckIfFilterPresetExistsAsync(FilterPresetDto createDto);

        Task CheckIfFilterItemsExistsAsync(FilterPresetDto createDto);

        void CheckIfFilterPresetItemsContainDuplicates(FilterPresetDto createDto);

        void CheckIfPageTypeExists(PageType page);
    }
}
