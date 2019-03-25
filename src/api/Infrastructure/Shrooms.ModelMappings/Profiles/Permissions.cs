using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Shrooms.DataTransferObjects.Models.Permissions;
using Shrooms.WebViewModels.Models;

namespace Shrooms.ModelMappings.Profiles
{
    public class Permissions : Profile
    {
        protected override void Configure()
        {
            CreateDtoToViewModelMappings();
        }

        private void CreateDtoToViewModelMappings()
        {
            CreateMap<PermissionGroupDTO, PermissionGroupViewModel>();
        }
    }
}
