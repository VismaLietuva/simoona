using AutoMapper;
using Shrooms.DataTransferObjects.Models.Birthdays;
using Shrooms.WebViewModels.Models.Birthday;

namespace Shrooms.ModelMappings.Profiles
{
    public class Birthdays : Profile
    {
        protected override void Configure()
        {
            CreateDtoToViewModelMappings();
        }

        private void CreateDtoToViewModelMappings()
        {
            CreateMap<BirthdayDTO, BirthdayViewModel>();
        }
    }
}
