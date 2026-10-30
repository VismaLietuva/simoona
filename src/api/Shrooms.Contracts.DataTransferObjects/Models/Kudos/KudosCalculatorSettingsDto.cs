namespace Shrooms.Contracts.DataTransferObjects.Models.Kudos
{
    public class KudosCalculatorSettingsDto
    {
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
