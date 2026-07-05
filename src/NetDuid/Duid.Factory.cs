using System.Globalization;

namespace NetDuid
{
    /// <content>
    ///     <see cref="Duid"/> static factory methods
    /// </content>
    public sealed partial class Duid
    {
        #region factory methods

        /// <summary>
        ///     Attempt to parse the given <paramref name="duidString"/> into a <see cref="Duid"/>
        /// </summary>
        /// <param name="duidString">the DUID string</param>
        /// <param name="duid">the created DUID on success</param>
        /// <returns><see langword="true"/> on success</returns>
        /// <remarks>
        ///     <para>the anticipated input format is a string of hexadecimal are as follows</para>
        ///     <list type="bullet">
        ///         <item>string of hexadecimal octet pairs delimited by a single dash ('-'), colon (':') or space (' ') character. Leading 0 in pair may be omitted</item>
        ///         <item>string of undelimited hexadecimal octet pairs</item>
        ///     </list>
        /// </remarks>
        public static bool TryParse(string duidString, out Duid duid)
        {
            duid = null;

            if (string.IsNullOrEmpty(duidString))
            {
                return false;
            }

            var trimmed = duidString.Trim();

            if (DuidRegexPatterns.GetDelimitedOctetsRegex().IsMatch(trimmed))
            {
                duid = new Duid(DelimitedStringToBytes(trimmed, 1));
                return true;
            }

            if (DuidRegexPatterns.GetUndelimitedOctetsRegex().IsMatch(trimmed))
            {
                duid = new Duid(UndelimitedStringToBytes(trimmed));
                return true;
            }

            return false;
        }

        /// <summary>
        ///     Parse the given <paramref name="duidString"/> into a <see cref="Duid"/>
        /// </summary>
        /// <param name="duidString">the DUID string</param>
        /// <returns>the created DUID</returns>
        /// <remarks>
        ///     <para>the anticipated input format is a string of hexadecimal are as follows</para>
        ///     <list type="bullet">
        ///         <item>string of hexadecimal octet pairs delimited by a single dash ('-'), colon (':') or space (' ') character. Leading 0 in pair may be omitted</item>
        ///         <item>string of undelimited hexadecimal octet pairs</item>
        ///     </list>
        /// </remarks>
        public static Duid Parse(string duidString)
        {
            if (string.IsNullOrEmpty(duidString))
            {
                throw new ArgumentException("cannot be null or empty", nameof(duidString));
            }

            var trimmed = duidString.Trim();

            if (DuidRegexPatterns.GetDelimitedOctetsRegex().IsMatch(trimmed))
            {
                return new Duid(DelimitedStringToBytes(trimmed, 1));
            }
            else if (DuidRegexPatterns.GetUndelimitedOctetsRegex().IsMatch(trimmed))
            {
                // convert and return non-delimited string of octets into bytes
                return new Duid(UndelimitedStringToBytes(trimmed));
            }

            throw new ArgumentException("could not parse as DUID", nameof(duidString));
        }

        #endregion factory methods

        #region utility methods

        /// <summary>
        /// Converts a string of undelimited hexadecimal characters to a byte array.
        /// </summary>
        /// <param name="str">The string containing undelimited hexadecimal characters.</param>
        /// <returns>A byte array representing the hexadecimal values in the string.</returns>
        private static byte[] UndelimitedStringToBytes(string str)
        {
            var bytes = new byte[str.Length / 2];
            for (var i = 0; i < str.Length / 2; i++)
            {
                bytes[i] = byte.Parse(str.Substring(i * 2, 2), NumberStyles.HexNumber);
            }
            return bytes;
        }

        /// <summary>
        /// Converts a string of delimited hexadecimal characters to a byte array.
        /// </summary>
        /// <param name="str">The string containing delimited hexadecimal characters.</param>
        /// <param name="delimiterLength">The length of the delimiter between hexadecimal characters.</param>
        /// <returns>A byte array representing the hexadecimal values in the string.</returns>
        private static byte[] DelimitedStringToBytes(string str, int delimiterLength)
        {
            var result = new List<byte>();

            var characterIndex = 0;
            while (characterIndex < str.Length)
            {
                var ithChar = HexCharToUpper(str[characterIndex]);

                if (characterIndex == str.Length - 1)
                {
                    result.Add(HexCharValue(ithChar));
                    break;
                }

                var nextChar = HexCharToUpper(str[characterIndex + 1]);
                var nextCharIsHex = nextChar is >= '0' and <= '9' or >= 'A' and <= 'F';

                if (nextCharIsHex)
                {
                    result.Add((byte)((HexCharValue(ithChar) << 4) | HexCharValue(nextChar)));
                    characterIndex++;
                }
                else
                {
                    result.Add(HexCharValue(ithChar));
                }

                characterIndex += delimiterLength + 1;
            }

            return [.. result];
        }

        /// <summary>
        /// Converts a hexadecimal character to its uppercase equivalent.
        /// </summary>
        /// <param name="input">The hexadecimal character.</param>
        /// <returns>The uppercase equivalent of the hexadecimal character.</returns>
        private static char HexCharToUpper(char input) => char.ToUpperInvariant(input);

        /// <summary>
        /// Converts a hexadecimal character to its byte value.
        /// </summary>
        /// <param name="input">The hexadecimal character.</param>
        /// <returns>The byte value of the hexadecimal character.</returns>
        private static byte HexCharValue(char input)
        {
            if (input < 'A')
            {
                return (byte)(input - '0');
            }

            return (byte)(10 + (input - 'A'));
        }

        #endregion
    }
}
