using ObservableHelpers.Serializers;
using System;
using Xunit;

namespace ObservableHelpersTest
{
    public class CustomObj
    {
        public int Prop1 { get; set; }
        public DateTime Prop2 { get; set; }
        public TimeSpan Prop3 { get; set; }

        public override bool Equals(object obj)
        {
            if (obj is CustomObj customObj)
            {
                return
                    Prop1 == customObj.Prop1 &&
                    Prop2 == customObj.Prop2 &&
                    Prop3 == customObj.Prop3;
            }
            else return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    public class CustomObjSerializer : Serializer<CustomObj>
    {
        public override CustomObj Deserialize(string data, CustomObj defaultValue = null)
        {
            var deserialized = Utils.DeserializeString(data);
            if (deserialized.Length != 3) return defaultValue;
            return new CustomObj()
            {
                Prop1 = Deserialize<int>(deserialized[0]),
                Prop2 = Deserialize<DateTime>(deserialized[1]),
                Prop3 = Deserialize<TimeSpan>(deserialized[2]),
            };
        }

        public override string Serialize(CustomObj value)
        {
            return Utils.SerializeString(
                Serialize(value.Prop1),
                Serialize(value.Prop2),
                Serialize(value.Prop3));
        }
    }

    public class SerializersTest
    {
        [Fact]
        public void CustomTest()
        {
            Serializer.Register(new CustomObjSerializer());
            CustomObj test = new CustomObj()
            {
                Prop1 = 99,
                Prop2 = DateTime.UtcNow,
                Prop3 = TimeSpan.FromSeconds(100)
            };
            var serialized = Serializer.Serialize(test);
            var deserialized = Serializer.Deserialize<CustomObj>(serialized);
            Assert.Equal(test, deserialized);
        }

        [Fact]
        public void BoolTest()
        {
            bool test = true;
            var serialized = Serializer.Serialize(test);
            var deserialized = Serializer.Deserialize<bool>(serialized);
            Assert.Equal(test, deserialized);
        }

        [Fact]
        public void ByteTest()
        {
            byte test = 128;
            var serialized = Serializer.Serialize(test);
            var deserialized = Serializer.Deserialize<byte>(serialized);
            Assert.Equal(test, deserialized);
        }

        [Fact]
        public void CharTest()
        {
            char test = 't';
            var serialized = Serializer.Serialize(test);
            var deserialized = Serializer.Deserialize<char>(serialized);
            Assert.Equal(test, deserialized);
        }

        [Fact]
        public void DecimalTest()
        {
            decimal test = 9999999999;
            var serialized = Serializer.Serialize(test);
            var deserialized = Serializer.Deserialize<decimal>(serialized);
            Assert.Equal(test, deserialized);
        }

        [Fact]
        public void DoubleTest()
        {
            double test = 9999;
            var serialized = Serializer.Serialize(test);
            var deserialized = Serializer.Deserialize<double>(serialized);
            Assert.Equal(test, deserialized);
        }

        [Fact]
        public void FloatTest()
        {
            float test = 99999;
            var serialized = Serializer.Serialize(test);
            var deserialized = Serializer.Deserialize<float>(serialized);
            Assert.Equal(test, deserialized);
        }

        [Fact]
        public void IntTest()
        {
            int test = 9;
            var serialized = Serializer.Serialize(test);
            var deserialized = Serializer.Deserialize<int>(serialized);
            Assert.Equal(test, deserialized);
        }

        [Fact]
        public void LongTest()
        {
            long test = 999;
            var serialized = Serializer.Serialize(test);
            var deserialized = Serializer.Deserialize<long>(serialized);
            Assert.Equal(test, deserialized);
        }

        [Fact]
        public void SByteTest()
        {
            sbyte test = 64;
            var serialized = Serializer.Serialize(test);
            var deserialized = Serializer.Deserialize<sbyte>(serialized);
            Assert.Equal(test, deserialized);
        }

        [Fact]
        public void ShortTest()
        {
            short test = 12;
            var serialized = Serializer.Serialize(test);
            var deserialized = Serializer.Deserialize<short>(serialized);
            Assert.Equal(test, deserialized);
        }

        [Fact]
        public void UIntTest()
        {
            uint test = 99;
            var serialized = Serializer.Serialize(test);
            var deserialized = Serializer.Deserialize<uint>(serialized);
            Assert.Equal(test, deserialized);
        }

        [Fact]
        public void ULongTest()
        {
            ulong test = 99;
            var serialized = Serializer.Serialize(test);
            var deserialized = Serializer.Deserialize<ulong>(serialized);
            Assert.Equal(test, deserialized);
        }

        [Fact]
        public void UShortTest()
        {
            ushort test = 99;
            var serialized = Serializer.Serialize(test);
            var deserialized = Serializer.Deserialize<ushort>(serialized);
            Assert.Equal(test, deserialized);
        }

        [Fact]
        public void DateTimeTest()
        {
            DateTime test = DateTime.UtcNow;
            var serialized = Serializer.Serialize(test);
            var deserialized = Serializer.Deserialize<DateTime>(serialized);
            Assert.Equal(test, deserialized);
        }

        [Fact]
        public void StringTest()
        {
            string test = "test";
            var serialized = Serializer.Serialize(test);
            var deserialized = Serializer.Deserialize<string>(serialized);
            Assert.Equal(test, deserialized);
        }

        [Fact]
        public void TimeSpanTest()
        {
            TimeSpan test = TimeSpan.FromDays(265);
            var serialized = Serializer.Serialize(test);
            var deserialized = Serializer.Deserialize<TimeSpan>(serialized);
            Assert.Equal(test, deserialized);
        }
    }
}
