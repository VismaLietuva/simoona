using System.Data.Entity;
using System.Threading.Tasks;
using Shrooms.Contracts.DAL;
using Shrooms.DataLayer.EntityModels.Models;

namespace Shrooms.Domain.Services.SyncTokens
{
    public class SyncTokenService : ISyncTokenService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<SyncToken> _syncTokenRepository;

        public SyncTokenService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _syncTokenRepository = _unitOfWork.GetRepository<SyncToken>();
        }

        public async Task<string> GetTokenAsync(string name)
        {
            var syncToken = await _syncTokenRepository.Get(n => n.Name == name).FirstOrDefaultAsync();

            if (syncToken == null)
            {
                return await CreateAsync(name);
            }

            return syncToken.Token;
        }

        public async Task<string> UpdateAsync(string name, string syncToken)
        {
            var syncTokenToUpdate = await _syncTokenRepository.Get(n => n.Name == name).FirstOrDefaultAsync();

            if (syncTokenToUpdate == null)
            {
                return "No sync token found with provided name";
            }

            syncTokenToUpdate.Token = syncToken;

            _syncTokenRepository.Update(syncTokenToUpdate);
            await _unitOfWork.SaveAsync();

            return string.Empty;
        }

        public async Task<string> CreateAsync(string name, string syncToken = "")
        {
            if (await _syncTokenRepository.Get(n => n.Name == name).AnyAsync())
            {
                return "Synch token already exsists with provided name";
            }

            _syncTokenRepository.Insert(new SyncToken { Name = name, Token = syncToken });
            await _unitOfWork.SaveAsync();

            return string.Empty;
        }
    }
}
