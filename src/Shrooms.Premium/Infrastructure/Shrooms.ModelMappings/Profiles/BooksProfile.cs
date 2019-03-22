using AutoMapper;
using Shrooms.DataTransferObjects.Models.Books;
using Shrooms.EntityModels.Models;
using Shrooms.WebViewModels.Models.Book;
using Shrooms.WebViewModels.Models.Book.BookDetails;

namespace Shrooms.Premium.Infrastructure.Shrooms.ModelMappings.Profiles
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
        }
        private void CreateViewModelToDtoMappings()
        {
            CreateMap<BookMobilePostViewModel, BookMobilePostDTO>();
            CreateMap<BookMobileReturnViewModel, BookMobileReturnDTO>();
            CreateMap<BookMobileTakeViewModel, BookTakeDTO>();
            CreateMap<BookMobileTakeSpecificViewModel, BookMobileTakeSpecificDTO>();
            CreateMap<BookMobileGetViewModel, BookMobileGetDTO>();
        }
    }
}
