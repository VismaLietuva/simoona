using AutoMapper;
using Shrooms.Contracts.DataTransferObjects.BlacklistUsers;
using Shrooms.Presentation.WebViewModels.Models.BlacklistUsers;

namespace Shrooms.Presentation.ModelMappings.Profiles
{
    public class BlacklistUsers : Profile
    {
        public BlacklistUsers()
        {
            CreateViewModelToDtoMappings();
            CreateDtoToViewModelMappings();
        }

        private void CreateViewModelToDtoMappings()
        {
            CreateMap<CreateBlacklistUserViewModel, BlacklistUserDto>()
                .Ignore(opt => opt.Modified)
                .Ignore(opt => opt.ModifiedBy)
                .Ignore(opt => opt.ModifiedByUserFirstName)
                .Ignore(opt => opt.ModifiedByUserLastName)
                .Ignore(opt => opt.Created)
                .Ignore(opt => opt.CreatedBy)
                .Ignore(opt => opt.CreatedByUserFirstName)
                .Ignore(opt => opt.CreatedByUserLastName)
                .Ignore(opt => opt.Status);
            CreateMap<UpdateBlacklistUserViewModel, BlacklistUserDto>()
                .Ignore(opt => opt.Modified)
                .Ignore(opt => opt.ModifiedBy)
                .Ignore(opt => opt.ModifiedByUserFirstName)
                .Ignore(opt => opt.ModifiedByUserLastName)
                .Ignore(opt => opt.Created)
                .Ignore(opt => opt.CreatedBy)
                .Ignore(opt => opt.CreatedByUserFirstName)
                .Ignore(opt => opt.CreatedByUserLastName)
                .Ignore(opt => opt.Status);
            CreateMap<CreateBlacklistUserViewModel, CreateBlacklistUserDto>();
            CreateMap<UpdateBlacklistUserViewModel, UpdateBlacklistUserDto>();
        }

        private void CreateDtoToViewModelMappings()
        {
            CreateMap<BlacklistUserDto, BlacklistUserViewModel>();
        }
    }
}
