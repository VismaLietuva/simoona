using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using AutoMapper;
using MoreLinq;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.ViewModels;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.Presentation.Api.Filters;
using Shrooms.Presentation.WebViewModels.Models;
using Shrooms.Presentation.WebViewModels.Models.PostModels;
using X.PagedList;

namespace Shrooms.Presentation.Api.Controllers
{
    [Authorize]
    public class OfficeController : AbstractWebApiController<Office, OfficeViewModel, OfficePostViewModel>
    {
        public OfficeController(IMapper mapper, IUnitOfWork unitOfWork)
            : base(mapper, unitOfWork)
        {
        }

        [PermissionAuthorize(Permission = BasicPermissions.Office)]
        public override Task<IEnumerable<OfficeViewModel>> GetAllAsync(int maxResults = 0, string orderBy = null, string includeProperties = null)
        {
            return base.GetAllAsync(maxResults, orderBy, includeProperties);
        }

        [PermissionAuthorize(Permission = BasicPermissions.Office)]
        public async Task<OfficeViewModel> GetDefault()
        {
            var office = _mapper.Map<Office, OfficeViewModel>(await _repository.Get(o => o.IsDefault).FirstOrDefaultAsync());

            if (office == null)
            {
                office = _mapper.Map<Office, OfficeViewModel>(await _repository.Get().FirstOrDefaultAsync());
            }

            return office ?? new OfficeViewModel();
        }

        [PermissionAuthorize(Permission = BasicPermissions.Office)]
        public IEnumerable<OfficeDropdownViewModel> GetAllOfficesForDropdown()
        {
            var offices = _repository.Get().ToList();
            var defaultOffice = offices.FirstOrDefault(x => x.Name == "Vilniaus ofisas" && x.OrganizationId == 2);
            if (defaultOffice != null)
            {
                offices.Remove(defaultOffice);
                offices.Insert(0, defaultOffice);
            }

            var mappedOffices = _mapper.Map<IEnumerable<OfficeDropdownViewModel>>(offices);

            return mappedOffices;
        }

        [PermissionAuthorize(Permission = AdministrationPermissions.Office)]
        public override async Task<PagedViewModel<OfficeViewModel>> GetPaged(string includeProperties = null,
            int page = 1,
            int pageSize = WebApiConstants.DefaultPageSize,
            string sort = null,
            string dir = "",
            string s = "")
        {
            if (sort == "City" || sort == "Country" || sort == "Street" || sort == "Building")
            {
                sort = "Address." + sort;
            }

            if (sort == "StreetBuilding")
            {
                sort = "Address.Street";
            }

            var sortString = string.IsNullOrEmpty(sort) ? null : $"{sort} {dir}";

            IQueryable<Office> offices;
            if (string.IsNullOrEmpty(s))
            {
                offices = _repository.Get(orderBy: sortString, includeProperties: includeProperties);
            }
            else
            {
                offices = _repository.Get(filter: o => o.Address.City.Contains(s) || o.Address.Country.Contains(s)
                                                    || o.Address.Street.Contains(s) || o.Address.Building.Contains(s)
                                                    || o.Name.Contains(s),
                                                  orderBy: sortString, includeProperties: includeProperties);
            }

            var officesViewModel = _mapper.Map<IEnumerable<Office>, IEnumerable<OfficeViewModel>>(offices);

            var officeViewModels = officesViewModel as IList<OfficeViewModel> ?? officesViewModel.ToList();
            officeViewModels.ForEach(o => o.Floors.ForEach(f =>
            {
                o.RoomsCount += f.RoomsCount;
                f.Rooms.ForEach(r =>
                {
                    o.ApplicationUsersCount += r.ApplicationUsersCount;
                });
            }));

            var pagedList = await officeViewModels.ToPagedListAsync(page, pageSize);

            var pagedModel = new PagedViewModel<OfficeViewModel>
            {
                PagedList = pagedList,
                PageCount = pagedList.PageCount,
                ItemCount = pagedList.TotalItemCount,
                PageSize = pageSize
            };

            return pagedModel;
        }

        [PermissionAuthorize(Permission = AdministrationPermissions.Office)]
        public override async Task<HttpResponseMessage> Post([FromBody] OfficePostViewModel model)
        {
            if (model == null)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }

            var office = _mapper.Map<OfficePostViewModel, Office>(model);

            if (office.IsDefault)
            {
                await ResetDefaultOfficeAsync();
            }

            _repository.Insert(office);
            await _unitOfWork.SaveAsync();

            return Request.CreateResponse(HttpStatusCode.Created);
        }

        [PermissionAuthorize(Permission = AdministrationPermissions.Office)]
        public override async Task<HttpResponseMessage> Put([FromBody] OfficePostViewModel viewModel)
        {
            if (viewModel == null)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }

            var model = await _repository.GetByIdAsync(viewModel.Id);
            _mapper.Map(viewModel, model);

            if (model.IsDefault)
            {
                await ResetDefaultOfficeAsync();
                model.IsDefault = true;
            }

            _repository.Update(model);
            await _unitOfWork.SaveAsync();

            return Request.CreateResponse(HttpStatusCode.Created);
        }

        [PermissionAuthorize(Permission = AdministrationPermissions.Office)]
        public override async Task<HttpResponseMessage> Delete(int id)
        {
            var office = await _repository.Get(filter: o => o.Id == id, includeProperties: "Floors,Floors.Rooms,Floors.Rooms.ApplicationUsers").FirstOrDefaultAsync();

            if (office == null)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound);
            }

            office.Floors.ForEach(f =>
            {
                f.Rooms?.ForEach(r =>
                {
                    r.ApplicationUsers?.ForEach(e => e.RoomId = null);
                    r.FloorId = null;
                });
                f.OfficeId = null;
            });

            _repository.Delete(office);
            await _unitOfWork.SaveAsync();

            return Request.CreateResponse(HttpStatusCode.OK);
        }

        private async Task ResetDefaultOfficeAsync()
        {
            await _repository.Get(o => o.IsDefault).ForEachAsync(o => o.IsDefault = false);
        }
    }
}