using CommonMark;
using RazorEngineCore;
using System;
using System.Web;

namespace Shrooms.Infrastructure.Email.Templates 
{
    public class EmailTemplateBase<T> : RazorEngineTemplateBase<T> where T : class
    {
        public Func<string> RenderBodyCallback { get; set; }

        public string Layout { get; set; }

        public string Markdown(string value)
        {
            var settings = CommonMarkSettings.Default;

            settings.AdditionalFeatures |= CommonMarkAdditionalFeatures.StrikethroughTilde;
            settings.RenderSoftLineBreaksAsLineBreaks = true;

            return CommonMarkConverter.Convert(value, settings);
        }

        public string HtmlEncode(object value)
        {
            return HttpUtility.HtmlEncode(value.ToString());
        }

        public string RenderBody()
        {
            return RenderBodyCallback();
        }
    }
}
