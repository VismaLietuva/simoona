using AutoMapper;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.DataTransferObjects.Models;
using Shrooms.Contracts.DataTransferObjects.Models.Administration;
using Shrooms.Contracts.DataTransferObjects.Models.Banners;
using Shrooms.Contracts.DataTransferObjects.Models.Events;
using Shrooms.Contracts.DataTransferObjects.Models.Kudos;
using Shrooms.Contracts.DataTransferObjects.Models.Support;
using Shrooms.Contracts.ViewModels;
using Shrooms.Contracts.ViewModels.User;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.DataLayer.EntityModels.Models.Kudos;
using Shrooms.Presentation.ModelMappings.Resolvers;
using Shrooms.Presentation.WebViewModels.Models;
using Shrooms.Presentation.WebViewModels.Models.Banners;
using Shrooms.Presentation.WebViewModels.Models.Certificate;
using Shrooms.Presentation.WebViewModels.Models.ChangeProfile;
using Shrooms.Presentation.WebViewModels.Models.Employees;
using Shrooms.Presentation.WebViewModels.Models.Events;
using Shrooms.Presentation.WebViewModels.Models.Exam;
using Shrooms.Presentation.WebViewModels.Models.PostModels;
using Shrooms.Presentation.WebViewModels.Models.Roles;
using Shrooms.Presentation.WebViewModels.Models.Skill;
using Shrooms.Presentation.WebViewModels.Models.Support;
using Shrooms.Presentation.WebViewModels.Models.User;
using Shrooms.Presentation.WebViewModels.Models.Users.Kudos;

namespace Shrooms.Presentation.ModelMappings.Profiles
{
    public class Other : Profile
    {
        public Other()
        {
            CreateViewModelMappings();
            CreateApplicationUserModelMappings();
            CreateMapViewModelMappings();
            CreateKudosViewModel();
            CreateAdministrationMappings();
            CreateKudosLogDtoMappings();
            CreateWelcomeKudosMappings();
            CreateMiscDtoMappings();
            CreateEventWidgetMappings();
            CreateBannerWidgetMappings();
        }

        /// <summary>
        /// It's normal that you create your mappings here. Consider using specific module profile
        /// </summary>
        private void CreateMiscDtoMappings()
        {
            CreateMap<SupportPostViewModel, SupportDto>(MemberList.None);
        }

        private void CreateKudosLogDtoMappings()
        {
            CreateMap<KudosType, KudosTypeDto>(MemberList.None);

            CreateMap<KudosLog, UserKudosInformationDto>(MemberList.None);
        }

        private void CreateWelcomeKudosMappings()
        {
            CreateMap<WelcomeKudosDto, WelcomeKudosViewModel>(MemberList.None);
            CreateMap<WelcomeKudosViewModel, WelcomeKudosDto>(MemberList.None);
        }

        private void CreateEventWidgetMappings()
        {
            CreateMap<UpcomingEventWidgetDto, UpcomingEventWidgetViewModel>(MemberList.None);
        }

        private void CreateBannerWidgetMappings()
        {
            CreateMap<BannerWidgetDto, BannerWidgetViewModel>(MemberList.None);
        }

        private void CreateAdministrationMappings()
        {
            CreateMap<ApplicationUser, AdministrationUserDto>(MemberList.None)
                .ForMember(dest => dest.HasRoom, opt => opt.MapFrom(new AdministrationUserRoomResolver()))
                .ForMember(dest => dest.JobTitle, opt => opt.MapFrom(u => u.JobPosition.Title));

            CreateMap<Project, AdministrationProjectDto>(MemberList.None);
            CreateMap<Skill, AdministrationSkillDto>(MemberList.None);
        }

        private void CreateKudosViewModel()
        {
            CreateMap<KudosTypeDto, KudosTypeViewModel>(MemberList.None);
            CreateMap<ApplicationUser, UserKudosViewModel>(MemberList.None);
            CreateMap<KudosLog, UserKudosInformationViewModel>(MemberList.None);
            CreateMap<KudosPieChartSliceDto, KudosPieChartSliceViewModel>(MemberList.None);
            CreateMap<KudosType, KudosTypeViewModel>(MemberList.None);
            CreateMap<UserKudosInformationDto, UserKudosInformationViewModel>(MemberList.None);
            CreateMap<KudosLogInputModel, KudosLogInputDto>(MemberList.None);
            CreateMap<UserKudosDto, UserKudosViewModel>(MemberList.None);
        }

        private void CreateApplicationUserModelMappings()
        {
            CreateMap<ApplicationUser, ApplicationUserDto>(MemberList.None);
            CreateMap<ApplicationUserDto, ApplicationUser>(MemberList.None);

            CreateMap<ApplicationUser, ApplicationUserDetailsViewModel>(MemberList.None)
                .ForMember(dest => dest.Roles, opt => opt.Ignore())
                .ForMember(dest => dest.JobTitle, src => src.MapFrom(d => d.JobPosition.Title))
                .ForMember(dest => dest.Projects, src => src.MapFrom(d => d.Projects));

            CreateMap<ApplicationUserMinimalDto, string>(MemberList.None)
                .ConvertUsing(src => src.Id);
            CreateMap<ApplicationUserViewModel, string>(MemberList.None)
                .ConvertUsing(src => src.Id);
            CreateMap<ApplicationUser, string>(MemberList.None)
                .ConvertUsing(src => src.Id);

            CreateMap<ApplicationUser, ChangeProfileInfoViewModel>(MemberList.None)
                .ForMember(dest => dest.JobTitle, opt => opt.MapFrom(u => u.JobPosition.Title));
            CreateMap<ChangeProfileInfoViewModel, ApplicationUser>(MemberList.None)
                .ForMember(dest => dest.Room, src => src.Ignore())
                .ForMember(dest => dest.Certificates, src => src.Ignore())
                .ForMember(dest => dest.Exams, src => src.Ignore())
                .ForMember(dest => dest.QualificationLevel, src => src.Ignore())
                .ForMember(dest => dest.Organization, src => src.Ignore())
                .ForMember(dest => dest.OrganizationId, src => src.Ignore());

            CreateMap<ApplicationUser, ChangeProfileOfficeViewModel>(MemberList.None);
            CreateMap<ChangeProfileOfficeViewModel, ApplicationUser>(MemberList.None)
                .ForMember(dest => dest.Room, src => src.Ignore())
                .ForMember(dest => dest.RoomId, src => src.Ignore())
                .ForMember(dest => dest.Certificates, src => src.Ignore())
                .ForMember(dest => dest.Exams, src => src.Ignore())
                .ForMember(dest => dest.QualificationLevel, src => src.Ignore())
                .ForMember(dest => dest.OrganizationId, src => src.Ignore());

            CreateMap<ApplicationUser, ApplicationUserReturnViewModel>(MemberList.None)
                .ForMember(a => a.FullName, d => d.MapFrom(a => $"{a.FirstName} {a.LastName}"));

            CreateMap<ApplicationUser, ChangeProfileLoginViewModel>(MemberList.None)
                .ForMember(dest => dest.UserName, src => src.MapFrom(e => e.UserName));
            CreateMap<ChangeProfileLoginViewModel, ApplicationUser>(MemberList.None)
                .ForMember(dest => dest.UserName, src => src.MapFrom(e => e.UserName));

            CreateMap<ApplicationUser, ChangeProfileViewModel>(MemberList.None).ReverseMap();

            CreateMap<ApplicationUserViewModel, ApplicationUser>(MemberList.None);

            CreateMap<ApplicationUserPutJobInfoViewModel, ApplicationUser>(MemberList.None)
                .ForMember(dest => dest.Certificates, opt => opt.Ignore());

            CreateMap<RegisterViewModel, ApplicationUser>(MemberList.None);

            CreateMap<ApplicationUser, ApplicationUserProfileViewModel>(MemberList.None);

            CreateMap<ApplicationUser, ApplicationUserPersonalInfoViewModel>(MemberList.None);

            CreateMap<ApplicationUser, ApplicationUserJobInfoViewModel>(MemberList.None)
                .ForMember(dest => dest.Roles, cfg => cfg.Ignore())
                .ForMember(dest => dest.Projects, src => src.MapFrom(u => u.Projects));

            CreateMap<ApplicationUser, ApplicationUserLoginInfoViewModel>(MemberList.None);

            CreateMap<ApplicationUser, ApplicationUserOfficeInfoViewModel>(MemberList.None);

            CreateMap<ApplicationUser, ApplicationUserShroomsInfoViewModel>(MemberList.None);

            CreateMap<ApplicationUser, ManagerMiniViewModel>(MemberList.None);

            CreateMap<ApplicationUser, ApplicationUserAutoCompleteViewModel>(MemberList.None)
                .ForMember(dest => dest.FullName, src => src.MapFrom(x => x.FirstName + " " + x.LastName));

            CreateMap<ApplicationUserPutPersonalInfoViewModel, ApplicationUser>(MemberList.None);

            CreateMap<ApplicationUserPutOfficeInfoViewModel, ApplicationUser>(MemberList.None);

            CreateMap<ApplicationUserShroomsInfoViewModel, ApplicationUser>(MemberList.None);

            CreateMap<ApplicationUser, ApplicationUserMinimalDto>(MemberList.None);
            CreateMap<ApplicationUserMinimalDto, ApplicationUser>(MemberList.None)
                .ForMember(dest => dest.JobPosition, cfg => cfg.Ignore());

            CreateMap<ApplicationUserMinimalDto, ApplicationUserMinimalViewModel>(MemberList.None);
            CreateMap<ApplicationUserMinimalViewModel, ApplicationUserMinimalDto>(MemberList.None);
            CreateMap<ApplicationUser, ApplicationUserMinimalViewModel>(MemberList.None);
        }

        private void CreateViewModelMap<TDbModel, TViewModel, TViewPostModel>()
            where TDbModel : class
            where TViewModel : class
            where TViewPostModel : class
        {
            CreateMap<TViewModel, TDbModel>(MemberList.None);
            CreateMap<TDbModel, TViewModel>(MemberList.None);
            CreateMap<TViewPostModel, TDbModel>(MemberList.None);
            CreateMap<TDbModel, TViewPostModel>(MemberList.None);
        }

        private void CreateViewModelMappings()
        {
            CreateViewModelMap<Organization, OrganizationViewModel, OrganizationPostViewModel>();
            CreateViewModelMap<Address, AddressViewModel, AddressPostViewModel>();
            // AbstractClassifier → AbstractClassifierViewModel is defined below with ForMember; only add the other 3 maps here
            CreateMap<AbstractClassifierViewModel, AbstractClassifier>(MemberList.None)
                .ForMember(dest => dest.OrganizationId, src => src.Ignore());
            CreateMap<AbstractClassifierPostViewModel, AbstractClassifier>(MemberList.None)
                .ForMember(dest => dest.OrganizationId, src => src.Ignore());
            CreateMap<AbstractClassifier, AbstractClassifierPostViewModel>(MemberList.None);

            CreateMap<CertificateViewModel, Certificate>(MemberList.None)
                .ForMember(dest => dest.OrganizationId, src => src.Ignore());
            CreateMap<Certificate, CertificateViewModel>(MemberList.None);
            CreateMap<CertificatePostViewModel, Certificate>(MemberList.None)
                .ForMember(dest => dest.Exams, opt => opt.Ignore());
            CreateMap<CertificatePostViewModel, int>(MemberList.None)
                .ConvertUsing(src => src.Id);
            CreateMap<Certificate, CertificatePostViewModel>(MemberList.None);

            CreateMap<OfficePostViewModel, Office>(MemberList.None)
                .ForMember(dest => dest.Id, src => src.Ignore())
                .ForMember(dest => dest.Floors, src => src.Ignore());
            CreateMap<Office, OfficeViewModel>(MemberList.None);
            CreateMap<Office, OfficePostViewModel>(MemberList.None);

            CreateMap<Office, OfficeMiniViewModel>(MemberList.None);

            CreateMap<FloorPostViewModel, Floor>(MemberList.None)
                .ForMember(dest => dest.Id, src => src.Ignore())
                .ForMember(dest => dest.Office, src => src.Ignore());
            CreateMap<Floor, FloorViewModel>(MemberList.None);
            CreateMap<Floor, FloorPostViewModel>(MemberList.None);

            CreateMap<RoomPostViewModel, Room>(MemberList.None)
                .ForMember(dest => dest.Id, src => src.Ignore())
                .ForMember(dest => dest.ApplicationUsers, src => src.Ignore())
                .ForMember(dest => dest.Floor, src => src.Ignore())
                .ForMember(dest => dest.RoomType, src => src.Ignore());

            CreateMap<Room, RoomViewModel>(MemberList.None)
                .ForMember(dest => dest.Office,
                    opts => opts.MapFrom(src => src.Floor.Office));

            CreateMap<RoomViewModel, Room>(MemberList.None);

            CreateMap<Room, RoomPostViewModel>(MemberList.None);

            CreateMap<Room, RoomMiniViewModel>(MemberList.None);

            CreateMap<ApplicationRole, RoleViewModel>(MemberList.None);

            CreateMap<RoleViewModel, ApplicationRole>(MemberList.None)
                .ForMember(dest => dest.Permissions, src => src.Ignore());

            CreateMap<ApplicationRole, ApplicationRoleMiniViewModel>(MemberList.None);

            CreateMap<ApplicationRole, ApplicationRoleViewModel>(MemberList.None);
            CreateMap<ApplicationRoleViewModel, ApplicationRole>(MemberList.None)
                .ForMember(dest => dest.OrganizationId, src => src.Ignore());

            CreateMap<ApplicationRole, RoleMiniViewModel>(MemberList.None)
                .ForMember(dest => dest.Permissions, src => src.Ignore());
            CreateMap<RoleMiniViewModel, ApplicationRole>(MemberList.None)
                .ForMember(dest => dest.Permissions, src => src.Ignore())
                .ForMember(dest => dest.OrganizationId, src => src.Ignore());

            CreateMap<RoomTypePostViewModel, RoomType>(MemberList.None)
                .ForMember(dest => dest.Id, src => src.Ignore())
                .ForMember(dest => dest.OrganizationId, src => src.Ignore())
                .ForMember(dest => dest.Rooms, src => src.Ignore());
            CreateMap<RoomType, RoomTypeViewModel>(MemberList.None);
            CreateMap<RoomType, RoomTypePostViewModel>(MemberList.None);

            CreateMap<RoomType, RoomTypeMiniViewModel>(MemberList.None);

            CreateMap<ApplicationUser, ApplicationUserViewModel>(MemberList.None)
                .ForMember(dest => dest.Roles, src => src.Ignore())
                .ForMember(dest => dest.JobTitle, src => src.MapFrom(d => d.JobPosition.Title));

            CreateMap<QualificationLevelViewModel, QualificationLevel>(MemberList.None)
                .ForMember(dest => dest.ApplicationUsers, src => src.Ignore());
            CreateMap<QualificationLevel, QualificationLevelViewModel>(MemberList.None);
            CreateMap<QualificationLevel, QualificationLevelMiniViewModel>(MemberList.None);
            CreateMap<QualificationLevel, QualificationLevelAutoCompleteViewModel>(MemberList.None);
            CreateMap<QualificationLevelPostViewModel, QualificationLevel>(MemberList.None)
                .ForMember(dest => dest.Id, src => src.Ignore());

            CreateMap<Room, AbstractViewModel>(MemberList.None);
            CreateMap<ApplicationUser, AbstractViewModel>(MemberList.None);

            CreateMap<AbstractClassifierAbstractViewModel, Language>(MemberList.None)
                .ForMember(dest => dest.Organization, src => src.Ignore())
                .ForMember(dest => dest.Parent, src => src.Ignore())
                .ForMember(dest => dest.Children, src => src.Ignore());

            CreateMap<AbstractClassifier, AbstractClassifierViewModel>(MemberList.None)
                .ForMember(dest => dest.AbstractClassifierType, src => src.MapFrom(c => c.GetType().Name));

            CreateMap<Exam, ExamViewModel>(MemberList.None);
            CreateMap<Exam, int>(MemberList.None)
                .ConvertUsing(src => src.Id);
            CreateMap<Exam, ExamAutoCompleteViewModel>(MemberList.None);
            CreateMap<Exam, ExamMiniViewModel>(MemberList.None);
            CreateMap<ExamPostViewModel, Exam>(MemberList.None)
                .ForMember(dest => dest.Id, src => src.Ignore());
            CreateMap<ExamPostViewModel, int>(MemberList.None)
                .ConvertUsing(src => src.Id);

            CreateMap<WorkingHours, WorkingHoursViewModel>(MemberList.None).ReverseMap();
            CreateMap<WorkingHours, WorkingHourslWithOutLunchViewModel>(MemberList.None);

            CreateMap<Certificate, CertificateAutoCompleteViewModel>(MemberList.None);
            CreateMap<Certificate, CertificateMiniViewModel>(MemberList.None);

            CreateMap<Skill, SkillMiniViewModel>(MemberList.None);
            CreateMap<Skill, SkillAutoCompleteViewModel>(MemberList.None);
            CreateMap<SkillPostViewModel, Skill>(MemberList.None)
                .ForMember(dest => dest.Id, src => src.Ignore());

            CreateMap<SkillMiniViewModel, Skill>(MemberList.None); // used for unit testing only. Never use for anything else!

            CreateMap<Office, OfficeDropdownViewModel>(MemberList.None);
        }

        private void CreateMapViewModelMappings()
        {
            CreateMap<Floor, FloorMiniViewModel>(MemberList.None);
        }
    }
}
