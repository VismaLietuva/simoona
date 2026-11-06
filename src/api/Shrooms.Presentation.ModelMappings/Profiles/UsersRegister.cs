using Mapster;
using Shrooms.Contracts.DataTransferObjects.Models.Users;
using Shrooms.Contracts.DataTransferObjects.Users;
using Shrooms.Contracts.ViewModels.User;
using Shrooms.Presentation.WebViewModels.Models;
using Shrooms.Presentation.WebViewModels.Models.User;

namespace Shrooms.Presentation.ModelMappings.Profiles
{
    /// <summary>Mapster translation of the <see cref="Users"/> AutoMapper profile.</summary>
    public class UsersRegister : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            CreateDtoToViewModelMappings(config);
            CreateViewModelToDtoMappings(config);
        }

        private static void CreateDtoToViewModelMappings(TypeAdapterConfig config)
        {
            config.NewConfig<UserDto, ApplicationUserViewModel>()
                .Ignore(
                    x => x.Id,
                    x => x.JobTitle,
                    x => x.Skills,
                    x => x.Bio,
                    x => x.EmploymentDate,
                    x => x.IsAbsent,
                    x => x.IsAdmin,
                    x => x.IsNewUser,
                    x => x.AbsentComment,
                    x => x.RoomId,
                    x => x.Room,
                    x => x.Roles,
                    x => x.HasRoom,
                    x => x.PictureId,
                    x => x.Organization,
                    x => x.OrganizationId,
                    x => x.TotalKudos,
                    x => x.SecurityStamp,
                    x => x.PostedUserPhoto,
                    x => x.QualificationLevel,
                    x => x.QualificationLevelId,
                    x => x.QualificationLevelName,
                    x => x.Map,
                    x => x.Email,
                    x => x.FirstName,
                    x => x.LastName);

            config.NewConfig<UserDto, UserViewModel>();
            config.NewConfig<TimeZoneDto, TimeZoneViewModel>();
            config.NewConfig<LanguageDto, LanguageViewModel>();
            config.NewConfig<WallNotificationsDto, WallNotificationsViewModel>();
            config.NewConfig<LocalizationSettingsDto, LocalizationSettingsViewModel>();
            config.NewConfig<UserNotificationsSettingsDto, UserNotificationsSettingsViewModel>();
            config.NewConfig<UserAutoCompleteDto, ApplicationUserAutoCompleteViewModel>();
        }

        private static void CreateViewModelToDtoMappings(TypeAdapterConfig config)
        {
            config.NewConfig<ChangeUserLocalizationSettingsViewModel, ChangeUserLocalizationSettingsDto>()
                .IgnoreUserOrgDto();

            config.NewConfig<WallNotificationsViewModel, WallNotificationsDto>();

            config.NewConfig<UserNotificationsSettingsViewModel, UserNotificationsSettingsDto>();
        }
    }
}
