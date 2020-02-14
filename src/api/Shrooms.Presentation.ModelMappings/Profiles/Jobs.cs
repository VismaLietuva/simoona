using AutoMapper;
using Shrooms.Contracts.DataTransferObjects.Models.Jobs;
using Shrooms.Presentation.WebViewModels.Models.Jobs;

namespace Shrooms.Presentation.ModelMappings.Profiles
{
    public class Jobs : Profile
    {
        protected override void Configure()
        {
            CreateDtoToViewModelMappings();
            CreateViewModelToDtoMappings();
        }

        private void CreateDtoToViewModelMappings()
        {
            CreateMap<JobTypeDTO, JobTypeViewModel>();
            CreateMap<JobTypeDTO, NewJobTypeViewModel>();
        }

        private void CreateViewModelToDtoMappings()
        {
            CreateMap<JobTypeViewModel, JobTypeDTO>();
            CreateMap<NewJobTypeViewModel, JobTypeDTO>();
        }
    }
}
