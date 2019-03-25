using System.Web.Script.Serialization;

namespace Shrooms.Resources.Helpers
{
    public static partial class LocalizationHelper
    {
        private static readonly JavaScriptSerializer _javaScriptSerializer = new JavaScriptSerializer();

        public static string ToJson(this object item)
        {
            return _javaScriptSerializer.Serialize(item);
        }
    }
}