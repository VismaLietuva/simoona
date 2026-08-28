using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Shrooms.EmailTemplates.TagHelpers
{
    // User-supplied text. Keeps word-break guards - this content overflows most often.
    [HtmlTargetElement("email-quote")]
    public class EmailQuoteTagHelper : TagHelper
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
            output.Content.SetHtmlContent(
                $"<tbody><tr><td style=\"background-color:{EmailDesign.Accent};" +
                $"border-radius:{EmailDesign.RadiusMd};padding:12px 16px;" +
                $"font-family:{EmailDesign.FontSans};font-size:{EmailDesign.SmallSize};" +
                $"line-height:{EmailDesign.SmallLineHeight};color:{EmailDesign.AccentForeground};" +
                $"word-wrap:break-word;word-break:break-word;overflow-wrap:break-word;\">" +
                $"{content}</td></tr></tbody>");
        }
    }
}
