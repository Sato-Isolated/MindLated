using System;
using System.Collections.Generic;
using System.Linq;

namespace Isolated.Services
{
    public static class GeneralUtils
    {
        public static string ToString(this byte[] bytes)
        {
            char[] chars = new char[bytes.Length / sizeof(char)];
            Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
            return new string(chars);
        }

        public static byte[] ToByteArray(this string @string)
        {
            byte[] bytes = new byte[@string.Length * sizeof(char)];
            Buffer.BlockCopy(@string.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }

        public static List<T> Shuffle<T>(List<T> array, out int[] position)
        {
            RandomGen rand = new RandomGen();
            List<KeyValuePair<int, T>> list = new List<KeyValuePair<int, T>>();
            foreach (T s in array)
                list.Add(new KeyValuePair<int, T>(rand.Next(), s));
            var sorted = from item in list
                         orderby item.Key
                         select item;
            T[] result = new T[array.Count];
            int index = 0;
            foreach (KeyValuePair<int, T> pair in sorted)
            {
                result[index] = pair.Value;
                index++;
            }
            List<int> positions = new List<int>();
            for (int i = 0; i < array.Count; i++)
                positions.Add(Array.IndexOf(array.ToArray(), result[i]));
            position = positions.ToArray();
            return result.ToList();
        }
    }
}