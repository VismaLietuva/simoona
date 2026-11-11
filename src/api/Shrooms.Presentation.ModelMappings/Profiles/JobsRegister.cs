using Mapster;
using Shrooms.Contracts.DataTransferObjects.Models.Jobs;
using Shrooms.Presentation.WebViewModels.Models.Jobs;

namespace Shrooms.Presentation.ModelMappings.Profiles
{
    /// <summary>Mapster translation of the <see cref="Jobs"/> AutoMapper profile.</summary>
    public class JobsRegister : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            CreateDtoToViewModelMappings(config);
            CreateViewModelToDtoMappings(config);
        }

        private static void CreateDtoToViewModelMappings(TypeAdapterConfig config)
        {
            config.NewConfig<JobTypeDto, JobTypeViewModel>();
            config.NewConfig<JobTypeDto, NewJobTypeViewModel>();
        }

        private static void CreateViewModelToDtoMappings(TypeAdapterConfig config)
        {
            config.NewConfig<JobTypeViewModel, JobTypeDto>();
            config.NewConfig<NewJobTypeViewModel, JobTypeDto>();
        }
    }
}
