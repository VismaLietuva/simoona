using System;
using System.Globalization;
using Newtonsoft.Json;

namespace Shrooms.Presentation.Api.Helpers
{
    public class FormattedDecimalConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(decimal);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var value = serializer.Deserialize<string>(reader);
            value = value.Replace(",", ".");
            var isParsed = decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var parsed);
            if (!isParsed)
            {
                throw new InvalidCastException();
            }

            return parsed;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var isParsed = decimal.TryParse(value.ToString(), NumberStyles.Any, CultureInfo.CurrentCulture, out var parsedDecimal);
            var converted = parsedDecimal.ToString("0.##");
            if (!isParsed)
            {
                throw new InvalidCastException();
            }

            writer.WriteValue(converted);
        }
    }
}