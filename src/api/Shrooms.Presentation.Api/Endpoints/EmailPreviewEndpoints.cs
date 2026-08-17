using System.Net;
using System.Text;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.Infrastructure.Email;
using Shrooms.EmailTemplates.Seeds;

namespace Shrooms.Presentation.Api.Endpoints
{
    // Developer-only preview of every template. Registered only in the Development environment.
    public static class EmailPreviewEndpoints
    {
        private const string BasePath = "/dev/email-preview";

        public static void MapEmailPreview(this WebApplication app)
        {
            if (!app.Environment.IsDevelopment())
            {
                return;
            }

            app.MapGet(BasePath, () => Results.Content(BuildIndex(), "text/html; charset=utf-8"))
                .AllowAnonymous()
                .ExcludeFromDescription();

            app.MapGet($"{BasePath}/{{**templatePath}}", async (
                string templatePath,
                string send,
                IMailTemplate mailTemplate,
                IMailingService mailingService) =>
            {
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
                .AllowAnonymous()
                .ExcludeFromDescription();
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
