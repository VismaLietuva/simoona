using System;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects.Models;
using Shrooms.Contracts.DataTransferObjects.Models.RefreshTokens;
using Shrooms.DataLayer.EntityModels.Models;

namespace Shrooms.Domain.Services.RefreshTokens
{
    public class RefreshTokenService : IRefreshTokenService
    {
        private readonly IDbSet<RefreshToken> _refreshTokensDbSet;
        private readonly IUnitOfWork2 _uow;

        public RefreshTokenService(IUnitOfWork2 uow)
        {
            _uow = uow;
            _refreshTokensDbSet = uow.GetDbSet<RefreshToken>();
        }

        public void AddNewToken(RefreshTokenDTO tokenDto)
        {
            var timestamp = DateTime.UtcNow;
            var newToken = new RefreshToken
            {
                Id = tokenDto.Id,
                ExpiresUtc = tokenDto.ExpiresUtc,
                IssuedUtc = tokenDto.IssuedUtc,
                Subject = tokenDto.Subject,
                ProtectedTicket = tokenDto.ProtectedTicket,
                Created = timestamp,
                Modified = timestamp,
                OrganizationId = tokenDto.OrganizationId,
                CreatedBy = tokenDto.Subject,
                ModifiedBy = tokenDto.Subject
            };

            _refreshTokensDbSet.Add(newToken);
            _uow.SaveChanges(false);
        }

        public RefreshToken GetTokenTicketById(string id)
        {
            var refreshToken = _refreshTokensDbSet
                .FirstOrDefault(x => x.Id == id);

            return refreshToken;
        }

        public void RemoveTokenBySubject(UserAndOrganizationDTO userOrg)
        {
            RemoveToken(x =>
                x.Subject == userOrg.UserId &&
                x.OrganizationId == userOrg.OrganizationId);
        }

        public void RemoveTokenById(string id)
        {
            RemoveToken(x => x.Id == id);
        }

        private void RemoveToken(Expression<Func<RefreshToken, bool>> filter)
        {
            var refreshToken = _refreshTokensDbSet.FirstOrDefault(filter);

            if (refreshToken != null)
            {
                _refreshTokensDbSet.Remove(refreshToken);
                _uow.SaveChanges(false);
            }
        }
    }
}
