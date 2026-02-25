using AutoMapper;
using Shrooms.Contracts.DataTransferObjects.Models.Users;
using Shrooms.Contracts.DataTransferObjects.Users;
using Shrooms.Contracts.ViewModels.User;
using Shrooms.Presentation.WebViewModels.Models;
using Shrooms.Presentation.WebViewModels.Models.User;

namespace Shrooms.Presentation.ModelMappings.Profiles
{
    public class Users : Profile
    {
        public Users()
        {
            CreateDtoToViewModelMappings();
            CreateViewModelToDtoMappings();
        }

        private void CreateDtoToViewModelMappings()
        {
            CreateMap<UserDto, ApplicationUserViewModel>(MemberList.None)
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

            CreateMap<UserDto, UserViewModel>(MemberList.None);
            CreateMap<TimeZoneDto, TimeZoneViewModel>(MemberList.None);
            CreateMap<LanguageDto, LanguageViewModel>(MemberList.None);
            CreateMap<WallNotificationsDto, WallNotificationsViewModel>(MemberList.None);
            CreateMap<LocalizationSettingsDto, LocalizationSettingsViewModel>(MemberList.None);
            CreateMap<UserNotificationsSettingsDto, UserNotificationsSettingsViewModel>(MemberList.None);
            CreateMap<UserAutoCompleteDto, ApplicationUserAutoCompleteViewModel>(MemberList.None);
        }

        private void CreateViewModelToDtoMappings()
        {
            CreateMap<ChangeUserLocalizationSettingsViewModel, ChangeUserLocalizationSettingsDto>(MemberList.None)
                .IgnoreUserOrgDto();

            CreateMap<WallNotificationsViewModel, WallNotificationsDto>(MemberList.None);

            CreateMap<UserNotificationsSettingsViewModel, UserNotificationsSettingsDto>(MemberList.None);
        }
    }
}