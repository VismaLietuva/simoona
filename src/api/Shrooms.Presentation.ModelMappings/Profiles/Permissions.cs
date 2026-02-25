using AutoMapper;
using Shrooms.Contracts.DataTransferObjects.Models.Permissions;
using Shrooms.Presentation.WebViewModels.Models;

namespace Shrooms.Presentation.ModelMappings.Profiles
{
    public class Permissions : Profile
    {
        public Permissions()
        {
            CreateDtoToViewModelMappings();
        }

        private void CreateDtoToViewModelMappings()
        {
            CreateMap<PermissionGroupDto, PermissionGroupViewModel>();
        }
    }
}
