using System.Net;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Shrooms.EmailTemplates.TagHelpers
{
    // Uppercase eyebrow over an extrabold headline. The serif accent is <email-em>, inline.
    [HtmlTargetElement("email-heading")]
    public class EmailHeadingTagHelper : TagHelper
    {
        public string Eyebrow { get; set; }

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            var headline = (await output.GetChildContentAsync()).GetContent().Trim();

            var content = string.Empty;
            if (!string.IsNullOrWhiteSpace(Eyebrow))
            {
                content +=
                    $"<p style=\"margin:0 0 4px;font-family:{EmailDesign.FontSans};" +
                    $"font-size:{EmailDesign.EyebrowSize};font-weight:{EmailDesign.EyebrowWeight};" +
                    $"letter-spacing:{EmailDesign.EyebrowTracking};text-transform:uppercase;" +
                    $"color:{EmailDesign.MutedForeground};\">{WebUtility.HtmlEncode(Eyebrow)}</p>";
            }

            output.TagName = "tr";
            output.TagMode = TagMode.StartTagAndEndTag;
            output.Content.SetHtmlContent(
                "<td align=\"left\" style=\"padding:0 4px;\">" +
                $"{content}" +
                $"<h1 style=\"margin:0;font-family:{EmailDesign.FontSans};font-size:{EmailDesign.HeadlineSize};" +
                $"line-height:{EmailDesign.HeadlineLineHeight};font-weight:{EmailDesign.HeadlineWeight};" +
                $"letter-spacing:{EmailDesign.HeadlineTracking};color:{EmailDesign.Foreground};\">{headline}</h1>" +
                "</td>");
        }
    }
}
