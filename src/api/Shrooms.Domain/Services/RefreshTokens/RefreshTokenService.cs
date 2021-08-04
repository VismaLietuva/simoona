using System;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects;
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

        public async Task AddNewTokenAsync(RefreshTokenDto tokenDto)
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
            await _uow.SaveChangesAsync(false);
        }

        public async Task<RefreshToken> GetTokenTicketByIdAsync(string id)
        {
            var refreshToken = await _refreshTokensDbSet.FirstOrDefaultAsync(x => x.Id == id);

            return refreshToken;
        }

        public async Task RemoveTokenBySubjectAsync(UserAndOrganizationDto userOrg)
        {
            await RemoveTokenAsync(x =>
                x.Subject == userOrg.UserId &&
                x.OrganizationId == userOrg.OrganizationId);
        }

        public async Task RemoveTokenByIdAsync(string id)
        {
            await RemoveTokenAsync(x => x.Id == id);
        }

        private async Task RemoveTokenAsync(Expression<Func<RefreshToken, bool>> filter)
        {
            var refreshToken = _refreshTokensDbSet.FirstOrDefault(filter);

            if (refreshToken != null)
            {
                _refreshTokensDbSet.Remove(refreshToken);
                await _uow.SaveChangesAsync(false);
            }
        }
    }
}
