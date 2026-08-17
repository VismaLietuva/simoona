using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Shrooms.EmailTemplates.TagHelpers
{
    // The shadcn Button at size lg. Padding sits on the anchor so the whole button is tappable.
    [HtmlTargetElement("email-button", Attributes = "href")]
    public class EmailButtonTagHelper : TagHelper
    {
        public string Href { get; set; }

        public string Align { get; set; } = "left";

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            var label = (await output.GetChildContentAsync()).GetContent().Trim();

            output.TagName = "table";
            output.TagMode = TagMode.StartTagAndEndTag;
            output.Attributes.SetAttribute("border", "0");
            output.Attributes.SetAttribute("cellpadding", "0");
            output.Attributes.SetAttribute("cellspacing", "0");
            output.Attributes.SetAttribute("role", "presentation");
            output.Attributes.SetAttribute("align", Align);
            output.Attributes.SetAttribute("style", "margin-top:20px;");
            output.Content.SetHtmlContent(
                $"<tbody><tr><td align=\"center\" bgcolor=\"{EmailDesign.Primary}\" " +
                $"style=\"background-color:{EmailDesign.Primary};border-radius:{EmailDesign.RadiusMd};\">" +
                $"<a href=\"{Href}\" target=\"_blank\" style=\"display:inline-block;padding:10px 24px;" +
                $"font-family:{EmailDesign.FontSans};font-size:{EmailDesign.SmallSize};" +
                $"line-height:{EmailDesign.SmallLineHeight};font-weight:{EmailDesign.MediumWeight};" +
                $"color:{EmailDesign.PrimaryForeground};text-decoration:none;\">{label}</a>" +
                "</td></tr></tbody>");
        }
    }
}
