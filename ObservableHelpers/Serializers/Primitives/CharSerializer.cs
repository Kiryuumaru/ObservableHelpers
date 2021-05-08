using System;
using System.Collections.Generic;
using System.Text;
using ObservableHelpers.Models;

namespace ObservableHelpers.Serializers.Primitives
{
    public class CharSerializer : Serializer<char>
    {
        public override string Serialize(char value)
        {
            return value.ToString();
        }

        public override char Deserialize(string data, char defaultValue = default)
        {
            if (string.IsNullOrEmpty(data)) return defaultValue;
            if (char.TryParse(data, out char result)) return result;
            return defaultValue;
        }
    }
}
