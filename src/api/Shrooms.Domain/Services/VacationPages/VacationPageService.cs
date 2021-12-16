using System.Data.Entity;
using System.Threading.Tasks;
using AutoMapper;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.DataTransferObjects.VacationPages;
using Shrooms.DataLayer.EntityModels.Models;

namespace Shrooms.Domain.Services.VacationPages
{
    public class VacationPageService : IVacationPageService
    {
        private readonly IUnitOfWork2 _uow;
        private readonly DbSet<VacationPage> _vacationPagesDbSet;
        private readonly IMapper _mapper;

        public VacationPageService(IUnitOfWork2 uow, IMapper mapper)
        {
            _vacationPagesDbSet = uow.GetDbSet<VacationPage>();
            _mapper = mapper;
            _uow = uow;
        }

        public async Task<VacationPageDto> GetVacationPage(int organizationId)
        {
            var vacationPage = await _vacationPagesDbSet.FirstOrDefaultAsync(page => page.OrganizationId == organizationId);

            return vacationPage == null ? null : _mapper.Map<VacationPage, VacationPageDto>(vacationPage);
        }

        public async Task EditVacationPage(UserAndOrganizationDto userAndOrg, VacationPageDto vacationPageDto)
        {
            var vacationPage = await _vacationPagesDbSet.FirstOrDefaultAsync(page => page.OrganizationId == userAndOrg.OrganizationId);

            if (vacationPage == null)
            {
                _vacationPagesDbSet.Add(new VacationPage
                {
                    Content = vacationPageDto.Content,
                    OrganizationId = userAndOrg.OrganizationId,
                });

                await _uow.SaveChangesAsync();
                return;
            }

            vacationPage.Content = vacationPageDto.Content;

            await _uow.SaveChangesAsync();
        }
    }
}