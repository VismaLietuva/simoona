using System.Text;

namespace Shrooms.Premium.Domain.Services.Vacations
{
    /// <summary>
    /// How the payroll report leaves the building: UTF-16LE, BOM first.
    ///
    /// Payroll opens the file in an Excel build whose legacy CSV path ignores a
    /// UTF-8 BOM and falls back to the local ANSI code page, which turns every
    /// Lithuanian diacritic into mojibake. The same path does honour the UTF-16
    /// BOM. Import reads the BOM back, so the round trip still holds.
    /// </summary>
    public static class VacationReportEncoding
    {
        public static readonly Encoding Encoding = new UnicodeEncoding(bigEndian: false, byteOrderMark: true);

        /// <summary>
        /// Encoding.GetBytes drops the preamble no matter how the encoding was
        /// constructed — byteOrderMark: true only changes what GetPreamble
        /// returns. Write the BOM by hand, or the file goes out bare and the
        /// spreadsheet guesses.
        /// </summary>
        public static byte[] ToBytes(string text)
        {
            var preamble = Encoding.GetPreamble();
            var body = Encoding.GetBytes(text ?? string.Empty);
            var content = new byte[preamble.Length + body.Length];

            preamble.CopyTo(content, 0);
            body.CopyTo(content, preamble.Length);

            return content;
        }
    }
}
