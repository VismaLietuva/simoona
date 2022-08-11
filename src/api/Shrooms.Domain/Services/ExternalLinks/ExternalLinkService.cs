using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.DataTransferObjects.Models.ExternalLinks;
using Shrooms.Contracts.Exceptions;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.Domain.Helpers;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace Shrooms.Domain.Services.ExternalLinks
{
    public class ExternalLinkService : IExternalLinkService
    {
        private readonly IDbSet<ExternalLink> _externalLinkDbSet;
        private readonly IUnitOfWork2 _uow;

        public ExternalLinkService(IUnitOfWork2 uow)
        {
            _uow = uow;
            _externalLinkDbSet = uow.GetDbSet<ExternalLink>();
        }

        public async Task<IEnumerable<ExternalLinkDto>> GetAllAsync(int organizationId)
        {
            var externalLinks = await _externalLinkDbSet
                .Where(link => link.OrganizationId == organizationId)
                .Select(link => new ExternalLinkDto
                {
                    Id = link.Id,
                    Name = link.Name,
                    Url = link.Url,
                    Type = link.Type,
                    Priority = link.Priority
                })
                .OrderByDescending(link => link.Priority)
                .ToListAsync();

            return externalLinks;
        }

        public async Task UpdateLinksAsync(ManageExternalLinkDto manageLinksDto)
        {
            var timestamp = DateTime.UtcNow;
            await DuplicateValuesValidationAsync(manageLinksDto);

            await UpdateLinksAsync(manageLinksDto, timestamp);
            await DeleteLinksAsync(manageLinksDto, timestamp);
            await CreateNewLinksAsync(manageLinksDto, timestamp);
        }

        public async Task<ExternalLinkDto> GetAsync(int externalLinkId, UserAndOrganizationDto userOrg)
        {
            var externalLink = await _externalLinkDbSet
                .FirstOrDefaultAsync(link => link.Id == externalLinkId && link.OrganizationId == userOrg.OrganizationId);

            if (externalLink == null)
            {
                return null;
            }

            return new ExternalLinkDto
            {
                Id = externalLink.Id,
                Name = externalLink.Name,
                Url = externalLink.Url,
                Type = externalLink.Type,
                Priority = externalLink.Priority
            };
        }

        private async Task DuplicateValuesValidationAsync(ManageExternalLinkDto manageLinksDto)
        {
            var externalLinks = await _externalLinkDbSet
                .Where(x => x.OrganizationId == manageLinksDto.OrganizationId)
                .Select(x => new ExternalLinkDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    Url = x.Url
                })
                .ToListAsync();

            if (manageLinksDto.LinksToCreate.Any(c => externalLinks.Any(l => l.Name == c.Name || l.Url == c.Url)) ||
                manageLinksDto.LinksToUpdate.Any(c => externalLinks.Any(l => (l.Name == c.Name || l.Url == c.Url) && l.Id != c.Id)))
            {
                throw new ValidationException(ErrorCodes.DuplicatesIntolerable, "Provided values must be unique");
            }
        }

        private async Task CreateNewLinksAsync(ManageExternalLinkDto manageLinks, DateTime timestamp)
        {
            foreach (var link in manageLinks.LinksToCreate)
            {
                var newLink = new ExternalLink
                {
                    Name = link.Name,
                    Url = link.Url,
                    OrganizationId = manageLinks.OrganizationId,
                    Created = timestamp,
                    CreatedBy = manageLinks.UserId,
                    Modified = timestamp,
                    Type = link.Type,
                    Priority = link.Priority
                };

                _externalLinkDbSet.Add(newLink);
            }

            await _uow.SaveChangesAsync(false);
        }

        private async Task DeleteLinksAsync(ManageExternalLinkDto manageLinks, DateTime timestamp)
        {
            var linksToDelete = await _externalLinkDbSet
                .Where(l =>
                    manageLinks.LinksToDelete.Contains(l.Id) &&
                    l.OrganizationId == manageLinks.OrganizationId)
                .ToListAsync();

            foreach (var link in linksToDelete)
            {
                link.UpdateMetadata(manageLinks.UserId, timestamp);
            }

            await _uow.SaveChangesAsync(false);

            foreach (var link in linksToDelete)
            {
                _externalLinkDbSet.Remove(link);
            }
        }

        private async Task UpdateLinksAsync(ManageExternalLinkDto manageLinks, DateTime timestamp)
        {
            var updatedLinksIds = manageLinks.LinksToUpdate.Select(l => l.Id);

            var linksToUpdate = await _externalLinkDbSet
                .Where(l => updatedLinksIds.Contains(l.Id) && l.OrganizationId == manageLinks.OrganizationId)
                .ToListAsync();

            foreach (var updatedLink in manageLinks.LinksToUpdate)
            {
                var link = linksToUpdate.First(l => l.Id == updatedLink.Id);

                link.Name = updatedLink.Name;
                link.Url = updatedLink.Url;
                link.Modified = timestamp;
                link.ModifiedBy = manageLinks.UserId;
                link.Type = updatedLink.Type;
                link.Priority = updatedLink.Priority;
            }
        }
    }
}
