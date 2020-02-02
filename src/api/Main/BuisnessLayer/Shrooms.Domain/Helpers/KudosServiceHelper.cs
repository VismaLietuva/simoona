using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Shrooms.Constants.BusinessLayer;
using Shrooms.DataTransferObjects.Models.Kudos;
using Shrooms.EntityModels.Models;
using Shrooms.EntityModels.Models.Kudos;

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

        public static Expression<Func<KudosLog, IEnumerable<ApplicationUser>, MainKudosLogDTO>> MapKudosLogsToDto()
        {
            return (log, users) => new MainKudosLogDTO
            {
                Comment = log.Comments,
                Created = log.Created,
                Sender = new KudosLogUserDTO
                {
                    Id = log.CreatedBy,
                    FullName = users.Select(u => u.FirstName + " " + u.LastName).FirstOrDefault() ?? string.Empty
                },
                Id = log.Id,
                Points = log.Points,
                Receiver = new KudosLogUserDTO
                {
                    Id = log.Employee == null ? null : log.Employee.Id,
                    FullName = log.Employee == null ? string.Empty : log.Employee.FirstName + " " + log.Employee.LastName
                },
                Type = new KudosLogTypeDTO
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
