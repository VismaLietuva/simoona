using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using AutoMapper;
using MoreLinq;
using PagedList;
using Shrooms.API.Filters;
using Shrooms.EntityModels.Models;
using Shrooms.Host.Contracts.Constants;
using Shrooms.Host.Contracts.DAL;
using Shrooms.WebViewModels.Models;
using Shrooms.WebViewModels.Models.PostModels;

namespace Shrooms.API.Controllers
{
    [Authorize]
    public class OfficeController : AbstractWebApiController<Office, OfficeViewModel, OfficePostViewModel>
    {
        public OfficeController(IMapper mapper, IUnitOfWork unitOfWork)
            : base(mapper, unitOfWork)
        {
        }

        [PermissionAuthorize(Permission = BasicPermissions.Office)]
        public override IEnumerable<OfficeViewModel> GetAll(int maxResults = 0, string orderBy = null, string includeProperties = null)
        {
            return base.GetAll(maxResults, orderBy, includeProperties);
        }

        [PermissionAuthorize(Permission = BasicPermissions.Office)]
        public OfficeViewModel GetDefault()
        {
            var office = _mapper.Map<Office, OfficeViewModel>(_repository.Get(o => o.IsDefault).FirstOrDefault());
            if (office == null)
            {
                office = _mapper.Map<Office, OfficeViewModel>(_repository.Get().FirstOrDefault());
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
        public override PagedViewModel<OfficeViewModel> GetPaged(string includeProperties = null, int page = 1, int pageSize = WebApiConstants.DefaultPageSize,
            string sort = null, string dir = "", string s = "")
        {
            if (sort == "City" || sort == "Country" || sort == "Street" || sort == "Building")
            {
                sort = "Address." + sort;
            }

            if (sort == "StreetBuilding")
            {
                sort = "Address.Street";
            }

            string sortString = string.IsNullOrEmpty(sort) ? null : $"{sort} {dir}";

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

            var pagedList = officeViewModels.ToPagedList(page, pageSize);

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
        public override HttpResponseMessage Post([FromBody]OfficePostViewModel model)
        {
            if (model == null)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }

            var office = _mapper.Map<OfficePostViewModel, Office>(model);

            if (office.IsDefault)
            {
                ResetDefaultOffice();
            }

            _repository.Insert(office);
            _unitOfWork.Save();

            return Request.CreateResponse(HttpStatusCode.Created);
        }

        [PermissionAuthorize(Permission = AdministrationPermissions.Office)]
        public override HttpResponseMessage Put([FromBody]OfficePostViewModel viewModel)
        {
            if (viewModel == null)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }

            var model = _repository.GetByID(viewModel.Id);
            _mapper.Map(viewModel, model);

            if (model.IsDefault)
            {
                ResetDefaultOffice();
                model.IsDefault = true; // ResetDefaultOffice() in tests case also resets model.IsDefault
            }

            _repository.Update(model);
            _unitOfWork.Save();

            return Request.CreateResponse(HttpStatusCode.Created);
        }

        [PermissionAuthorize(Permission = AdministrationPermissions.Office)]
        public override HttpResponseMessage Delete(int id)
        {
            var office = _repository.Get(filter: of => of.Id == id, includeProperties: "Floors,Floors.Rooms,Floors.Rooms.ApplicationUsers").FirstOrDefault();

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
            _unitOfWork.Save();
            return Request.CreateResponse(HttpStatusCode.OK);
        }

        private void ResetDefaultOffice()
        {
            _repository.Get(o => o.IsDefault).ForEach(o => o.IsDefault = false);
        }
    }
}