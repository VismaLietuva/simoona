using Shrooms.Contracts.DataTransferObjects.FilterPresets;
using System.Threading.Tasks;

namespace Shrooms.Domain.ServiceValidators.Validators.FilterPresets
{
    public interface IFilterPresetValidator
    {
        Task CheckIfFilterPresetExistsAsync(CreateFilterPresetDto createDto);

        Task CheckIfFilterItemsExistsAsync(CreateFilterPresetDto createDto);

        void CheckIfFilterPresetItemsContainDuplicates(CreateFilterPresetDto createDto);
    }
}
