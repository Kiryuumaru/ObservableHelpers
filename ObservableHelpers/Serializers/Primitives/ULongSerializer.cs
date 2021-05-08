using System;
using System.Collections.Generic;
using System.Text;
using ObservableHelpers.Models;

namespace ObservableHelpers.Serializers.Primitives
{
    public class ULongSerializer : Serializer<ulong>
    {
        public override string Serialize(ulong value)
        {
            return value.ToString();
        }

        public override ulong Deserialize(string data, ulong defaultValue = default)
        {
            if (string.IsNullOrEmpty(data)) return defaultValue;
            if (ulong.TryParse(data, out ulong result)) return result;
            return defaultValue;
        }
    }
}
