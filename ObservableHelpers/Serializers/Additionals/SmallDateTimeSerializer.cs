using System;
using System.Collections.Generic;
using System.Text;
using ObservableHelpers.Models;

namespace ObservableHelpers.Serializers.Additionals
{
    public class SmallDateTimeSerializer : Serializer<SmallDateTime>
    {
        public override string Serialize(SmallDateTime value)
        {
            var bytes = Utils.ToUnsignedArbitraryBaseSystem((ulong)value.GetCompressedTime(), 64);
            string base64 = "";
            foreach (var num in bytes)
            {
                base64 += Utils.Base64Charset[(int)num];
            }
            return base64;
        }

        public override SmallDateTime Deserialize(string data, SmallDateTime defaultValue = default)
        {
            if (string.IsNullOrEmpty(data)) return defaultValue;

            var indexes = new List<uint>();
            foreach (var num in data)
            {
                var indexOf = Utils.Base64Charset.IndexOf(num);
                if (indexOf == -1) return defaultValue;
                indexes.Add((uint)indexOf);
            }
            var unix = Utils.ToUnsignedNormalBaseSystem(indexes.ToArray(), 64);

            return new SmallDateTime((long)unix);
        }
    }
}
