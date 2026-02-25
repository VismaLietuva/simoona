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
        public BooksProfile()
        {
            CreateModelMappings();
            CreateDtoToViewModelMappings();
            CreateViewModelToDtoMappings();
        }

        private void CreateModelMappings()
        {
            CreateMap<BookMobileDto, Book>(MemberList.None);
            CreateMap<Book, BookMobileDto>(MemberList.None);
            CreateMap<ApplicationUser, MobileUserDto>(MemberList.None);
        }

        private void CreateDtoToViewModelMappings()
        {
            CreateMap<BookMobileGetDto, BookMobileGetViewModel>(MemberList.None);
            CreateMap<OfficeBookDto, OfficeBookViewModel>(MemberList.None);
            CreateMap<MobileUserDto, MobileUserViewModel>(MemberList.None);
            CreateMap<BookMobileLogDto, BookMobileLogViewModel>(MemberList.None);
            CreateMap<RetrievedBookInfoDto, RetrievedMobileBookInfoViewModel>(MemberList.None);
            CreateMap<BookReportDto, BookReportViewModel>(MemberList.None);
        }
        private void CreateViewModelToDtoMappings()
        {
            CreateMap<BookMobilePostViewModel, BookMobilePostDto>(MemberList.None);
            CreateMap<BookMobileReturnViewModel, BookMobileReturnDto>(MemberList.None);
            CreateMap<BookMobileTakeViewModel, BookTakeDto>(MemberList.None);
            CreateMap<BookMobileTakeSpecificViewModel, BookMobileTakeSpecificDto>(MemberList.None);
            CreateMap<BookMobileGetViewModel, BookMobileGetDto>(MemberList.None);
            CreateMap<BookReportViewModel, BookReportDto>(MemberList.None);
        }
    }
}
