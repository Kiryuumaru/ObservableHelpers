using System;
using System.Collections.Generic;
using System.Text;
using ObservableHelpers.Models;

namespace ObservableHelpers.Serializers.Primitives
{
    public class DoubleSerializer : Serializer<double>
    {
        public override string Serialize(double value)
        {
            return value.ToString();
        }

        public override double Deserialize(string data, double defaultValue = default)
        {
            if (string.IsNullOrEmpty(data)) return defaultValue;
            if (double.TryParse(data, out double result)) return result;
            return defaultValue;
        }
    }
}
