namespace Shrooms.Contracts.DataTransferObjects.Models.Kudos
{
    public class KudosPieChartSliceDto
    {
        /// <summary>
        /// Display name. Translated to the user's culture for the built-in
        /// types, so it must never be used to match against stored data.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The stored, untranslated kudos type name. Culture-independent, so
        /// this is the value to send back when filtering the kudos log.
        /// </summary>
        public string TypeName { get; set; }

        public decimal Value { get; set; }
    }
}
