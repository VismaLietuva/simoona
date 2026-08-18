using System.Net;
using System.Security.Claims;
using System.Text;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.Infrastructure.Email;
using Shrooms.Domain.Services.Permissions;
using Shrooms.EmailTemplates.Seeds;
using Shrooms.Presentation.Common.Helpers;

namespace Shrooms.Presentation.Api.Endpoints
{
    // Preview of every template, rendered against the shared seed models. Open to organization administrators.
    public static class EmailPreviewEndpoints
    {
        private const string BasePath = "/email-preview";

        public static void MapEmailPreview(this WebApplication app)
        {
            app.MapGet(BasePath, async (ClaimsPrincipal user, IPermissionService permissionService) =>
            {
                if (!await IsPermittedAsync(user, permissionService))
                {
                    return MissingPermission();
                }

                return Results.Content(BuildIndex(), "text/html; charset=utf-8");
            })
                .RequireAuthorization()
                .ExcludeFromDescription();

            app.MapGet($"{BasePath}/{{**templatePath}}", async (
                string templatePath,
                string send,
                ClaimsPrincipal user,
                IPermissionService permissionService,
                IMailTemplate mailTemplate,
                IMailingService mailingService) =>
            {
                if (!await IsPermittedAsync(user, permissionService))
                {
                    return MissingPermission();
                }

                var key = "/EmailTemplates/" + templatePath;
                var seed = EmailTemplateSeeds.All.FirstOrDefault(candidate => candidate.Key == key);
                if (seed is null)
                {
                    return Results.NotFound($"No seeded template for '{key}'. See {BasePath} for the full list.");
                }

                var html = await mailTemplate.GenerateAsync(seed.Model, seed.Key);

                if (string.IsNullOrWhiteSpace(send))
                {
                    return Results.Content(html, "text/html; charset=utf-8");
                }

                await mailingService.SendEmailAsync(new EmailDto(send, $"[Preview] {templatePath}", html), skipDomainChange: true);
                return Results.Text($"Sent {templatePath} to {send}.");
            })
                .RequireAuthorization()
                .ExcludeFromDescription();
        }

        // Opened straight in a browser tab, so answer refusals in kind rather than with an empty 403.
        private static IResult MissingPermission()
        {
            return Results.Text("Missing permission", "text/plain; charset=utf-8", null, StatusCodes.Status403Forbidden);
        }

        private static async Task<bool> IsPermittedAsync(ClaimsPrincipal user, IPermissionService permissionService)
        {
            return await permissionService.UserHasPermissionAsync(
                user.Identity.GetUserAndOrganization(),
                AdministrationPermissions.Organization);
        }

        private static string BuildIndex()
        {
            var builder = new StringBuilder();
            builder.Append("<!doctype html><meta charset=\"utf-8\"><title>Email previews</title>");
            builder.Append("<style>body{font:15px/1.6 system-ui,sans-serif;margin:2rem auto;max-width:44rem;padding:0 1rem}");
            builder.Append("h1{font-size:1.4rem}h2{font-size:.85rem;text-transform:uppercase;letter-spacing:.06em;color:#666;margin:1.6rem 0 .4rem}");
            builder.Append("ul{list-style:none;padding:0;margin:0}li{padding:.15rem 0}a{color:#006199}p{color:#666}</style>");
            builder.Append($"<h1>Email previews</h1><p>{EmailTemplateSeeds.All.Count} templates, rendered against the shared seed models. ");
            builder.Append("Append <code>?send=you@example.com</code> to mail one to yourself.</p>");

            var groups = EmailTemplateSeeds.All
                .Select(seed => seed.Key["/EmailTemplates/".Length..])
                .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
                .GroupBy(path => path.Contains('/') ? path[..path.IndexOf('/')] : "General");

            foreach (var group in groups.OrderBy(group => group.Key, StringComparer.OrdinalIgnoreCase))
            {
                builder.Append($"<h2>{WebUtility.HtmlEncode(group.Key)}</h2><ul>");
                foreach (var path in group)
                {
                    var name = Path.GetFileNameWithoutExtension(path);
                    builder.Append($"<li><a href=\"{BasePath}/{path}\">{WebUtility.HtmlEncode(name)}</a></li>");
                }

                builder.Append("</ul>");
            }

            return builder.ToString();
        }
    }
}
