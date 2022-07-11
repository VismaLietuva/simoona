using AutoMapper;
using Shrooms.Contracts.DataTransferObjects.BlacklistStates;
using Shrooms.Presentation.WebViewModels.Models.BlacklistStates;

namespace Shrooms.Presentation.ModelMappings.Profiles
{
    public class BlacklistStates : Profile
    {
        protected override void Configure()
        {
            CreateViewModelToDtoMappings();
            CreateDtoToViewModelMappings();
        }

        private void CreateViewModelToDtoMappings()
        {
            CreateMap<CreateBlacklistStateViewModel, BlacklistStateDto>();
            CreateMap<UpdateBlacklistStateViewModel, BlacklistStateDto>();
            CreateMap<CreateBlacklistStateViewModel, CreateBlacklistStateDto>();
            CreateMap<UpdateBlacklistStateViewModel, UpdateBlacklistStateDto>();
        }

        private void CreateDtoToViewModelMappings()
        {
            CreateMap<BlacklistStateDto, BlacklistStateViewModel>();
        }
    }
}
