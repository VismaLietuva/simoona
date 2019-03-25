using System;
using System.Linq.Expressions;
using AutoMapper;
using Shrooms.DataTransferObjects.Models;

namespace Shrooms.ModelMappings
{
    internal static class Helpers
    {
        public static IMappingExpression<Src, Dest> Ignore<Src, Dest>(this IMappingExpression<Src, Dest> expr, Expression<Func<Dest, object>> selection)
        {
            return expr.ForMember(selection, opt => opt.Ignore());
        }

        public static IMappingExpression<Src, Dest> IgnoreUserOrgDto<Src, Dest>(this IMappingExpression<Src, Dest> expr) 
            where Dest : UserAndOrganizationDTO
        {
            return expr
                .ForMember(x => x.OrganizationId, opt => opt.Ignore())
                .ForMember(x => x.UserId, opt => opt.Ignore());
        }
    }
}
