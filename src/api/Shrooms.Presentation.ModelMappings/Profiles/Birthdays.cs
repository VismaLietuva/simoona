using AutoMapper;
using Shrooms.Contracts.DataTransferObjects.Models.Birthdays;
using Shrooms.Presentation.WebViewModels.Models.Birthday;

namespace Shrooms.Presentation.ModelMappings.Profiles
{
    public class Birthdays : Profile
    {
        protected override void Configure()
        {
            CreateDtoToViewModelMappings();
        }

        private void CreateDtoToViewModelMappings()
        {
            CreateMap<BirthdayDto, BirthdayViewModel>();
        }
    }
}
