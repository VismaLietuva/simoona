using AutoMapper;
using Shrooms.Contracts.DataTransferObjects.Models.Users;
using Shrooms.Contracts.DataTransferObjects.Users;
using Shrooms.Contracts.DataTransferObjects.Wall.Likes;
using Shrooms.Contracts.ViewModels.User;
using Shrooms.Presentation.WebViewModels.Models;
using Shrooms.Presentation.WebViewModels.Models.User;

namespace Shrooms.Presentation.ModelMappings.Profiles
{
    public class Users : Profile
    {
        protected override void Configure()
        {
            CreateDtoToViewModelMappings();
            CreateViewModelToDtoMappings();
        }

        private void CreateDtoToViewModelMappings()
        {
            CreateMap<UserDto, ApplicationUserViewModel>()
                .Ignore(x => x.Id)
                .Ignore(x => x.JobTitle)
                .Ignore(x => x.Skills)
                .Ignore(x => x.Bio)
                .Ignore(x => x.EmploymentDate)
                .Ignore(x => x.IsAbsent)
                .Ignore(x => x.IsAdmin)
                .Ignore(x => x.IsNewUser)
                .Ignore(x => x.AbsentComment)
                .Ignore(x => x.RoomId)
                .Ignore(x => x.Room)
                .Ignore(x => x.Roles)
                .Ignore(x => x.HasRoom)
                .Ignore(x => x.PictureId)
                .Ignore(x => x.Organization)
                .Ignore(x => x.OrganizationId)
                .Ignore(x => x.TotalKudos)
                .Ignore(x => x.SecurityStamp)
                .Ignore(x => x.PostedUserPhoto)
                .Ignore(x => x.QualificationLevel)
                .Ignore(x => x.QualificationLevelId)
                .Ignore(x => x.QualificationLevelName)
                .Ignore(x => x.Map)
                .Ignore(x => x.Email)
                .Ignore(x => x.FirstName)
                .Ignore(x => x.LastName);

            CreateMap<LikeDto, UserViewModel>();
            CreateMap<UserDto, UserViewModel>();
            CreateMap<TimeZoneDto, TimeZoneViewModel>();
            CreateMap<LanguageDto, LanguageViewModel>();
            CreateMap<WallNotificationsDto, WallNotificationsViewModel>();
            CreateMap<LocalizationSettingsDto, LocalizationSettingsViewModel>();
            CreateMap<UserNotificationsSettingsDto, UserNotificationsSettingsViewModel>();
            CreateMap<UserAutoCompleteDto, ApplicationUserAutoCompleteViewModel>();
        }

        private void CreateViewModelToDtoMappings()
        {
            CreateMap<ChangeUserLocalizationSettingsViewModel, ChangeUserLocalizationSettingsDto>()
                .IgnoreUserOrgDto();

            CreateMap<WallNotificationsViewModel, WallNotificationsDto>();

            CreateMap<UserNotificationsSettingsViewModel, UserNotificationsSettingsDto>();
        }
    }
}