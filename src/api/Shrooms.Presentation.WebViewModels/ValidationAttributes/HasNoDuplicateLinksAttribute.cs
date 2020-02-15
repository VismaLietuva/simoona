using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Shrooms.Presentation.WebViewModels.Models.ExternalLink;

namespace Shrooms.Presentation.WebViewModels.ValidationAttributes
{
    public class HasNoDuplicateLinksAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            var model = value as List<NewExternalLinkViewModel>;
            if (model == null)
            {
                return true;
            }

            if (model.Count != model.Select(x => x.Name).Distinct().Count())
            {
                return false;
            }

            if (model.Count != model.Select(x => x.Url).Distinct().Count())
            {
                return false;
            }

            return true;
        }

        public override string FormatErrorMessage(string name)
        {
            return "List can not contain duplicate values";
        }
    }
}
