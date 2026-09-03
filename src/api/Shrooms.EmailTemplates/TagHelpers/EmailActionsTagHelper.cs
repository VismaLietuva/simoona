using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Shrooms.EmailTemplates.TagHelpers
{
    // Two buttons on one line. Outlook ignores flex and inline-block spacing, so
    // the gap is a spacer cell rather than a margin, and each button keeps its
    // own table from EmailButtonTagHelper.
    [HtmlTargetElement("email-actions")]
    public class EmailActionsTagHelper : TagHelper
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
            output.Attributes.SetAttribute("style", "margin-top:20px;");
            output.Content.SetHtmlContent($"<tbody><tr>{content}</tr></tbody>");
        }
    }

    // One cell of the row above. The button inside brings its own top margin, so
    // it is cancelled here to keep the pair on one line.
    [HtmlTargetElement("email-action", ParentTag = "email-actions")]
    public class EmailActionTagHelper : TagHelper
    {
        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            var content = (await output.GetChildContentAsync()).GetContent().Trim();

            output.TagName = "td";
            output.TagMode = TagMode.StartTagAndEndTag;
            output.Attributes.SetAttribute("align", "left");
            output.Attributes.SetAttribute("valign", "top");
            output.Attributes.SetAttribute("style", "padding-right:8px;");
            output.Content.SetHtmlContent(content.Replace("margin-top:20px;", string.Empty));
        }
    }
}
