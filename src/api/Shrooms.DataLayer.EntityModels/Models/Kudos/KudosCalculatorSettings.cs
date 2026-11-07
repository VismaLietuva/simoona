namespace Shrooms.DataLayer.EntityModels.Models.Kudos
{
    /// <summary>
    /// The Kudos committee's rates for the Kudos calculator. An organization without a row
    /// uses the defaults.
    /// </summary>
    public class KudosCalculatorSettings : BaseModelWithOrg
    {
        public const int MaxUrlLength = 2000;

        public decimal VatPercent { get; set; }

        public decimal TaxPercent { get; set; }

        public decimal TeamMultiplier { get; set; }

        public decimal CreditPercent { get; set; }

        public int TechWearMonths { get; set; }

        public int FurnitureWearMonths { get; set; }

        public int BuyoutMinimum { get; set; }

        public string GuideUrl { get; set; }

        public string WhitelistUrl { get; set; }
    }
}
