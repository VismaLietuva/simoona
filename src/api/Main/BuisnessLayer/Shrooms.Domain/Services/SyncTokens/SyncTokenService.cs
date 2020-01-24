using System.Linq;
using Shrooms.EntityModels.Models;
using Shrooms.Host.Contracts.DAL;

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

        public string GetToken(string name)
        {
            var syncToken = _syncTokenRepository.Get(n => n.Name == name).FirstOrDefault();

            if (syncToken == null)
            {
                return Create(name);
            }

            return syncToken.Token;
        }

        public string Update(string name, string syncToken)
        {
            var syncTokenToUpdate = _syncTokenRepository.Get(n => n.Name == name).FirstOrDefault();

            if (syncTokenToUpdate == null)
            {
                return "No synch token found with provided name";
            }

            syncTokenToUpdate.Token = syncToken;

            _syncTokenRepository.Update(syncTokenToUpdate);
            _unitOfWork.Save();

            return string.Empty;
        }

        public string Create(string name, string syncToken = "")
        {
            if (_syncTokenRepository.Get(n => n.Name == name).Any())
            {
                return "Synch token already exsists with provided name";
            }

            _syncTokenRepository.Insert(new SyncToken { Name = name, Token = syncToken });
            _unitOfWork.Save();

            return string.Empty;
        }
    }
}
