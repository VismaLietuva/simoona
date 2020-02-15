namespace Shrooms.Presentation.WebViewModels.Models.User
{
    public class ApplicationUserProfileViewModel : ApplicationUserBaseViewModel
    {
        public override string Id
        {
            get
            {
                return base.Id;
            }

            set
            {
                base.Id = value;
                LoginInfoInfo.Id = value;
                PersonalInfo.Id = value;
                JobInfo.Id = value;
                OfficeInfo.Id = value;
                ShroomsInfo.Id = value;
            }
        }

        public ApplicationUserProfileViewModel()
        {
            LoginInfoInfo = new ApplicationUserLoginInfoViewModel();
            PersonalInfo = new ApplicationUserPersonalInfoViewModel();
            JobInfo = new ApplicationUserJobInfoViewModel();
            OfficeInfo = new ApplicationUserOfficeInfoViewModel();
            ShroomsInfo = new ApplicationUserShroomsInfoViewModel();
        }

        public ApplicationUserPersonalInfoViewModel PersonalInfo { get; set; }

        public ApplicationUserJobInfoViewModel JobInfo { get; set; }

        public ApplicationUserOfficeInfoViewModel OfficeInfo { get; set; }

        public ApplicationUserLoginInfoViewModel LoginInfoInfo { get; set; }

        public ApplicationUserShroomsInfoViewModel ShroomsInfo { get; set; }
    }
}