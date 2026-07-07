using AutoMapper;
using Shrooms.Contracts.DataTransferObjects.Models.Projects;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.Presentation.WebViewModels.Models.Projects;

namespace Shrooms.Presentation.ModelMappings.Profiles
{
    public class Projects : Profile
    {
        public Projects()
        {
            CreateDtoToViewModelMappings();
            CreateViewModelToDtoMappings();
            CreateEntityToViewModelMappings();
        }

        private void CreateDtoToViewModelMappings()
        {
            CreateMap<ProjectsListItemDto, ProjectsListItemViewModel>(MemberList.None);
            CreateMap<ProjectsAutoCompleteDto, ProjectsBasicInfoViewModel>(MemberList.None);
            CreateMap<EditProjectDisplayDto, EditProjectDisplayViewModel>(MemberList.None);
            CreateMap<ProjectDetailsDto, ProjectDetailsViewModel>(MemberList.None);
        }

        private void CreateViewModelToDtoMappings()
        {
            CreateMap<NewProjectViewModel, NewProjectDto>(MemberList.None);
            CreateMap<EditProjectViewModel, EditProjectDto>(MemberList.None);
        }

        private void CreateEntityToViewModelMappings()
        {
            CreateMap<Project, ProjectsBasicInfoViewModel>(MemberList.None);
        }
    }
}
