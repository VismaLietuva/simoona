using Mapster;
using Shrooms.Premium.DataTransferObjects.Models.Groups;
using Shrooms.Premium.Presentation.WebViewModels.Groups;

namespace Shrooms.Premium.Presentation.ModelMappings.Profiles
{
    /// <summary>Mapster translation of the <see cref="GroupProfile"/> AutoMapper profile.</summary>
    public class GroupRegister : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            CreateViewModelToDtoMappings(config);
            CreateDtoToViewModelMappings(config);
        }

        private static void CreateViewModelToDtoMappings(TypeAdapterConfig config)
        {
            config.NewConfig<CreateGroupTypeViewModel, CreateGroupTypeDto>();
            config.NewConfig<UpdateGroupTypeViewModel, UpdateGroupTypeDto>();
            config.NewConfig<GroupPostViewModel, GroupPostDto>();
            config.NewConfig<GroupMemberPostViewModel, GroupMemberPostDto>();
            config.NewConfig<GroupReferenceViewModel, GroupReferenceDto>();
        }

        private static void CreateDtoToViewModelMappings(TypeAdapterConfig config)
        {
            config.NewConfig<GroupTypeDto, GroupTypeViewModel>();
            config.NewConfig<GroupDto, GroupViewModel>();
            config.NewConfig<GroupMemberDto, GroupMemberViewModel>();
            config.NewConfig<GroupReferenceDto, GroupReferenceViewModel>();
        }
    }
}
