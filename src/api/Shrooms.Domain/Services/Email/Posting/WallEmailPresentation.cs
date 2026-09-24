using Shrooms.Contracts.Constants;
using Shrooms.Contracts.Enums;
using MultiwallWall = Shrooms.DataLayer.EntityModels.Models.Multiwall.Wall;

namespace Shrooms.Domain.Services.Email.Posting
{
    public static class WallEmailPresentation
    {
        public static string GetPostTitle(MultiwallWall wall)
        {
            switch (wall.Type)
            {
                case WallType.Events:
                    return string.Format(EmailTemplates.EventPostTitle, wall.Name);
                case WallType.Project:
                    return string.Format(EmailTemplates.ProjectPostTitle, wall.Name);
                default:
                    return string.Format(EmailTemplates.DefaultPostTitle, wall.Name);
            }
        }

        public static string GetCommentTitle(MultiwallWall wall)
        {
            switch (wall.Type)
            {
                case WallType.Events:
                    return string.Format(EmailTemplates.EventCommentTitle, wall.Name);
                case WallType.Project:
                    return string.Format(EmailTemplates.ProjectCommentTitle, wall.Name);
                default:
                    return string.Format(EmailTemplates.DefaultCommentTitle, wall.Name);
            }
        }

        public static string GetActionButtonTitle(MultiwallWall wall)
        {
            switch (wall.Type)
            {
                case WallType.Events:
                    return EmailTemplates.EventActionButtonTitle;
                case WallType.Project:
                    return EmailTemplates.ProjectActionButtonTitle;
                default:
                    return EmailTemplates.DefaultActionButtonTitle;
            }
        }

        public static string GetEyebrow(MultiwallWall wall)
        {
            switch (wall.Type)
            {
                case WallType.Events:
                    return EmailTemplates.EventEyebrow;
                case WallType.Project:
                    return EmailTemplates.ProjectEyebrow;
                default:
                    return EmailTemplates.DefaultEyebrow;
            }
        }
    }
}
