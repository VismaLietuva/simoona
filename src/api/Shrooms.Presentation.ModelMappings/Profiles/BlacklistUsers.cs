using AutoMapper;
using Shrooms.Contracts.DataTransferObjects.BlacklistUsers;
using Shrooms.Presentation.WebViewModels.Models.BlacklistUsers;

namespace Shrooms.Presentation.ModelMappings.Profiles
{
    public class BlacklistUsers : Profile
    {
        protected override void Configure()
        {
            CreateViewModelToDtoMappings();
            CreateDtoToViewModelMappings();
        }

        private void CreateViewModelToDtoMappings()
        {
            CreateMap<CreateBlacklistUserViewModel, BlacklistUserDto>();
            CreateMap<UpdateBlacklistUserViewModel, BlacklistUserDto>();
            CreateMap<CreateBlacklistUserViewModel, CreateBlacklistUserDto>();
            CreateMap<UpdateBlacklistUserViewModel, UpdateBlacklistUserDto>();
        }

        private void CreateDtoToViewModelMappings()
        {
            CreateMap<BlacklistUserDto, BlacklistUserViewModel>();
        }
    }
}
