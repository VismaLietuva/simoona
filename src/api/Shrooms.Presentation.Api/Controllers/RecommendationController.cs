using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using AutoMapper;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DataTransferObjects.Models.Recommendation;
using Shrooms.Domain.Services.Recommendation;
using Shrooms.Presentation.Api.Filters;
using Shrooms.Presentation.WebViewModels.Models.Recommendation;
namespace Shrooms.Presentation.Api.Controllers
{
    [Authorize]
    public class RecommendationController : BaseController
    {
        private readonly IMapper _mapper;
        private readonly IRecommendationService _recommendationService;

        public RecommendationController(IMapper mapper, IRecommendationService recommendationService)
        {
            _mapper = mapper;
            _recommendationService = recommendationService;
        }

        //[PermissionAuthorize(Permission = BasicPermissions.Recommendation)]
        [HttpPost]
        public async Task<HttpResponseMessage> SubmitTicket(RecommendationPostViewModel recommendation)
        {
            if (!ModelState.IsValid)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }

            var recommendationDto = _mapper.Map<RecommendationPostViewModel, RecommendationDto>(recommendation);

            await _recommendationService.SubmitTicketAsync(GetUserAndOrganization(), recommendationDto);

            return Request.CreateResponse(HttpStatusCode.Created);
        }

    }
}