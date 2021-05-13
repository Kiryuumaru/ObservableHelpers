using System;
using System.Collections.Generic;
using System.Text;

namespace ObservableHelpers.Serializers.Primitives
{
    public class BoolSerializer : Serializer<bool>
    {
        public override string Serialize(bool value)
        {
            return value ? "1" : "0";
        }

        public override bool Deserialize(string data, bool defaultValue = default)
        {
            if (string.IsNullOrEmpty(data)) return defaultValue;
            return data.Equals("1");
        }
    }
}
