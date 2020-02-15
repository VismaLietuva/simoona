namespace Shrooms.Presentation.WebViewModels.Models
{
    public class OrganizationNameCheckResultModel
    {
        public static readonly OrganizationNameCheckResultModel Ok = new OrganizationNameCheckResultModel
        {
            IsError = false,
            ErrorMessage = string.Empty
        };

        public static readonly OrganizationNameCheckResultModel Exists = new OrganizationNameCheckResultModel
        {
            IsError = true,
            ErrorMessage = "Organization with same name or short name exists, try other name"
        };

        public static readonly OrganizationNameCheckResultModel NoName = new OrganizationNameCheckResultModel
        {
            IsError = true,
            ErrorMessage = "Organization must have name ant short name"
        };

        public bool IsError { get; set; }

        public string ErrorMessage { get; set; }
    }
}