using AutoMapper;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DataTransferObjects.Models.Support;
using Shrooms.Domain.Services.Support;
using Shrooms.Presentation.Common.Controllers;
using Shrooms.Presentation.Common.Filters;
using Shrooms.Presentation.WebViewModels.Models.Support;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace Shrooms.Presentation.Api.Controllers
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
        public async Task<HttpResponseMessage> SubmitTicket(SupportPostViewModel support)
        {
            var maxSupportTypeIndex = Enum.GetValues(typeof(SupportType)).Cast<int>().Max();

            if (!ModelState.IsValid || maxSupportTypeIndex < support.Type)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }

            var supportDto = _mapper.Map<SupportPostViewModel, SupportDto>(support);

            await _supportService.SubmitTicketAsync(GetUserAndOrganization(), supportDto);

            return Request.CreateResponse(HttpStatusCode.Created);
        }

        [PermissionAuthorize(Permission = BasicPermissions.Support)]
        public IEnumerable<SupportType> GetSupportTypes()
        {
            return Enum.GetValues(typeof(SupportType)).Cast<SupportType>();
        }
    }
}
