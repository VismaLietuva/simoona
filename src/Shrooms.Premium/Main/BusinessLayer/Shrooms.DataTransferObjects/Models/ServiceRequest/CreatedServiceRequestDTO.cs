using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shrooms.DataTransferObjects.Models.ServiceRequest
{
    public class CreatedServiceRequestDTO
    {
        public int ServiceRequestId { get; set; }
    }

    public class UpdatedServiceRequestDTO : CreatedServiceRequestDTO
    {
        public int? NewStatusId { get; set; }
    }

    public class ServiceRequestCreatedCommentDTO : CreatedServiceRequestDTO
    {
        public string CommentedEmployeeId { get; set; }
        public string CommentContent { get; set; }
    }
}
