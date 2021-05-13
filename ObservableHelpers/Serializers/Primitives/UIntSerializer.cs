using System;
using System.Collections.Generic;
using System.Text;

namespace ObservableHelpers.Serializers.Primitives
{
    public class UIntSerializer : Serializer<uint>
    {
        public override string Serialize(uint value)
        {
            return value.ToString();
        }

        public override uint Deserialize(string data, uint defaultValue = default)
        {
            if (string.IsNullOrEmpty(data)) return defaultValue;
            if (uint.TryParse(data, out uint result)) return result;
            return defaultValue;
        }
    }
}
