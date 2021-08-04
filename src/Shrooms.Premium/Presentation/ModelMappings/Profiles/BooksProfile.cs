using AutoMapper;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.DataLayer.EntityModels.Models.Books;
using Shrooms.Premium.DataTransferObjects.Models.Books;
using Shrooms.Premium.Presentation.WebViewModels.Book;
using Shrooms.Premium.Presentation.WebViewModels.Book.BookDetails;

namespace Shrooms.Premium.Presentation.ModelMappings.Profiles
{
    public class BooksProfile : Profile
    {
        protected override void Configure()
        {
            CreateModelMappings();
            CreateDtoToViewModelMappings();
            CreateViewModelToDtoMappings();
        }

        private void CreateModelMappings()
        {
            CreateMap<BookMobileDto, Book>();
            CreateMap<Book, BookMobileDto>();
            CreateMap<ApplicationUser, MobileUserDto>();
        }

        private void CreateDtoToViewModelMappings()
        {
            CreateMap<BookMobileGetDto, BookMobileGetViewModel>();
            CreateMap<OfficeBookDto, OfficeBookViewModel>();
            CreateMap<MobileUserDto, MobileUserViewModel>();
            CreateMap<BookMobileLogDto, BookMobileLogViewModel>();
            CreateMap<RetrievedBookInfoDto, RetrievedMobileBookInfoViewModel>();
            CreateMap<BookReportDto, BookReportViewModel>();
        }
        private void CreateViewModelToDtoMappings()
        {
            CreateMap<BookMobilePostViewModel, BookMobilePostDto>();
            CreateMap<BookMobileReturnViewModel, BookMobileReturnDto>();
            CreateMap<BookMobileTakeViewModel, BookTakeDto>();
            CreateMap<BookMobileTakeSpecificViewModel, BookMobileTakeSpecificDto>();
            CreateMap<BookMobileGetViewModel, BookMobileGetDto>();
            CreateMap<BookReportViewModel, BookReportDto>();
        }
    }
}
