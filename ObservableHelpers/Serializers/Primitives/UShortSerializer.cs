using System;
using System.Collections.Generic;
using System.Text;

namespace ObservableHelpers.Serializers.Primitives
{
    public class UShortSerializer : Serializer<ushort>
    {
        public override string Serialize(ushort value)
        {
            return value.ToString();
        }

        public override ushort Deserialize(string data, ushort defaultValue = default)
        {
            if (string.IsNullOrEmpty(data)) return defaultValue;
            if (ushort.TryParse(data, out ushort result)) return result;
            return defaultValue;
        }
    }
}
