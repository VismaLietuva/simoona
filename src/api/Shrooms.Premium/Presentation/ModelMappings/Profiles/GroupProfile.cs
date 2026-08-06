using AutoMapper;
using Shrooms.Premium.DataTransferObjects.Models.Groups;
using Shrooms.Premium.Presentation.WebViewModels.Groups;

namespace Shrooms.Premium.Presentation.ModelMappings.Profiles
{
    public class GroupProfile : Profile
    {
        public GroupProfile()
        {
            CreateViewModelToDtoMappings();
            CreateDtoToViewModelMappings();
        }

        private void CreateViewModelToDtoMappings()
        {
            CreateMap<CreateGroupTypeViewModel, CreateGroupTypeDto>(MemberList.None);
            CreateMap<UpdateGroupTypeViewModel, UpdateGroupTypeDto>(MemberList.None);
            CreateMap<GroupPostViewModel, GroupPostDto>(MemberList.None);
            CreateMap<GroupMemberPostViewModel, GroupMemberPostDto>(MemberList.None);
            CreateMap<GroupReferenceViewModel, GroupReferenceDto>(MemberList.None);
        }

        private void CreateDtoToViewModelMappings()
        {
            CreateMap<GroupTypeDto, GroupTypeViewModel>(MemberList.None);
            CreateMap<GroupDto, GroupViewModel>(MemberList.None);
            CreateMap<GroupMemberDto, GroupMemberViewModel>(MemberList.None);
            CreateMap<GroupReferenceDto, GroupReferenceViewModel>(MemberList.None);
        }
    }
}
