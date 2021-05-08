using System;
using System.Collections.Generic;
using System.Text;
using ObservableHelpers.Models;

namespace ObservableHelpers.Serializers.Primitives
{
    public class SByteSerializer : Serializer<sbyte>
    {
        public override string Serialize(sbyte value)
        {
            return value.ToString();
        }

        public override sbyte Deserialize(string data, sbyte defaultValue = default)
        {
            if (string.IsNullOrEmpty(data)) return defaultValue;
            if (sbyte.TryParse(data, out sbyte result)) return result;
            return defaultValue;
        }
    }
}
