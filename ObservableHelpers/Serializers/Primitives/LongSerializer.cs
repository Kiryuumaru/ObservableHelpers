using System;
using System.Collections.Generic;
using System.Text;

namespace ObservableHelpers.Serializers.Primitives
{
    public class LongSerializer : Serializer<long>
    {
        public override string Serialize(long value)
        {
            return value.ToString();
        }

        public override long Deserialize(string data, long defaultValue = default)
        {
            if (string.IsNullOrEmpty(data)) return defaultValue;
            if (long.TryParse(data, out long result)) return result;
            return defaultValue;
        }
    }
}
