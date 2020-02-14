using AutoMapper;
using Shrooms.Contracts.DataTransferObjects.Models.Projects;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.Presentation.WebViewModels.Models.Projects;

namespace Shrooms.Presentation.ModelMappings.Profiles
{
    public class Projects : Profile
    {
        protected override void Configure()
        {
            CreateDtoToViewModelMappings();
            CreateViewModelToDtoMappings();
            CreateEntityToViewModelMappings();
        }

        private void CreateDtoToViewModelMappings()
        {
            CreateMap<ProjectsListItemDto, ProjectsListItemViewModel>();
            CreateMap<ProjectsAutoCompleteDto, ProjectsBasicInfoViewModel>();
            CreateMap<EditProjectDisplayDto, EditProjectDisplayViewModel>();
            CreateMap<ProjectDetailsDto, ProjectDetailsViewModel>();
        }

        private void CreateViewModelToDtoMappings()
        {
            CreateMap<NewProjectViewModel, NewProjectDto>();
            CreateMap<EditProjectViewModel, EditProjectDto>();
        }

        private void CreateEntityToViewModelMappings()
        {
            CreateMap<Project, ProjectsBasicInfoViewModel>();
        }
    }
}
