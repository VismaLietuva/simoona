using System.Net;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Shrooms.EmailTemplates.TagHelpers
{
    // Label/value pairs used across events, books and service requests.
    [HtmlTargetElement("email-details")]
    public class EmailDetailsTagHelper : TagHelper
    {
        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            var content = (await output.GetChildContentAsync()).GetContent().Trim();

            output.TagName = "table";
            output.TagMode = TagMode.StartTagAndEndTag;
            output.Attributes.SetAttribute("border", "0");
            output.Attributes.SetAttribute("cellpadding", "0");
            output.Attributes.SetAttribute("cellspacing", "0");
            output.Attributes.SetAttribute("role", "presentation");
            output.Attributes.SetAttribute("width", "100%");
            output.Attributes.SetAttribute("style", "margin:12px 0;");
            output.Content.SetHtmlContent($"<tbody>{content}</tbody>");
        }
    }

    [HtmlTargetElement("email-detail", ParentTag = "email-details", Attributes = "label")]
    public class EmailDetailTagHelper : TagHelper
    {
        public string Label { get; set; }

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            var value = (await output.GetChildContentAsync()).GetContent().Trim();

            output.TagName = "tr";
            output.TagMode = TagMode.StartTagAndEndTag;
            output.Content.SetHtmlContent(
                $"<td align=\"left\" width=\"35%\" style=\"background-color:{EmailDesign.Card};padding:6px 12px 6px 0;" +
                $"font-family:{EmailDesign.FontSans};font-size:{EmailDesign.EyebrowSize};" +
                $"font-weight:{EmailDesign.EyebrowWeight};letter-spacing:{EmailDesign.EyebrowTracking};" +
                $"text-transform:uppercase;color:{EmailDesign.MutedForeground};vertical-align:top;" +
                $"line-height:{EmailDesign.SmallLineHeight};\">{WebUtility.HtmlEncode(Label)}</td>" +
                $"<td align=\"left\" style=\"background-color:{EmailDesign.Card};padding:6px 0;font-family:{EmailDesign.FontSans};" +
                $"font-size:{EmailDesign.SmallSize};line-height:{EmailDesign.SmallLineHeight};" +
                $"color:{EmailDesign.Foreground};vertical-align:top;word-break:break-word;" +
                $"overflow-wrap:break-word;\">{value}</td>");
        }
    }
}
