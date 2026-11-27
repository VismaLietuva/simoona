using Mapster;
using Shrooms.Contracts.DataTransferObjects.Employees;
using Shrooms.Presentation.WebViewModels.Models.Employees;

namespace Shrooms.Presentation.ModelMappings.Profiles
{
    /// <summary>Mapster translation of the <see cref="Employees"/> AutoMapper profile.</summary>
    public class EmployeesRegister : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            CreateViewModelToDtoMappings(config);
            CreateDtoToViewModelMappings(config);
        }

        private static void CreateViewModelToDtoMappings(TypeAdapterConfig config)
        {
            config.NewConfig<EmployeeListingArgsViewModel, EmployeeListingArgsDto>();
        }

        private static void CreateDtoToViewModelMappings(TypeAdapterConfig config)
        {
            config.NewConfig<EmployeeDto, EmployeeViewModel>()
                .Map(dest => dest.BlacklistEndDate, src => src.BlacklistEntry.EndDate);
            config.NewConfig<WorkingHourslWithOutLunchDto, WorkingHourslWithOutLunchViewModel>();
        }
    }
}
