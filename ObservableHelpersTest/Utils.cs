using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObservableHelpersTest
{
    internal static class Utils
    {
        internal const string NullIdentifier = "-";
        internal const string EmptyIdentifier = "_";

        internal const string Base64Charset = "-0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ_abcdefghijklmnopqrstuvwxyz";
        internal const string Base62Charset = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
        internal const string Base38Charset = "-0123456789_abcdefghijklmnopqrstuvwxyz";
        internal const string Base36Charset = "0123456789abcdefghijklmnopqrstuvwxyz";
        internal const string Base32Charset = "2345678abcdefghijklmnpqrstuvwxyz"; // Excluded 0, 1, 9, o

        internal static string ToBase62(int number)
        {
            var arbitraryBase = ToUnsignedArbitraryBaseSystem((ulong)number, 62);
            string base62 = "";
            foreach (var num in arbitraryBase)
            {
                base62 += Base62Charset[(int)num];
            }
            return base62;
        }

        internal static int FromBase62(string number)
        {
            var indexes = new List<uint>();
            foreach (var num in number)
            {
                var indexOf = Base62Charset.IndexOf(num);
                if (indexOf == -1) throw new Exception("Unknown charset");
                indexes.Add((uint)indexOf);
            }
            return (int)ToUnsignedNormalBaseSystem(indexes.ToArray(), 62);
        }

        internal static string ToBase64(int number)
        {
            var arbitraryBase = ToUnsignedArbitraryBaseSystem((ulong)number, 64);
            string base64 = "";
            foreach (var num in arbitraryBase)
            {
                base64 += Base64Charset[(int)num];
            }
            return base64;
        }

        internal static int FromBase64(string number)
        {
            var indexes = new List<uint>();
            foreach (var num in number)
            {
                var indexOf = Base64Charset.IndexOf(num);
                if (indexOf == -1) throw new Exception("Unknown charset");
                indexes.Add((uint)indexOf);
            }
            return (int)ToUnsignedNormalBaseSystem(indexes.ToArray(), 64);
        }

        internal static string SerializeString(params string[] datas)
        {
            if (datas == null) return NullIdentifier;
            if (datas.Length == 0) return EmptyIdentifier;
            var dataLength = ToBase62(datas.Length);
            var lengths = datas.Select(i => i == null ? NullIdentifier : (string.IsNullOrEmpty(i) ? EmptyIdentifier : ToBase62(i.Length))).ToArray();
            int maxDigitLength = Math.Max(lengths.Max(i => i.Length), dataLength.Length);
            var maxDigitLength62 = ToBase62(maxDigitLength); ;
            for (int i = 0; i < datas.Length; i++)
            {
                lengths[i] = lengths[i].PadLeft(maxDigitLength, Base62Charset[0]);
            }
            var lengthsAndDatas = new string[lengths.Length + datas.Length];
            Array.Copy(lengths, lengthsAndDatas, lengths.Length);
            Array.Copy(datas, 0, lengthsAndDatas, lengths.Length, datas.Length);
            var joinedLengthsAndDatas = string.Join("", lengthsAndDatas);
            string serialized = string.Join("", maxDigitLength62, dataLength.PadLeft(maxDigitLength, Base62Charset[0]));
            var joinedArr = new string[] { serialized, joinedLengthsAndDatas };
            return string.Join("", joinedArr);
        }

        internal static string[] DeserializeString(string data)
        {
            if (string.IsNullOrEmpty(data)) return null;
            if (data.Equals(NullIdentifier)) return null;
            if (data.Equals(EmptyIdentifier)) return Array.Empty<string>();
            if (data.Length < 4) return new string[] { "" };
            var d = (string)data.Clone();

            int indexDigits = FromBase62(d[0].ToString());
            int indexCount = FromBase62(d.Substring(1, indexDigits));
            var indices = d.Substring(1 + indexDigits, indexDigits * indexCount);
            var dataPart = d.Substring(1 + indexDigits + (indexDigits * indexCount));
            string[] datas = new string[indexCount];
            var currIndex = 0;
            for (int i = 0; i < indexCount; i++)
            {
                var subData = indices.Substring(indexDigits * i, indexDigits).TrimStart(Base62Charset[0]);
                if (subData.Equals(NullIdentifier)) datas[i] = null;
                else if (subData.Equals(EmptyIdentifier)) datas[i] = "";
                else
                {
                    var currLength = FromBase62(subData);
                    datas[i] = dataPart.Substring(currIndex, currLength);
                    currIndex += currLength;
                }
            }
            return datas;
        }

        internal static uint[] ToUnsignedArbitraryBaseSystem(ulong number, uint baseSystem)
        {
            if (baseSystem < 2) throw new Exception("Base below 1 error");
            var baseArr = new List<uint>();
            while (number >= baseSystem)
            {
                var ans = number / baseSystem;
                var remainder = number % baseSystem;
                number = ans;
                baseArr.Add((uint)remainder);
            }
            baseArr.Add((uint)number);
            baseArr.Reverse();
            return baseArr.ToArray();
        }

        internal static ulong ToUnsignedNormalBaseSystem(uint[] arbitraryBaseNumber, uint baseSystem)
        {
            if (baseSystem < 2) throw new Exception("Base below 1 error");
            if (arbitraryBaseNumber.Any(i => i >= baseSystem)) throw new Exception("Number has greater value than base number system");
            ulong value = 0;
            var reverse = arbitraryBaseNumber.Reverse().ToArray();
            for (int i = 0; i < arbitraryBaseNumber.Length; i++)
            {
                value += (ulong)(reverse[i] * Math.Pow(baseSystem, i));
            }
            return value;
        }

        internal static uint[] ToSignedArbitraryBaseSystem(long number, uint baseSystem)
        {
            var num = ToUnsignedArbitraryBaseSystem((ulong)Math.Abs(number), baseSystem);
            var newNum = new uint[num.Length + 1];
            Array.Copy(num, 0, newNum, 1, num.Length);
            newNum[0] = number < 0 ? baseSystem - 1 : 0;
            return newNum;
        }

        internal static long ToSignedNormalBaseSystem(uint[] arbitraryBaseNumber, uint baseSystem)
        {
            bool isNegative;
            if (arbitraryBaseNumber[0] == 0) isNegative = false;
            else if (arbitraryBaseNumber[0] == baseSystem - 1) isNegative = true;
            else throw new Exception("Not a signed number");
            var num = (long)ToUnsignedNormalBaseSystem(arbitraryBaseNumber.Skip(1).ToArray(), baseSystem);
            return isNegative ? -num : num;
        }
    }
}
