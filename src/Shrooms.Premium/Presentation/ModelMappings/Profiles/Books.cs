using AutoMapper;
using Shrooms.Premium.DataTransferObjects.Models.Books;
using Shrooms.Premium.DataTransferObjects.Models.Books.BookDetails;
using Shrooms.Premium.DataTransferObjects.Models.Books.BooksByOffice;
using Shrooms.Premium.DataTransferObjects.Models.LazyPaged;
using Shrooms.Premium.Presentation.WebViewModels.Book;
using Shrooms.Premium.Presentation.WebViewModels.Book.BookDetails;
using Shrooms.Premium.Presentation.WebViewModels.Book.BooksByOffice;

namespace Shrooms.Premium.Presentation.ModelMappings.Profiles
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
