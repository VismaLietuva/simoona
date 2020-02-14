using AutoMapper;
using Shrooms.Contracts.DataTransferObjects.Models.LazyPaged;
using Shrooms.Premium.Main.BusinessLayer.DataTransferObjects.Models.Books;
using Shrooms.Premium.Main.BusinessLayer.DataTransferObjects.Models.Books.BookDetails;
using Shrooms.Premium.Main.BusinessLayer.DataTransferObjects.Models.Books.BooksByOffice;
using Shrooms.Premium.Main.PresentationLayer.WebViewModels.Models.Book;
using Shrooms.Premium.Main.PresentationLayer.WebViewModels.Models.Book.BookDetails;
using Shrooms.Premium.Main.PresentationLayer.WebViewModels.Models.Book.BooksByOffice;

namespace Shrooms.Premium.Infrastructure.ModelMappings.Profiles
{
    public class Books : Profile
    {
        protected override void Configure()
        {
            CreateDtoToViewModelMappings();
            CreateViewModelToDtoMappings();
        }

        private void CreateDtoToViewModelMappings()
        {
            CreateMap<RetrievedBookInfoDTO, RetrievedBookForPostViewModel>();

            CreateMap<RetrievedBookInfoDTO, RetrievedBookInfoViewModel>();
            CreateMap<BooksByOfficeDTO, BooksByOfficeViewModel>();
            CreateMap<BasicBookUserDTO, BasicBookUserViewModel>();
            CreateMap<ILazyPaged<BooksByOfficeDTO>, ILazyPaged<BooksByOfficeViewModel>>();
            CreateMap<BookDetailsDTO, BookDetailsViewModel>();
            CreateMap<BookDetailsLogDTO, BookDetailsLogViewModel>();
            CreateMap<BookDetailsAdministrationDTO, BookDetailsAdministrationViewModel>();
            CreateMap<BookQuantityByOfficeDTO, BookQuantityByOfficeViewModel>();
        }

        private void CreateViewModelToDtoMappings()
        {
            CreateMap<NewBookViewModel, NewBookDTO>();
            CreateMap<NewBookQuantityViewModel, NewBookQuantityDTO>();
            CreateMap<EditBookViewModel, EditBookDTO>();
        }
    }
}
