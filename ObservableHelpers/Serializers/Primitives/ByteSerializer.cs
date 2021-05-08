using System;
using System.Collections.Generic;
using System.Text;
using ObservableHelpers.Models;

namespace ObservableHelpers.Serializers.Primitives
{
    public class ByteSerializer : Serializer<byte>
    {
        public override string Serialize(byte value)
        {
            return value.ToString();
        }

        public override byte Deserialize(string data, byte defaultValue = default)
        {
            if (string.IsNullOrEmpty(data)) return defaultValue;
            if (byte.TryParse(data, out byte result)) return result;
            return defaultValue;
        }
    }
}
