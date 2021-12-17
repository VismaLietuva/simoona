namespace Shrooms.Premium.DataTransferObjects.Models.ServiceRequest
{
    public class CreatedServiceRequestDto
    {
        public int ServiceRequestId { get; set; }
    }

    public class UpdatedServiceRequestDto : CreatedServiceRequestDto
    {
        public int NewStatusId { get; set; }
    }

    public class ServiceRequestCreatedCommentDto : CreatedServiceRequestDto
    {
        public string CommentedEmployeeId { get; set; }
        public string CommentContent { get; set; }
    }
}
