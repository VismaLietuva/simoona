using AutoMapper;
using Shrooms.Contracts.DataTransferObjects.Models.Monitors;
using Shrooms.Presentation.WebViewModels.Models.Monitors;

namespace Shrooms.Presentation.ModelMappings.Profiles
{
    public class Monitors : Profile
    {
        protected override void Configure()
        {
            CreateDtoToViewModelMappings();
            CreateViewModelToDTOMappings();
        }

        private void CreateViewModelToDTOMappings()
        {
            CreateMap<CreateMonitorViewModel, MonitorDTO>()
                .Ignore(x => x.Id);
            CreateMap<MonitorViewModel, MonitorDTO>();
        }

        private void CreateDtoToViewModelMappings()
        {
            CreateMap<MonitorDTO, MonitorViewModel>();
        }
    }
}
