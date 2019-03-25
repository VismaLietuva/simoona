using System.Linq;
using System.Text;

namespace Shrooms.Domain.Helpers
{
    public class LinkBuilder
    {
        public string GenerateLink(params string[] linkElements)
        {
            var sb = new StringBuilder();
            var lastElement = linkElements.Last();
            foreach (var element in linkElements)
            {
                sb.Append(element);
                if (element != lastElement)
                {
                    sb.Append("/");
                }
            }

            return sb.ToString();
         }
    }
}
