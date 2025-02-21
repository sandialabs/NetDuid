using System;

namespace NetDuid
{
    /// <content>
    ///     <see cref="Duid"/> implementation of <see cref="IFormattable"/>
    /// </content>
    public sealed partial class Duid : IFormattable
    {
        #region IFormattable

        /// <summary>
        /// Converts the DUID bytes to a formatted string representation.
        /// </summary>
        /// <param name="format">
        /// A format string that specifies the formatting options. The format string can contain:
        /// <list type="bullet">
        /// <item>
        /// <description>'U' for uppercase hexadecimal characters.</description>
        /// </item>
        /// <item>
        /// <description>'L' for lowercase hexadecimal characters.</description>
        /// </item>
        /// <item>
        /// <description>':' for colon delimiter between bytes.</description>
        /// </item>
        /// <item>
        /// <description>'-' for dash delimiter between bytes.</description>
        /// </item>
        /// </list>
        /// <remarks>
        /// Valid format combinations:
        /// <list type="bullet">
        /// <item>
        /// <description><see langword="null"/>, empty string, <c>":"</c>, or <c>"U:"</c> (default): Uppercase with colon delimiter (e.g., <c>"12:34:AB:CD"</c>).</description>
        /// </item>
        /// <item>
        /// <description><c>"U-"</c>, or <c>"-"</c>: Uppercase with dash delimiter (e.g., <c>"12-34-AB-CD"</c>).</description>
        /// </item>
        /// <item>
        /// <description><c>"U"</c>: Uppercase with no delimiter (e.g., <c>"1234ABCD"</c>).</description>
        /// </item>
        /// <item>
        /// <description><c>"L:"</c>: Lowercase with colon delimiter (e.g., <c>"12:34:ab:cd"</c>).</description>
        /// </item>
        /// <item>
        /// <description><c>"L-"</c>: Lowercase with dash delimiter (e.g., <c>"12-34-ab-cd"</c>).</description>
        /// </item>
        /// <item>
        /// <description><c>"L"</c>: Lowercase with no delimiter (e.g., <c>"1234abcd"</c>).</description>
        /// </item>
        /// </list>
        /// </remarks>
        /// </param>
        /// <param name="formatProvider">An object that supplies culture-specific formatting information. This parameter is ignored in this implementation.</param>
        /// <returns>A string representation of the DUID bytes formatted according to the specified format string.</returns>
        /// <exception cref="FormatException">Thrown when the format string is invalid.</exception>
        public string ToString(string format, IFormatProvider formatProvider)
        {
            // Default format is uppercase with colon delimiter if format is null or empty
            if (string.IsNullOrEmpty(format))
            {
                format = "U:"; // Default format
            }

            // Format string should be at most 2 characters long
            if (format.Length > 2)
            {
                throw new FormatException($"Invalid format string: \"{format}\".");
            }

            char? delimiter;
            bool toUpper;

            // Determine the formatting options based on the format string
            if (format.Length == 1)
            {
                // Single character format string
                switch (format[0])
                {
                    case 'U':
                    case 'u':
                        toUpper = true;
                        delimiter = null;
                        break;
                    case 'L':
                    case 'l':
                        toUpper = false;
                        delimiter = null;
                        break;
                    case ':':
                        toUpper = true;
                        delimiter = ':';
                        break;
                    case '-':
                        toUpper = true;
                        delimiter = '-';
                        break;
                    default:
                        throw new FormatException($"Invalid format string: \"{format}\".");
                }
            }
            else
            {
                // Two character format string
                switch (char.ToUpper(format[0]))
                {
                    case 'U':
                    case 'u':
                        toUpper = true;
                        break;
                    case 'L':
                    case 'l':
                        toUpper = false;
                        break;
                    default:
                        throw new FormatException($"Invalid format string: \"{format}\".");
                }

                switch (format[1])
                {
                    case ':':
                        delimiter = ':';
                        break;
                    case '-':
                        delimiter = '-';
                        break;
                    default:
                        throw new FormatException($"Invalid format string: \"{format}\".");
                }
            }

            // Calculate the length of the resulting string
            var octetLength = delimiter is null ? 2 : 3; // Each byte is represented by 2 hex characters plus an optional delimiter
            var resultLength = _duidBytes.Length * octetLength;
            var characters = new char[resultLength];

            // Select the appropriate nibble formatting based on the case preference
            var nibbleFormatter = toUpper ? (Func<int, char>)GetUpperHexNibble : GetLowerHexNibble;

            // Convert each byte to its hexadecimal representation
            for (var i = 0; i < _duidBytes.Length; i++)
            {
                var @byte = _duidBytes[i];
                var characterIndex = i * octetLength; // Calculate the index in the result array

                // Convert the byte to hexadecimal characters
                characters[characterIndex] = nibbleFormatter(@byte / 16);
                characters[characterIndex + 1] = nibbleFormatter(@byte % 16);

                // Add the delimiter if specified
                if (delimiter != null)
                {
                    characters[characterIndex + 2] = (char)delimiter;
                }
            }

            // Return the formatted string, excluding the trailing delimiter if present
            if (delimiter is null)
            {
                return new string(characters);
            }
            return new string(characters, 0, resultLength - 1);
        }

        #endregion

        #region utility methods

        /// <summary>
        /// Converts an integer to its corresponding uppercase hexadecimal character.
        /// </summary>
        /// <param name="i">The integer to convert. Must be in the range 0-15.</param>
        /// <returns>
        /// A character representing the hexadecimal value of the integer.
        /// Returns '0'-'9' for values 0-9 and 'A'-'F' for values 10-15.
        /// </returns>
        private static char GetUpperHexNibble(int i)
        {
            // offset 0-9 by character offset '0', 10-16 by character offset 'A'
            return i < 10 ? (char)(i + '0') : (char)(i - 10 + 'A');
        }

        /// <summary>
        /// Converts an integer to its corresponding lowercase hexadecimal character.
        /// </summary>
        /// <param name="i">The integer to convert. Must be in the range 0-15.</param>
        /// <returns>
        /// A character representing the hexadecimal value of the integer.
        /// Returns '0'-'9' for values 0-9 and 'a'-'f' for values 10-15.
        /// </returns>
        private static char GetLowerHexNibble(int i)
        {
            // offset 0-9 by character offset '0', 10-16 by character offset 'a'
            return i < 10 ? (char)(i + '0') : (char)(i - 10 + 'a');
        }

        #endregion
    }
}
