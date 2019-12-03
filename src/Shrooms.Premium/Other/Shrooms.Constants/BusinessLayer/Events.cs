using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shrooms.Premium.Other.Shrooms.Constants.BusinessLayer
{
    public static class EventConstants
    {
        public enum FoodOptions
        {
            None = 0,
            Required,
            Optional
        }

        public const string WillEatOptionLT = "Valgysiu šiame renginyje";
        public const string WillEatOptionEN = "I will eat at this event";
        public const string WillNotEatOptionLT = "Nevalgysiu šiame renginyje";
        public const string WillNotEatOptionEN = "I will not eat at this event";
    }
}