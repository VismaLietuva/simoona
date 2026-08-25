using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Shrooms.EmailTemplates.TagHelpers
{
    // The shadcn Card flattened for email: rounded-xl, 1px border, shadow-sm, p-6.
    [HtmlTargetElement("email-card")]
    public class EmailCardTagHelper : TagHelper
    {
        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            var content = (await output.GetChildContentAsync()).GetContent().Trim();

            output.TagName = "tr";
            output.TagMode = TagMode.StartTagAndEndTag;
            output.Content.SetHtmlContent(
                "<td align=\"left\" style=\"padding:16px 0 8px;\">" +
                "<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" role=\"presentation\" width=\"100%\" " +
                $"style=\"background-color:{EmailDesign.Card};border:1px solid {EmailDesign.Border};" +
                $"border-radius:{EmailDesign.RadiusXl};box-shadow:{EmailDesign.CardShadow};\">" +
                $"<tbody><tr><td align=\"left\" style=\"padding:{EmailDesign.CardPadding};" +
                $"font-family:{EmailDesign.FontSans};font-size:{EmailDesign.BodySize};" +
                $"line-height:{EmailDesign.BodyLineHeight};color:{EmailDesign.Foreground};" +
                "word-break:break-word;overflow-wrap:break-word;\">" +
                $"{content}</td></tr></tbody></table></td>");
        }
    }
}
