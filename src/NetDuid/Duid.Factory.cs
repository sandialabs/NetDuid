using System;
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
            try
            {
                duid = Parse(duidString);
                return true;
            }
            catch
            {
                duid = null;
                return false;
            }
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

            if (DuidRegexSource.GetDelimitedOctetsRegex().IsMatch(trimmed))
            {
                return new Duid(DelimitedStringToBytes(trimmed, 1));
            }
            else if (DuidRegexSource.GetUndelimitedOctetsRegex().IsMatch(trimmed))
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
            // delimited bytes are a bit ugly as values between 0x0 and 0xF may optionally not include a leading '0' in byte octets
            // byte length is not known beforehand, but the maximum size can be calculated.
            // Each byte is represented by 1 or 2 characters plus the length of the delimiter (including a non-existent trailing delimiter
            // which will be trimmed on copy to array).
            var maxBytesLength = (str.Length + delimiterLength) / (1 + delimiterLength);
            var bytes = new byte[maxBytesLength];
            var byteIndex = 0;

            // Iterate though the string a character at a time looking ahead to the the next character
            // If the next character is hexadecimal character a full octet is present, if it is not then it is considered the start
            // if the delimiter is the next character (characterIndex+1). It is assumed that a delimiter will never start with a
            // hexadecimal digit.
            var characterIndex = 0;
            while (characterIndex < str.Length)
            {
                var ithChar = HexCharToUpper(str[characterIndex]); // ith character should always be a hex digit; it may also be a '0' prefix

                if (characterIndex == str.Length - 1) // at last position in string, no possibility of next
                {
                    // process the final character
                    bytes[byteIndex] = HexCharValue(ithChar);
                    byteIndex++; // byte was built, go to next index position in the byte array
                    break; // break the loop
                }

                // there exists a character after the ith character of the string
                var nextChar = HexCharToUpper(str[characterIndex + 1]);
                var nextCharIsHex = (nextChar >= '0' && nextChar <= '9') || (nextChar >= 'A' && nextChar <= 'F');

                if (nextCharIsHex) // next character is hex
                {
                    bytes[byteIndex] = (byte)((HexCharValue(ithChar) << 4) | HexCharValue(nextChar)); // build a byte from two hex characters
                    characterIndex++; // two digit octet consumed; skip the next character in the iteration
                }
                else // next character must be the start of delimiter
                {
                    bytes[byteIndex] = HexCharValue(ithChar);
                }

                byteIndex++; // byte was built, go to next index position in the byte array
                characterIndex += delimiterLength + 1;
            }

            // build actual byte array based on findings
            var result = new byte[byteIndex];
            Buffer.BlockCopy(bytes, 0, result, 0, byteIndex);
            return result;
        }

        /// <summary>
        /// Converts a lowercase hexadecimal character to its uppercase equivalent.
        /// </summary>
        /// <param name="input">The lowercase hexadecimal character.</param>
        /// <returns>The uppercase equivalent of the hexadecimal character.</returns>
        private static char HexCharToUpper(char input)
        {
            if (input >= 'a' && input <= 'f')
            {
                return (char)(input - ' '); // Upper case is 32 characters before lower case in ASCII; it just so happens space is 32
            }

            return input;
        }

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
