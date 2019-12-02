using AutoMapper;
using Shrooms.DataTransferObjects.Models.Books;
using Shrooms.DataTransferObjects.Models.Books.BookDetails;
using Shrooms.DataTransferObjects.Models.Books.BooksByOffice;
using Shrooms.DataTransferObjects.Models.LazyPaged;
using Shrooms.WebViewModels.Models.Book;
using Shrooms.WebViewModels.Models.Book.BookDetails;
using Shrooms.WebViewModels.Models.Book.BooksByOffice;

namespace Shrooms.ModelMappings.Profiles
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
