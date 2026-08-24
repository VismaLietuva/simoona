using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Shrooms.EmailTemplates.TagHelpers
{
    // The serif-italic accent, inline so it can land on the word that carries the news rather than
    // on whatever is left at the end of the headline.
    [HtmlTargetElement("email-em")]
    public class EmailEmTagHelper : TagHelper
    {
        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            var content = (await output.GetChildContentAsync()).GetContent().Trim();

            output.TagName = "em";
            output.TagMode = TagMode.StartTagAndEndTag;
            // An IHtmlContent value is written literally; a string would have its quotes encoded,
            // and FontSerif carries them.
            output.Attributes.SetAttribute(new TagHelperAttribute(
                "style",
                new HtmlString($"font-family:{EmailDesign.FontSerif};font-weight:400;font-style:italic;"),
                HtmlAttributeValueStyle.DoubleQuotes));
            output.Content.SetHtmlContent(content);
        }
    }
}
