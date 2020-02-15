namespace Shrooms.Presentation.WebViewModels.Models.ChangeProfile
{
    public class ChangeProfileViewModel : ChangeProfileBaseModel
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
                Info.Id = value;

                Office.Id = value;
                Login.Id = value;
            }
        }

        public ChangeProfileViewModel()
        {
            Info = new ChangeProfileInfoViewModel();
            Office = new ChangeProfileOfficeViewModel();
            Login = new ChangeProfileLoginViewModel();
        }

        public ChangeProfileInfoViewModel Info { get; set; }

        public ChangeProfileOfficeViewModel Office { get; set; }

        public ChangeProfileLoginViewModel Login { get; set; }
    }
}