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
            CreateViewModelToDtoMappings();
        }

        private void CreateViewModelToDtoMappings()
        {
            CreateMap<CreateMonitorViewModel, MonitorDto>()
                .Ignore(x => x.Id);
            CreateMap<MonitorViewModel, MonitorDto>();
        }

        private void CreateDtoToViewModelMappings()
        {
            CreateMap<MonitorDto, MonitorViewModel>();
        }
    }
}
