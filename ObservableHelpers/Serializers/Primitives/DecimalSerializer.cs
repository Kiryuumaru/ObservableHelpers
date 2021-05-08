using System;
using System.Collections.Generic;
using System.Text;
using ObservableHelpers.Models;

namespace ObservableHelpers.Serializers.Primitives
{
    public class DecimalSerializer : Serializer<decimal>
    {
        public override string Serialize(decimal value)
        {
            return value.ToString();
        }

        public override decimal Deserialize(string data, decimal defaultValue = default)
        {
            if (string.IsNullOrEmpty(data)) return defaultValue;
            if (decimal.TryParse(data, out decimal result)) return result;
            return defaultValue;
        }
    }
}
