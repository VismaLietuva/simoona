using System;
using System.Globalization;
using Newtonsoft.Json;

namespace Shrooms.API.Helpers
{
    public class FormattedDecimalConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(decimal);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            string value = serializer.Deserialize<string>(reader);
            value = value.Replace(",", ".");
            decimal parsed;
            var isParsed = decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out parsed);
            if (!isParsed)
            {
                throw new InvalidCastException();
            }

            return parsed;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            decimal parsedDecimal;
            var isParsed = decimal.TryParse(value.ToString(), NumberStyles.Any, CultureInfo.CurrentCulture, out parsedDecimal);
            var converted = parsedDecimal.ToString("0.##");
            if (!isParsed)
            {
                throw new InvalidCastException();
            }

            writer.WriteValue(converted);
        }
    }
}