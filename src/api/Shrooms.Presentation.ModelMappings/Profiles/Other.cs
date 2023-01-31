using AutoMapper;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.DataTransferObjects.Models;
using Shrooms.Contracts.DataTransferObjects.Models.Administration;
using Shrooms.Contracts.DataTransferObjects.Models.Events;
using Shrooms.Contracts.DataTransferObjects.Models.Kudos;
using Shrooms.Contracts.DataTransferObjects.Models.Support;
using Shrooms.Contracts.ViewModels;
using Shrooms.Contracts.ViewModels.User;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.DataLayer.EntityModels.Models.Kudos;
using Shrooms.Presentation.ModelMappings.Resolvers;
using Shrooms.Presentation.WebViewModels.Models;
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
        protected override void Configure()
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
        }

        /// <summary>
        /// It's normal that you create your mappings here. Consider using specific module profile
        /// </summary>
        private void CreateMiscDtoMappings()
        {
            CreateMap<SupportPostViewModel, SupportDto>();
        }

        private void CreateKudosLogDtoMappings()
        {
            CreateMap<KudosType, KudosTypeDto>();

            CreateMap<KudosLog, UserKudosInformationDto>();
        }

        private void CreateWelcomeKudosMappings()
        {
            CreateMap<WelcomeKudosDto, WelcomeKudosViewModel>();
            CreateMap<WelcomeKudosViewModel, WelcomeKudosDto>();
        }

        private void CreateEventWidgetMappings()
        {
            CreateMap<UpcomingEventWidgetDto, UpcomingEventWidgetViewModel>();
        }

        private void CreateAdministrationMappings()
        {
            CreateMap<ApplicationUser, AdministrationUserDto>()
                .ForMember(dest => dest.HasRoom, opt => opt.ResolveUsing(new AdministrationUserRoomResolver()))
                .ForMember(dest => dest.JobTitle, opt => opt.MapFrom(u => u.JobPosition.Title));

            CreateMap<Project, AdministrationProjectDto>();
            CreateMap<Skill, AdministrationSkillDto>();
        }

        private void CreateKudosViewModel()
        {
            CreateMap<KudosTypeDto, KudosTypeViewModel>();
            CreateMap<ApplicationUser, UserKudosViewModel>();
            CreateMap<KudosLog, UserKudosInformationViewModel>();
            CreateMap<KudosPieChartSliceDto, KudosPieChartSliceViewModel>();
            CreateMap<KudosType, KudosTypeViewModel>();
            CreateMap<UserKudosInformationDto, UserKudosInformationViewModel>();
            CreateMap<KudosLogInputModel, KudosLogInputDto>();
            CreateMap<UserKudosDto, UserKudosViewModel>();
        }

        private void CreateApplicationUserModelMappings()
        {
            CreateMap<ApplicationUser, ApplicationUserDto>();
            CreateMap<ApplicationUserDto, ApplicationUser>();

            CreateMap<ApplicationUser, ApplicationUserDetailsViewModel>()
                .ForMember(dest => dest.Roles, opt => opt.Ignore())
                .ForMember(dest => dest.JobTitle, src => src.MapFrom(d => d.JobPosition.Title))
                .ForMember(dest => dest.Projects, src => src.MapFrom(d => d.Projects));

            CreateMap<ApplicationUserMinimalDto, string>()
                .ConvertUsing(src => src.Id);
            CreateMap<ApplicationUserViewModel, string>()
                .ConvertUsing(src => src.Id);
            CreateMap<ApplicationUser, string>()
                .ConvertUsing(src => src.Id);

            CreateMap<ApplicationUser, ChangeProfileInfoViewModel>()
                .ForMember(dest => dest.JobTitle, opt => opt.MapFrom(u => u.JobPosition.Title));
            CreateMap<ChangeProfileInfoViewModel, ApplicationUser>()
                .ForMember(dest => dest.Room, src => src.Ignore())
                .ForMember(dest => dest.Certificates, src => src.Ignore())
                .ForMember(dest => dest.Exams, src => src.Ignore())
                .ForMember(dest => dest.QualificationLevel, src => src.Ignore())
                .ForMember(dest => dest.Roles, src => src.Ignore())
                .ForMember(dest => dest.Organization, src => src.Ignore())
                .ForMember(dest => dest.OrganizationId, src => src.Ignore());

            CreateMap<ApplicationUser, ChangeProfileOfficeViewModel>();
            CreateMap<ChangeProfileOfficeViewModel, ApplicationUser>()
                .ForMember(dest => dest.Room, src => src.Ignore())
                .ForMember(dest => dest.RoomId, src => src.Ignore())
                .ForMember(dest => dest.Certificates, src => src.Ignore())
                .ForMember(dest => dest.Exams, src => src.Ignore())
                .ForMember(dest => dest.QualificationLevel, src => src.Ignore())
                .ForMember(dest => dest.OrganizationId, src => src.Ignore());

            CreateMap<ApplicationUser, ApplicationUserReturnViewModel>()
                .ForMember(a => a.FullName, d => d.MapFrom(a => $"{a.FirstName} {a.LastName}"));

            CreateMap<ApplicationUser, ChangeProfileLoginViewModel>()
                .ForMember(dest => dest.UserName, src => src.MapFrom(e => e.UserName));
            CreateMap<ChangeProfileLoginViewModel, ApplicationUser>()
                .ForMember(dest => dest.UserName, src => src.MapFrom(e => e.UserName));

            CreateMap<ApplicationUser, ChangeProfileViewModel>().ReverseMap();

            CreateMap<ApplicationUserViewModel, ApplicationUser>();

            CreateMap<ApplicationUserPutJobInfoViewModel, ApplicationUser>()
                .ForMember(dest => dest.Certificates, opt => opt.Ignore());

            CreateMap<RegisterViewModel, ApplicationUser>();

            CreateMap<ApplicationUser, ApplicationUserProfileViewModel>();

            CreateMap<ApplicationUser, ApplicationUserPersonalInfoViewModel>();

            CreateMap<ApplicationUser, ApplicationUserJobInfoViewModel>()
                .ForMember(dest => dest.Roles, cfg => cfg.Ignore())
                .ForMember(dest => dest.Projects, src => src.MapFrom(u => u.Projects));

            CreateMap<ApplicationUser, ApplicationUserLoginInfoViewModel>();

            CreateMap<ApplicationUser, ApplicationUserOfficeInfoViewModel>();

            CreateMap<ApplicationUser, ApplicationUserShroomsInfoViewModel>();

            CreateMap<ApplicationUser, ManagerMiniViewModel>();

            CreateMap<ApplicationUser, ApplicationUserAutoCompleteViewModel>()
                .ForMember(dest => dest.FullName, src => src.MapFrom(x => x.FirstName + " " + x.LastName));

            CreateMap<ApplicationUserPutPersonalInfoViewModel, ApplicationUser>();

            CreateMap<ApplicationUserPutOfficeInfoViewModel, ApplicationUser>();

            CreateMap<ApplicationUserShroomsInfoViewModel, ApplicationUser>();
            CreateMap<ApplicationUser, ApplicationUserShroomsInfoViewModel>();

            CreateMap<ApplicationUser, ApplicationUserMinimalDto>();
            CreateMap<ApplicationUserMinimalDto, ApplicationUser>()
                .ForMember(dest => dest.JobPosition, cfg => cfg.Ignore());

            CreateMap<ApplicationUserMinimalDto, ApplicationUserMinimalViewModel>();
            CreateMap<ApplicationUserMinimalViewModel, ApplicationUserMinimalDto>();
            CreateMap<ApplicationUser, ApplicationUserMinimalViewModel>();
        }

        private void CreateViewModelMap<TDbModel, TViewModel, TViewPostModel>()
            where TDbModel : class
            where TViewModel : class
            where TViewPostModel : class
        {
            CreateMap<TViewModel, TDbModel>();
            CreateMap<TDbModel, TViewModel>();
            CreateMap<TViewPostModel, TDbModel>();
            CreateMap<TDbModel, TViewPostModel>();
        }

        private void CreateViewModelMappings()
        {
            CreateViewModelMap<Organization, OrganizationViewModel, OrganizationPostViewModel>();
            CreateViewModelMap<Address, AddressViewModel, AddressPostViewModel>();
            CreateViewModelMap<AbstractClassifier, AbstractClassifierViewModel, AbstractClassifierPostViewModel>();

            CreateMap<CertificateViewModel, Certificate>();
            CreateMap<Certificate, CertificateViewModel>();
            CreateMap<CertificatePostViewModel, Certificate>()
                .ForMember(dest => dest.Exams, opt => opt.Ignore());
            CreateMap<CertificatePostViewModel, int>()
                .ConvertUsing(src => src.Id);
            CreateMap<Certificate, CertificatePostViewModel>();

            CreateMap<OfficePostViewModel, Office>()
                .ForMember(dest => dest.Floors, src => src.Ignore());
            CreateMap<Office, OfficeViewModel>();
            CreateMap<Office, OfficePostViewModel>();

            CreateMap<Office, OfficeMiniViewModel>();

            CreateMap<FloorPostViewModel, Floor>()
                .ForMember(dest => dest.Office, src => src.Ignore());
            CreateMap<Floor, FloorViewModel>();
            CreateMap<Floor, FloorPostViewModel>();

            CreateMap<RoomPostViewModel, Room>()
                .ForMember(dest => dest.ApplicationUsers, src => src.Ignore())
                .ForMember(dest => dest.Floor, src => src.Ignore())
                .ForMember(dest => dest.RoomType, src => src.Ignore());

            CreateMap<Room, RoomViewModel>()
                .ForMember(dest => dest.Office,
                    opts => opts.MapFrom(src => src.Floor.Office));

            CreateMap<RoomViewModel, Room>();

            CreateMap<Room, RoomPostViewModel>();

            CreateMap<Room, RoomMiniViewModel>();

            CreateMap<ApplicationRole, RoleViewModel>();

            CreateMap<RoleViewModel, ApplicationRole>()
                .ForMember(dest => dest.Users, src => src.Ignore())
                .ForMember(dest => dest.Permissions, src => src.Ignore());

            CreateMap<ApplicationRole, ApplicationRoleMiniViewModel>();

            CreateMap<ApplicationRole, ApplicationRoleViewModel>();
            CreateMap<ApplicationRoleViewModel, ApplicationRole>();

            CreateMap<ApplicationRole, RoleMiniViewModel>()
                .ForMember(dest => dest.Users, src => src.Ignore())
                .ForMember(dest => dest.Permissions, src => src.Ignore());
            CreateMap<RoleMiniViewModel, ApplicationRole>()
                .ForMember(dest => dest.Users, src => src.Ignore())
                .ForMember(dest => dest.Permissions, src => src.Ignore());

            CreateMap<Organization, OrganizationViewModel>();
            CreateMap<OrganizationViewModel, Organization>();

            CreateMap<RoomTypePostViewModel, RoomType>()
                .ForMember(dest => dest.Rooms, src => src.Ignore());
            CreateMap<RoomType, RoomTypeViewModel>();
            CreateMap<RoomType, RoomTypePostViewModel>();

            CreateMap<RoomType, RoomTypeMiniViewModel>();

            CreateMap<ApplicationUser, ApplicationUserViewModel>()
                .ForMember(dest => dest.Roles, src => src.Ignore())
                .ForMember(dest => dest.JobTitle, src => src.MapFrom(d => d.JobPosition.Title));

            CreateMap<QualificationLevelViewModel, QualificationLevel>()
                .ForMember(dest => dest.ApplicationUsers, src => src.Ignore());
            CreateMap<QualificationLevel, QualificationLevelViewModel>();
            CreateMap<QualificationLevel, QualificationLevelMiniViewModel>();
            CreateMap<QualificationLevel, QualificationLevelAutoCompleteViewModel>();
            CreateMap<QualificationLevelPostViewModel, QualificationLevel>();

            CreateMap<Room, AbstractViewModel>();
            CreateMap<ApplicationUser, AbstractViewModel>();

            CreateMap<AbstractClassifierAbstractViewModel, Language>()
                .ForMember(dest => dest.Organization, src => src.Ignore())
                .ForMember(dest => dest.Parent, src => src.Ignore())
                .ForMember(dest => dest.Children, src => src.Ignore());

            CreateMap<AbstractClassifier, AbstractClassifierViewModel>()
                .ForMember(dest => dest.AbstractClassifierType, src => src.MapFrom(c => c.GetType().Name));

            CreateMap<Exam, ExamViewModel>();
            CreateMap<Exam, int>()
                .ConvertUsing(src => src.Id);
            CreateMap<Exam, ExamAutoCompleteViewModel>();
            CreateMap<Exam, ExamMiniViewModel>();
            CreateMap<ExamPostViewModel, Exam>();
            CreateMap<ExamPostViewModel, int>()
                .ConvertUsing(src => src.Id);

            CreateMap<WorkingHours, WorkingHoursViewModel>().ReverseMap();
            CreateMap<WorkingHours, WorkingHourslWithOutLunchViewModel>();

            CreateMap<Certificate, CertificateAutoCompleteViewModel>();
            CreateMap<Certificate, CertificateMiniViewModel>();

            CreateMap<Skill, SkillMiniViewModel>();
            CreateMap<Skill, SkillAutoCompleteViewModel>();
            CreateMap<SkillPostViewModel, Skill>();

            CreateMap<SkillMiniViewModel, Skill>(); // used for unit testing only. Never use for anything else!

            CreateMap<Office, OfficeDropdownViewModel>();
        }

        private void CreateMapViewModelMappings()
        {
            CreateMap<Floor, FloorMiniViewModel>();
        }
    }
}