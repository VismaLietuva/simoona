using Mapster;
using Shrooms.Contracts.DataTransferObjects.BlacklistUsers;
using Shrooms.Presentation.WebViewModels.Models.BlacklistUsers;

namespace Shrooms.Presentation.ModelMappings.Profiles
{
    /// <summary>Mapster translation of the <see cref="BlacklistUsers"/> AutoMapper profile.</summary>
    public class BlacklistUsersRegister : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            CreateViewModelToDtoMappings(config);
            CreateDtoToViewModelMappings(config);
        }

        private static void CreateViewModelToDtoMappings(TypeAdapterConfig config)
        {
            config.NewConfig<CreateBlacklistUserViewModel, BlacklistUserDto>()
                .Ignore(
                    x => x.Modified,
                    x => x.ModifiedBy,
                    x => x.ModifiedByUserFirstName,
                    x => x.ModifiedByUserLastName,
                    x => x.Created,
                    x => x.CreatedBy,
                    x => x.CreatedByUserFirstName,
                    x => x.CreatedByUserLastName,
                    x => x.Status);
            config.NewConfig<UpdateBlacklistUserViewModel, BlacklistUserDto>()
                .Ignore(
                    x => x.Modified,
                    x => x.ModifiedBy,
                    x => x.ModifiedByUserFirstName,
                    x => x.ModifiedByUserLastName,
                    x => x.Created,
                    x => x.CreatedBy,
                    x => x.CreatedByUserFirstName,
                    x => x.CreatedByUserLastName,
                    x => x.Status);
            config.NewConfig<CreateBlacklistUserViewModel, CreateBlacklistUserDto>();
            config.NewConfig<UpdateBlacklistUserViewModel, UpdateBlacklistUserDto>();
        }

        private static void CreateDtoToViewModelMappings(TypeAdapterConfig config)
        {
            config.NewConfig<BlacklistUserDto, BlacklistUserViewModel>();
        }
    }
}
