using System;
using System.Linq.Expressions;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DataTransferObjects.Models.Kudos;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.DataLayer.EntityModels.Models.Kudos;

namespace Shrooms.Domain.Helpers
{
    public static class KudosServiceHelper
    {
        public static Expression<Func<KudosLog, bool>> StatusFilter(string kudosLogStatus)
        {
            if (string.Equals(kudosLogStatus, BusinessLayerConstants.KudosStatusAllFilter, StringComparison.OrdinalIgnoreCase))
            {
                return log => true;
            }

            var kudosType = (KudosStatus)Enum.Parse(typeof(KudosStatus), kudosLogStatus, true);
            return log => log.Status == kudosType;
        }

        public static Expression<Func<KudosLog, bool>> UserFilter(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return log => true;
            }

            return log => log.EmployeeId == userId;
        }

        public static Expression<Func<KudosLog, ApplicationUser, MainKudosLogDto>> MapKudosLogsToDto()
        {
            return (log, sender) => new MainKudosLogDto
            {
                Comment = log.Comments,
                Created = log.Created,
                Sender = new KudosLogUserDto
                {
                    Id = log.CreatedBy,
                    FullName = sender == null ? string.Empty : sender.FirstName + " " + sender.LastName,
                    PictureId = sender == null ? null : sender.PictureId
                },
                Id = log.Id,
                Points = log.Points,
                Receiver = new KudosLogUserDto
                {
                    Id = log.Employee == null ? null : log.Employee.Id,
                    FullName = log.Employee == null ? string.Empty : log.Employee.FirstName + " " + log.Employee.LastName,
                    PictureId = log.Employee == null ? null : log.Employee.PictureId
                },
                Type = new KudosLogTypeDto
                {
                    Name = log.KudosTypeName,
                    Value = log.KudosTypeValue,
                    Type = log.KudosSystemType
                },
                Status = log.Status.ToString(),
                Multiplier = log.MultiplyBy,
                RejectionMessage = log.RejectionMessage,
                PictureId = log.PictureId
            };
        }

        internal static Expression<Func<KudosLog, bool>> TypeFilter(string filteringType)
        {
            if (string.IsNullOrEmpty(filteringType) || filteringType.Equals("All", StringComparison.OrdinalIgnoreCase))
            {
                return x => true;
            }

            return x => x.KudosTypeName == filteringType;
        }
    }
}
