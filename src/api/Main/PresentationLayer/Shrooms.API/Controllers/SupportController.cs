using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using AutoMapper;
using Shrooms.API.Filters;
using Shrooms.Constants.Authorization.Permissions;
using Shrooms.DataTransferObjects.Models.Support;
using Shrooms.Domain.Services.Support;
using Shrooms.EntityModels.Models;
using Shrooms.WebViewModels.Models.Support;

namespace Shrooms.API.Controllers
{
    [Authorize]
    public class SupportController : BaseController
    {
        private readonly IMapper _mapper;
        private readonly ISupportService _supportService;

        public SupportController(IMapper mapper, ISupportService supportService)
        {
            _mapper = mapper;
            _supportService = supportService;
        }

        [PermissionAuthorize(Permission = BasicPermissions.Support)]
        [HttpPost]
        public HttpResponseMessage SubmitTicket(SupportPostViewModel support)
        {
            var maxSupportTypeIndex = Enum.GetValues(typeof(SupportType)).Cast<int>().Max();

            if (!ModelState.IsValid || maxSupportTypeIndex < support.Type)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }

            var supportDto = _mapper.Map<SupportPostViewModel, SupportDto>(support);

            _supportService.SubmitTicket(GetUserAndOrganization(), supportDto);

            return Request.CreateResponse(HttpStatusCode.Created);
        }

        [PermissionAuthorize(Permission = BasicPermissions.Support)]
        public IEnumerable<SupportType> GetSupportTypes()
        {
            return Enum.GetValues(typeof(SupportType)).Cast<SupportType>();
        }
    }
}