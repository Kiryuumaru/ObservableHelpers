using System;
using System.Collections.Generic;
using System.Text;

namespace ObservableHelpers.Serializers.Primitives
{
    public class FloatSerializer : Serializer<float>
    {
        public override string Serialize(float value)
        {
            return value.ToString();
        }

        public override float Deserialize(string data, float defaultValue = default)
        {
            if (string.IsNullOrEmpty(data)) return defaultValue;
            if (float.TryParse(data, out float result)) return result;
            return defaultValue;
        }
    }
}
