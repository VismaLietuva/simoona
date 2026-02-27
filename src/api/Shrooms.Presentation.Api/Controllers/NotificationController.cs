using AutoMapper;
using Shrooms.Contracts.DataTransferObjects.Notification;
using Shrooms.Contracts.Enums;
using Shrooms.Contracts.ViewModels.Notifications;
using Shrooms.Domain.Services.Notifications;
using Shrooms.Presentation.Common.Controllers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Shrooms.Presentation.Api.Controllers
{
    [Authorize]
    public class NotificationController : BaseController
    {
        private readonly INotificationService _notificationService;
        private readonly IMapper _mapper;

        public NotificationController(INotificationService notificationService, IMapper mapper)
        {
            _notificationService = notificationService;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IEnumerable<NotificationViewModel>> GetAll()
        {
            var result = await _notificationService.GetAllAsync(GetUserAndOrganization());

            return MakeCommentsStacked(result);
        }

        [HttpPut]
        public async Task<IActionResult> MarkAsRead(IEnumerable<int> ids)
        {
            await _notificationService.MarkAsReadAsync(GetUserAndOrganization(), ids);

            return Ok();
        }

        [HttpPut]
        public async Task<IActionResult> MarkAllAsRead()
        {
            await _notificationService.MarkAllAsReadAsync(GetUserAndOrganization());

            return Ok();
        }

        private IEnumerable<NotificationViewModel> MakeCommentsStacked(IEnumerable<NotificationDto> comments)
        {
            var stackedList = new List<NotificationViewModel>();

            foreach (var item in comments)
            {
                var parentComment = stackedList
                    .FirstOrDefault(x => CompareSourcesIds(x.sourceIds, item.SourceIds) && item.Type != NotificationType.EventReminder);

                if (parentComment == null)
                {
                    stackedList.Add(_mapper.Map<NotificationViewModel>(item));
                }
                else
                {
                    parentComment.stackedIds.Add(item.Id);
                    if (parentComment.title.Equals(item.Title) == false)
                    {
                        parentComment.others++;
                    }
                }
            }

            return stackedList;
        }

        private static bool CompareSourcesIds(SourcesViewModel viewModel, SourcesDto dtoModel)
        {
            if (viewModel.PostId != dtoModel.PostId || viewModel.EventId != dtoModel.EventId || viewModel.ProjectId != dtoModel.ProjectId || viewModel.WallId != dtoModel.WallId)
            {
                return false;
            }

            return true;
        }
    }
}
