using System;
using System.Collections.Generic;
using System.Text;
using ObservableHelpers.Models;

namespace ObservableHelpers.Serializers.Additionals
{
    public class DateTimeSerializer : Serializer<DateTime>
    {
        public override string Serialize(DateTime value)
        {
            var bytes = Utils.ToUnsignedArbitraryBaseSystem((ulong)value.Ticks, 64);
            string base64 = "";
            foreach (var num in bytes)
            {
                base64 += Utils.Base64Charset[(int)num];
            }
            return base64;
        }

        public override DateTime Deserialize(string data, DateTime defaultValue = default)
        {
            if (string.IsNullOrEmpty(data)) return defaultValue;

            var indexes = new List<uint>();
            foreach (var num in data)
            {
                var indexOf = Utils.Base64Charset.IndexOf(num);
                if (indexOf == -1) return defaultValue;
                indexes.Add((uint)indexOf);
            }
            var ticks = Utils.ToUnsignedNormalBaseSystem(indexes.ToArray(), 64);

            return new DateTime((long)ticks);
        }
    }
}
