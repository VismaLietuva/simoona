using System.Net;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Shrooms.EmailTemplates.TagHelpers
{
    // The shadcn Button at size lg. Padding sits on the anchor so the whole button is tappable.
    [HtmlTargetElement("email-button", Attributes = "href")]
    public class EmailButtonTagHelper : TagHelper
    {
        public string Href { get; set; }

        public string Align { get; set; } = "left";

        /// <summary>"primary" fills; "outline" is the quieter second action beside it.</summary>
        public string Variant { get; set; } = "primary";

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            var label = (await output.GetChildContentAsync()).GetContent().Trim();
            var outline = Variant == "outline";

            // bgcolor as well as the style: Outlook drops background-color on a cell.
            var background = outline ? EmailDesign.Card : EmailDesign.Primary;
            var foreground = outline ? EmailDesign.Foreground : EmailDesign.PrimaryForeground;
            var border = outline ? $"border:1px solid {EmailDesign.Border};" : string.Empty;
            var padding = outline ? "9px 23px" : "10px 24px";

            output.TagName = "table";
            output.TagMode = TagMode.StartTagAndEndTag;
            output.Attributes.SetAttribute("border", "0");
            output.Attributes.SetAttribute("cellpadding", "0");
            output.Attributes.SetAttribute("cellspacing", "0");
            output.Attributes.SetAttribute("role", "presentation");
            output.Attributes.SetAttribute("align", Align);
            // border-collapse:separate keeps the outline variant round: a collapsed
            // border on the cell wins over its border-radius and squares it off.
            output.Attributes.SetAttribute("style", "margin-top:20px;border-collapse:separate;");
            output.Content.SetHtmlContent(
                $"<tbody><tr><td align=\"center\" bgcolor=\"{background}\" " +
                $"style=\"background-color:{background};{border}border-radius:{EmailDesign.RadiusMd};\">" +
                $"<a href=\"{WebUtility.HtmlEncode(Href)}\" target=\"_blank\" style=\"display:inline-block;padding:{padding};" +
                $"font-family:{EmailDesign.FontSans};font-size:{EmailDesign.SmallSize};" +
                $"line-height:{EmailDesign.SmallLineHeight};font-weight:{EmailDesign.MediumWeight};" +
                $"color:{foreground};text-decoration:none;\">{label}</a>" +
                "</td></tr></tbody>");
        }
    }
}
