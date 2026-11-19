using Mapster;
using Shrooms.Contracts.DataTransferObjects.Models.Projects;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.Presentation.WebViewModels.Models.Projects;

namespace Shrooms.Presentation.ModelMappings.Profiles
{
    /// <summary>Mapster translation of the <see cref="Projects"/> AutoMapper profile.</summary>
    public class ProjectsRegister : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            CreateDtoToViewModelMappings(config);
            CreateViewModelToDtoMappings(config);
            CreateEntityToViewModelMappings(config);
        }

        private static void CreateDtoToViewModelMappings(TypeAdapterConfig config)
        {
            config.NewConfig<ProjectsListItemDto, ProjectsListItemViewModel>();
            config.NewConfig<ProjectsAutoCompleteDto, ProjectsBasicInfoViewModel>();
            config.NewConfig<EditProjectDisplayDto, EditProjectDisplayViewModel>();
            config.NewConfig<ProjectDetailsDto, ProjectDetailsViewModel>();
        }

        private static void CreateViewModelToDtoMappings(TypeAdapterConfig config)
        {
            config.NewConfig<NewProjectViewModel, NewProjectDto>();
            config.NewConfig<EditProjectViewModel, EditProjectDto>();
        }

        private static void CreateEntityToViewModelMappings(TypeAdapterConfig config)
        {
            config.NewConfig<Project, ProjectsBasicInfoViewModel>();
        }
    }
}
