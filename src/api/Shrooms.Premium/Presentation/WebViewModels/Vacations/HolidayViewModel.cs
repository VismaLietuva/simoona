namespace Shrooms.Premium.Presentation.WebViewModels.Vacations
{
    /// <summary>
    /// One holiday, for the client to grey out in its date pickers.
    /// <see cref="Date"/> is a bare "YYYY-MM-DD" string rather than a DateTime for
    /// the reason set out on VacationWireFormat: serialised as an instant it picks
    /// up an offset on each hop and stops being the day it names.
    /// </summary>
    public class HolidayViewModel
    {
        public string Date { get; set; }

        public string Name { get; set; }
    }
}
