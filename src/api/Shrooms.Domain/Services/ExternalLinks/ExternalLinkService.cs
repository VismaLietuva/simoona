using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects.Models.ExternalLinks;
using Shrooms.Contracts.Exceptions;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.Domain.Helpers;

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

        public async Task UpdateLinksAsync(AddEditDeleteExternalLinkDto updateLinksDto)
        {
            var timestamp = DateTime.UtcNow;
            await DuplicateValuesValidationAsync(updateLinksDto);

            await UpdateLinksAsync(updateLinksDto, timestamp);
            await DeleteLinksAsync(updateLinksDto, timestamp);
            await CreateNewLinksAsync(updateLinksDto, timestamp);
        }

        private async Task DuplicateValuesValidationAsync(AddEditDeleteExternalLinkDto updateLinksDto)
        {
            var externalLinks = await _externalLinkDbSet
                .Where(x => x.OrganizationId == updateLinksDto.OrganizationId)
                .Select(x => new ExternalLinkDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    Url = x.Url
                })
                .ToListAsync();

            if (updateLinksDto.LinksToCreate.Any(c => externalLinks.Any(l => l.Name == c.Name || l.Url == c.Url)) ||
                updateLinksDto.LinksToUpdate.Any(c => externalLinks.Any(l => (l.Name == c.Name || l.Url == c.Url) && l.Id != c.Id)))
            {
                throw new ValidationException(ErrorCodes.DuplicatesIntolerable, "Provided values must be unique");
            }
        }

        private async Task CreateNewLinksAsync(AddEditDeleteExternalLinkDto updateLinks, DateTime timestamp)
        {
            foreach (var link in updateLinks.LinksToCreate)
            {
                var newLink = new ExternalLink
                {
                    Name = link.Name,
                    Url = link.Url,
                    OrganizationId = updateLinks.OrganizationId,
                    Created = timestamp,
                    CreatedBy = updateLinks.UserId,
                    Modified = timestamp,
                    Type = link.Type,
                    Priority = link.Priority,
                };

                _externalLinkDbSet.Add(newLink);
            }

            await _uow.SaveChangesAsync(false);
        }

        private async Task DeleteLinksAsync(AddEditDeleteExternalLinkDto updateLinks, DateTime timestamp)
        {
            var linksToDelete = await _externalLinkDbSet
                .Where(l =>
                    updateLinks.LinksToDelete.Contains(l.Id) &&
                    l.OrganizationId == updateLinks.OrganizationId)
                .ToListAsync();

            foreach (var link in linksToDelete)
            {
                link.UpdateMetadata(updateLinks.UserId, timestamp);
            }

            await _uow.SaveChangesAsync(false);

            foreach (var link in linksToDelete)
            {
                _externalLinkDbSet.Remove(link);
            }
        }

        private async Task UpdateLinksAsync(AddEditDeleteExternalLinkDto updateLinks, DateTime timestamp)
        {
            var updatedLinksIds = updateLinks.LinksToUpdate.Select(l => l.Id);

            var linksToUpdate = await _externalLinkDbSet
                .Where(l => updatedLinksIds.Contains(l.Id) && l.OrganizationId == updateLinks.OrganizationId)
                .ToListAsync();

            foreach (var updatedLink in updateLinks.LinksToUpdate)
            {
                var link = linksToUpdate.First(l => l.Id == updatedLink.Id);

                link.Name = updatedLink.Name;
                link.Url = updatedLink.Url;
                link.Modified = timestamp;
                link.ModifiedBy = updateLinks.UserId;
                link.Type = updatedLink.Type;
                link.Priority = updatedLink.Priority;
            }
        }
    }
}
