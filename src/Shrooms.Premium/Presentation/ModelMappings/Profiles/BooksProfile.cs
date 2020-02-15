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
            CreateMap<BookMobileDTO, Book>();
            CreateMap<Book, BookMobileDTO>();
            CreateMap<ApplicationUser, MobileUserDTO>();
        }

        private void CreateDtoToViewModelMappings()
        {
            CreateMap<BookMobileGetDTO, BookMobileGetViewModel>();
            CreateMap<OfficeBookDTO, OfficeBookViewModel>();
            CreateMap<MobileUserDTO, MobileUserViewModel>();
            CreateMap<BookMobileLogDTO, BookMobileLogViewModel>();
            CreateMap<RetrievedBookInfoDTO, RetrievedMobileBookInfoViewModel>();
            CreateMap<BookReportDTO, BookReportViewModel>();
        }
        private void CreateViewModelToDtoMappings()
        {
            CreateMap<BookMobilePostViewModel, BookMobilePostDTO>();
            CreateMap<BookMobileReturnViewModel, BookMobileReturnDTO>();
            CreateMap<BookMobileTakeViewModel, BookTakeDTO>();
            CreateMap<BookMobileTakeSpecificViewModel, BookMobileTakeSpecificDTO>();
            CreateMap<BookMobileGetViewModel, BookMobileGetDTO>();
            CreateMap<BookReportViewModel, BookReportDTO>();
        }
    }
}
