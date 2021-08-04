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
            CreateMap<RetrievedBookInfoDto, RetrievedBookForPostViewModel>();

            CreateMap<RetrievedBookInfoDto, RetrievedBookInfoViewModel>();
            CreateMap<BooksByOfficeDto, BooksByOfficeViewModel>();
            CreateMap<BasicBookUserDto, BasicBookUserViewModel>();
            CreateMap<ILazyPaged<BooksByOfficeDto>, ILazyPaged<BooksByOfficeViewModel>>();
            CreateMap<BookDetailsDto, BookDetailsViewModel>();
            CreateMap<BookDetailsLogDto, BookDetailsLogViewModel>();
            CreateMap<BookDetailsAdministrationDto, BookDetailsAdministrationViewModel>();
            CreateMap<BookQuantityByOfficeDto, BookQuantityByOfficeViewModel>();
        }

        private void CreateViewModelToDtoMappings()
        {
            CreateMap<NewBookViewModel, NewBookDto>();
            CreateMap<NewBookQuantityViewModel, NewBookQuantityDto>();
            CreateMap<EditBookViewModel, EditBookDto>();
        }
    }
}
