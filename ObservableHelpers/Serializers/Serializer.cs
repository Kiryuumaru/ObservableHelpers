using ObservableHelpers.Serializers.Additionals;
using ObservableHelpers.Serializers.Primitives;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObservableHelpers.Serializers
{
    public class SerializerHolder
    {
        private readonly Func<object, string> serialize;
        private readonly Func<string, object, object> deserialize;

        public SerializerHolder(Func<object, string> serialize, Func<string, object, object> deserialize)
        {
            this.serialize = serialize;
            this.deserialize = deserialize;
        }

        public string Serialize(object value) => serialize(value);
        public object Deserialize(string data, object defaultValue = default) => deserialize(data, defaultValue);
    }

    public class SerializerHolder<T> : SerializerHolder
    {
        public SerializerHolder(Func<T, string> serialize, Func<string, T, T> deserialize)
            : base(
                  new Func<object, string>(obj => serialize((T)obj)),
                  new Func<string, object, object>((data, defaultValue) => deserialize(data, (T)defaultValue)))
        {

        }

        public string Serialize(T value) => base.Serialize(value);
        public T Deserialize(string data, T defaultValue = default) => (T)base.Deserialize(data, defaultValue);
    }

    public abstract class Serializer
    {
        private static readonly List<Serializer> serializers = new List<Serializer>()
        {
            new BoolSerializer(),
            new ByteSerializer(),
            new SByteSerializer(),
            new CharSerializer(),
            new DecimalSerializer(),
            new DoubleSerializer(),
            new FloatSerializer(),
            new IntSerializer(),
            new UIntSerializer(),
            new LongSerializer(),
            new ULongSerializer(),
            new ShortSerializer(),
            new UShortSerializer(),
            new StringSerializer(),
            new DateTimeSerializer(),
            new TimeSpanSerializer()
        };

        public static SerializerHolder GetSerializer(Type type)
        {
            if (type.IsArray)
            {
                var arrayType = type.GetElementType();
                foreach (var conv in serializers)
                {
                    if (conv.Type == arrayType)
                    {
                        return new SerializerHolder(
                            values => conv.SerializeEnumerableObject(values),
                            (data, defaultValue) => conv.DeserializeEnumerableObject(data, defaultValue));
                    }
                }
            }
            else if (typeof(IEnumerable).IsAssignableFrom(type) && type.GetGenericArguments()?.Length == 1)
            {
                var genericType = type.GetGenericArguments()[0];
                foreach (var conv in serializers)
                {
                    if (conv.Type == genericType)
                    {
                        return new SerializerHolder(
                            values => conv.SerializeEnumerableObject(values),
                            (data, defaultValue) => conv.DeserializeEnumerableObject(data, defaultValue));
                    }
                }
            }
            else
            {
                foreach (var conv in serializers)
                {
                    if (conv.Type == type)
                    {
                        return new SerializerHolder(
                            conv.SerializeObject,
                            conv.DeserializeObject);
                    }
                }
            }
            throw new Exception(type.Name + " data type not supported");
        }

        public static SerializerHolder<T> GetSerializer<T>()
        {
            var type = typeof(T);
            if (type.IsArray)
            {
                var arrayType = type.GetElementType();
                foreach (var conv in serializers)
                {
                    if (conv.Type == arrayType)
                    {
                        return new SerializerHolder<T>(
                            values => conv.SerializeEnumerableObject(values),
                            (data, defaultValue) => (T)conv.DeserializeEnumerableObject(data, defaultValue));
                    }
                }
            }
            else if(typeof(IEnumerable).IsAssignableFrom(typeof(T)) && type.GetGenericArguments()?.Length == 1)
            {
                var genericType = type.GetGenericArguments()[0];
                foreach (var conv in serializers)
                {
                    if (conv.Type == genericType)
                    {
                        return new SerializerHolder<T>(
                            values => conv.SerializeEnumerableObject(values),
                            (data, defaultValue) => (T)conv.DeserializeEnumerableObject(data, defaultValue));
                    }
                }
            }
            else
            {
                foreach (var conv in serializers)
                {
                    if (conv.Type == type)
                    {
                        var derivedConv = (Serializer<T>)conv;
                        return new SerializerHolder<T>(
                            derivedConv.Serialize,
                            derivedConv.Deserialize);
                    }
                }
            }
            throw new Exception(typeof(T).Name + " data type not supported");
        }

        public static string Serialize<T>(T value)
        {
            return GetSerializer<T>().Serialize(value);
        }

        public static T Deserialize<T>(string data, T defaultValue = default)
        {
            return GetSerializer<T>().Deserialize(data, defaultValue);
        }

        public static void Register(Serializer serializer)
        {
            serializers.RemoveAll(i => i.Type == serializer.Type);
            serializers.Add(serializer);
        }

        public abstract Type Type { get; }

        public abstract string SerializeObject(object value);

        public abstract object DeserializeObject(string data, object defaultValue = default);

        public abstract string SerializeEnumerableObject(object value);

        public abstract object DeserializeEnumerableObject(string data, object defaultValue = default);
    }

    public abstract class Serializer<T> : Serializer
    {
        #region Properties

        public override Type Type { get => typeof(T); }

        public abstract string Serialize(T value);

        public abstract T Deserialize(string data, T defaultValue = default);

        public string SerializeEnumerable(IEnumerable<T> values)
        {
            if (values == null) return null;
            var count = values.Count();
            var encodedValues = new string[count];
            for (int i = 0; i < count; i++)
            {
                encodedValues[i] = Serialize(values.ElementAt(i));
            }
            return Utils.SerializeString(encodedValues);
        }

        public IEnumerable<T> DeserializeEnumerable(string data, IEnumerable<T> defaultValue = default)
        {
            var encodedValues = Utils.DeserializeString(data);
            if (encodedValues == null) return defaultValue;
            var decodedValues = new T[encodedValues.Length];
            for (int i = 0; i < encodedValues.Length; i++)
            {
                decodedValues[i] = Deserialize(encodedValues[i]);
            }
            return decodedValues;
        }

        public override string SerializeObject(object value)
        {
            return Serialize((T)value);
        }

        public override object DeserializeObject(string data, object defaultValue = default)
        {
            return Deserialize(data, (T)defaultValue);
        }

        public override string SerializeEnumerableObject(object value)
        {
            return SerializeEnumerable((IEnumerable<T>)value);
        }

        public override object DeserializeEnumerableObject(string data, object defaultValue = default)
        {
            return DeserializeEnumerable(data, (IEnumerable<T>)defaultValue);
        }

        #endregion
    }
}
