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
        public Books()
        {
            CreateDtoToViewModelMappings();
            CreateViewModelToDtoMappings();
        }

        private void CreateDtoToViewModelMappings()
        {
            CreateMap<RetrievedBookInfoDto, RetrievedBookForPostViewModel>(MemberList.None);

            CreateMap<RetrievedBookInfoDto, RetrievedBookInfoViewModel>(MemberList.None);
            CreateMap<BooksByOfficeDto, BooksByOfficeViewModel>(MemberList.None);
            CreateMap<BasicBookUserDto, BasicBookUserViewModel>(MemberList.None);
            CreateMap<ILazyPaged<BooksByOfficeDto>, ILazyPaged<BooksByOfficeViewModel>>(MemberList.None);
            CreateMap<BookDetailsDto, BookDetailsViewModel>(MemberList.None);
            CreateMap<BookDetailsLogDto, BookDetailsLogViewModel>(MemberList.None);
            CreateMap<BookDetailsAdministrationDto, BookDetailsAdministrationViewModel>(MemberList.None);
            CreateMap<BookQuantityByOfficeDto, BookQuantityByOfficeViewModel>(MemberList.None);
        }

        private void CreateViewModelToDtoMappings()
        {
            CreateMap<NewBookViewModel, NewBookDto>(MemberList.None);
            CreateMap<NewBookQuantityViewModel, NewBookQuantityDto>(MemberList.None);
            CreateMap<EditBookViewModel, EditBookDto>(MemberList.None);
        }
    }
}
