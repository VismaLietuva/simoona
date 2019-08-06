using Shrooms.Infrastructure.FireAndForget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shrooms.Infrastructure.Logger;
using Shrooms.DataTransferObjects.Models.ServiceRequest;
using Shrooms.Domain.Services.Email.ServiceRequest;
using Shrooms.DataTransferObjects.Models;

namespace Shrooms.Premium.Main.PresentationLayer.Shrooms.API.BackgroundWorkers
{
    public class ServiceRequestNotifier : IBackgroundWorker
    {
        private readonly ILogger _logger;
        private readonly IServiceRequestNotificationService _notificationService;

        public ServiceRequestNotifier(ILogger logger, IServiceRequestNotificationService notificationService)
        {
            _logger = logger;
            _notificationService = notificationService;
        }

        public void NotifyOnCreate(CreatedServiceRequestDTO createdRequest)
        {
            try
            {
                _notificationService.NotifyAboutNewServiceRequest(createdRequest);
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
        }

        public void NotifyOnStatusUpdate(UpdatedServiceRequestDTO updatedRequest, UserAndOrganizationDTO userAndOrganizationDTO)
        {
            try
            {
                _notificationService.NotifyAboutServiceRequestStatusUpdate(updatedRequest, userAndOrganizationDTO);
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
        }

        public void NotifyOnComment(ServiceRequestCreatedCommentDTO comment)
        {
            try
            {
                _notificationService.NotifyAboutNewComment(comment);
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
        }
    }
}
