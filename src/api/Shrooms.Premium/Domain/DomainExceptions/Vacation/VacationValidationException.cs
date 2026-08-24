using System;
using System.Collections.Generic;

namespace Shrooms.Premium.Domain.DomainExceptions.Vacation
{
    /// <summary>
    /// A broken vacation rule.
    ///
    /// Carries both a human-readable message (from Vacations.resx) and the
    /// stable machine <see cref="Code"/> the client already renders, plus any
    /// values that message interpolates. The client validates before submitting,
    /// so these mostly surface races — someone booking an overlapping period
    /// between opening the form and posting it — and the message has to be as
    /// specific as the one the form would have shown.
    /// </summary>
    public class VacationValidationException : Exception
    {
        public VacationValidationException(int errorCode, string code, string message, IDictionary<string, object> parameters = null)
            : base(message)
        {
            ErrorCode = errorCode;
            Code = code;
            Parameters = parameters;
        }

        public int ErrorCode { get; }

        /// <summary>e.g. "overlap", "startInPast", "reasonTooShort".</summary>
        public string Code { get; }

        public IDictionary<string, object> Parameters { get; }
    }
}
