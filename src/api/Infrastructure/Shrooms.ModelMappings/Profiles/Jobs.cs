using AutoMapper;
using Shrooms.DataTransferObjects.Models.Jobs;
using Shrooms.WebViewModels.Models.Jobs;

namespace Shrooms.ModelMappings.Profiles
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
