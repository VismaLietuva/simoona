using Shrooms.Contracts.Enums;

namespace Shrooms.Presentation.WebViewModels.Models.Users.Kudos
{
    public class KudosLogTypeViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public decimal Value { get; set; }

        public KudosTypeEnum Type { get; set; }
    }
}
