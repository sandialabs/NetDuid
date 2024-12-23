using System;
using System.Linq;

namespace NetDuid.Tests
{
    /// <summary>
    ///     A class with static test data
    /// </summary>
    public static class StaticTestData
    {
        /// <summary>
        ///     Given an array of bytes create an optionally delimited big endian string
        /// </summary>
        /// <param name="input">bytes to convert</param>
        /// <param name="delimiter">optional delimiter</param>
        /// <returns>bytes as string</returns>
        public static string BytesAsString(this byte[] input, string delimiter = null)
        {
            return BitConverter.ToString(input).Replace("-", delimiter ?? string.Empty);
        }

        /// <summary>
        ///     Given an array of bytes create an optionally delimited big endian string
        /// </summary>
        /// <param name="input">bytes to convert</param>
        /// <param name="delimiter">optional delimiter</param>
        /// <returns>bytes as string</returns>
        public static string BytesAsStringNoLeadingZero(this byte[] input, string delimiter = null)
        {
            return BytesAsString(input, delimiter).Replace(delimiter + 0, delimiter ?? string.Empty);
        }

        /// <summary>
        ///     Concatenate arrays of bytes
        /// </summary>
        /// <param name="arrays">the arrays to concatenate</param>
        /// <returns>the concatenated array</returns>
        public static byte[] ConcatenateBytes(params byte[][] arrays)
        {
            if (arrays.Length == 0)
            {
                return Array.Empty<byte>();
            }

            var bytes = new byte[arrays.Sum(a => a.Length)];
            var offset = 0;

            foreach (var array in arrays)
            {
                Buffer.BlockCopy(array, 0, bytes, offset, array.Length);
                offset += array.Length;
            }

            return bytes;
        }

        /// <summary>
        ///     Prepend a byte array with one or more bytes
        /// </summary>
        /// <param name="byteArray">the array to prepended</param>
        /// <param name="bytes">the bytes to prepend to the array</param>
        /// <returns>the prepended array</returns>
        public static byte[] PrependBytes(this byte[] byteArray, params byte[] bytes)
        {
            if (bytes.Length == 0)
            {
                return byteArray;
            }

            if (byteArray.Length == 0)
            {
                return bytes;
            }

            return ConcatenateBytes(bytes, byteArray);
        }

        /// <summary>
        ///     Generate an array of bytes
        /// </summary>
        /// <param name="length">intended length</param>
        /// <param name="seed">specify seed for testing constancy</param>
        /// <returns>an array of bytes</returns>
        public static byte[] GenerateBytes(int length, int seed = 42)
        {
            var random = new Random(seed);
            var bytes = new byte[length]; // convert kb to byte
            random.NextBytes(bytes);
            return bytes;
        }
    }
}
