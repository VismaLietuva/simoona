using AutoMapper;
using Shrooms.Contracts.DataTransferObjects.Models.Jobs;
using Shrooms.Presentation.WebViewModels.Models.Jobs;

namespace Shrooms.Presentation.ModelMappings.Profiles
{
    public class Jobs : Profile
    {
        public Jobs()
        {
            CreateDtoToViewModelMappings();
            CreateViewModelToDtoMappings();
        }

        private void CreateDtoToViewModelMappings()
        {
            CreateMap<JobTypeDto, JobTypeViewModel>(MemberList.None);
            CreateMap<JobTypeDto, NewJobTypeViewModel>(MemberList.None);
        }

        private void CreateViewModelToDtoMappings()
        {
            CreateMap<JobTypeViewModel, JobTypeDto>(MemberList.None);
            CreateMap<NewJobTypeViewModel, JobTypeDto>(MemberList.None);
        }
    }
}
