using System;
using System.Collections.Generic;
using System.Text;
using ObservableHelpers.Models;

namespace ObservableHelpers.Serializers.Primitives
{
    public class ShortSerializer : Serializer<short>
    {
        public override string Serialize(short value)
        {
            return value.ToString();
        }

        public override short Deserialize(string data, short defaultValue = default)
        {
            if (string.IsNullOrEmpty(data)) return defaultValue;
            if (short.TryParse(data, out short result)) return result;
            return defaultValue;
        }
    }
}
