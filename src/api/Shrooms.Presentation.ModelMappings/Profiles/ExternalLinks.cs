using AutoMapper;
using Shrooms.Contracts.DataTransferObjects.Models.ExternalLinks;
using Shrooms.Presentation.WebViewModels.Models.ExternalLink;

namespace Shrooms.Presentation.ModelMappings.Profiles
{
    public class ExternalLinks : Profile
    {
        public ExternalLinks()
        {
            CreateDtoToViewModelMappings();
            CreateViewModelToDtoMappings();
        }

        private void CreateDtoToViewModelMappings()
        {
            CreateMap<ExternalLinkDto, ExternalLinkViewModel>(MemberList.None);
        }

        private void CreateViewModelToDtoMappings()
        {
            CreateMap<ManageExternalLinkViewModel, ManageExternalLinkDto>(MemberList.None)
                .IgnoreUserOrgDto();
            CreateMap<UpdatedExternalLinkViewModel, ExternalLinkDto>(MemberList.None);
            CreateMap<NewExternalLinkViewModel, NewExternalLinkDto>(MemberList.None);
            CreateMap<NewExternalLinkViewModel, ExternalLinkDto>(MemberList.None)
                .Ignore(x => x.Id);
        }
    }
}
