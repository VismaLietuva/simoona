using System;
using System.ComponentModel.DataAnnotations.Schema;
using Shrooms.Constants.BusinessLayer;
using Shrooms.DomainExceptions.Exceptions;
using Shrooms.Host.Contracts.Constants;

namespace Shrooms.EntityModels.Models.Kudos
{
    public class KudosLog : BaseModelWithOrg
    {
        [ForeignKey("Employee")]
        public string EmployeeId { get; set; }

        public virtual ApplicationUser Employee { get; set; }

        public string KudosTypeName { get; set; }

        public decimal KudosTypeValue { get; set; }

        public BusinessLayerConstants.KudosTypeEnum KudosSystemType { get; set; }

        public KudosStatus Status { get; set; }

        public decimal Points { get; set; }

        public string Comments { get; set; }

        public int MultiplyBy { get; set; }

        [ForeignKey("KudosBasket")]
        public int? KudosBasketId { get; set; }

        public virtual KudosBasket KudosBasket { get; set; }

        public string RejectionMessage { get; set; }

        public bool IsRecipientDeleted() => !string.IsNullOrEmpty(EmployeeId) && Employee == null;

        public bool IsMinus() => KudosSystemType == BusinessLayerConstants.KudosTypeEnum.Minus;

        public string PictureId { get; set; }

        public void Approve(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                throw new ValidationException(ErrorCodes.UserNotFound, "User not found");
            }

            if (userId == CreatedBy)
            {
                throw new ValidationException(ErrorCodes.SenderReceiverCannotAcceptRejectKudos, "Sender/Receiver cannot accept/reject kudos logs");
            }

            if (userId == EmployeeId)
            {
                throw new ValidationException(ErrorCodes.CanNotSendKudosToSelf, "Kudos receiver can not be a sender");
            }

            StatusShouldntBeProcessed();

            Status = KudosStatus.Approved;
            Modified = DateTime.UtcNow;
            ModifiedBy = userId;
        }

        public void Reject(string userId, string reason)
        {
            if (string.IsNullOrEmpty(userId))
            {
                throw new ValidationException(ErrorCodes.UserNotFound, "User not found");
            }

            if (string.IsNullOrEmpty(reason))
            {
                throw new ValidationException(ErrorCodes.ContentDoesNotExist, "Rejection message is empty");
            }

            StatusShouldntBeProcessed();

            Status = KudosStatus.Rejected;
            RejectionMessage = reason;
            Modified = DateTime.UtcNow;
            ModifiedBy = userId;
        }

        private void StatusShouldntBeProcessed()
        {
            if (Status != KudosStatus.Pending)
            {
                throw new ValidationException(ErrorCodes.KudosAlreadyApproved, "Kudos log is already approved");
            }
        }
    }
}
