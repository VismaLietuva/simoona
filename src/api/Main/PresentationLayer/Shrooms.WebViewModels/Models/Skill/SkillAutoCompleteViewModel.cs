namespace Shrooms.WebViewModels.Models
{
    public class SkillAutoCompleteViewModel : AbstractViewModel
    {
        public string Title { get; set; }

        public string Name
        {
            get { return Title; }
        }
    }
}
