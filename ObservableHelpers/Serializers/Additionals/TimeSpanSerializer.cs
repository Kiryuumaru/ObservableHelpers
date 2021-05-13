using System;
using System.Collections.Generic;
using System.Text;

namespace ObservableHelpers.Serializers.Additionals
{
    public class TimeSpanSerializer : Serializer<TimeSpan>
    {
        public override string Serialize(TimeSpan value)
        {
            return value.TotalHours.ToString();
        }

        public override TimeSpan Deserialize(string data, TimeSpan defaultValue = default)
        {
            if (string.IsNullOrEmpty(data)) return defaultValue;
            if (double.TryParse(data, out double result)) return TimeSpan.FromHours(result);
            return defaultValue;
        }
    }
}
