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
            var (delimiter, toUpper) = ParseFormatString(format);

            var octetLength = delimiter is null ? 2 : 3;
            var resultLength = _duidBytes.Length * octetLength;
            var characters = new char[resultLength];
            var nibbleFormatter = toUpper ? (Func<int, char>)GetUpperHexNibble : GetLowerHexNibble;

            for (var i = 0; i < _duidBytes.Length; i++)
            {
                var @byte = _duidBytes[i];
                var characterIndex = i * octetLength;

                characters[characterIndex] = nibbleFormatter(@byte >> 4);
                characters[characterIndex + 1] = nibbleFormatter(@byte & 0x0F);

                if (delimiter is not null)
                {
                    characters[characterIndex + 2] = delimiter.Value;
                }
            }

            return delimiter is null ? new string(characters) : new string(characters, 0, resultLength - 1);
        }

        #endregion

        #region utility methods

        /// <summary>
        /// Parses a format string into a delimiter character and case preference.
        /// </summary>
        /// <param name="format">
        /// A format string that specifies the formatting options.
        /// Valid values: <c>null</c>, empty, <c>"U"</c>, <c>"u"</c>, <c>"L"</c>, <c>"l"</c>,
        /// <c>":"</c>, <c>"-"</c>, <c>"U:"</c>, <c>"u:"</c>, <c>"U-"</c>, <c>"u-"</c>,
        /// <c>"L:"</c>, <c>"l:"</c>, <c>"L-"</c>, <c>"l-"</c>.
        /// </param>
        /// <returns>A tuple containing the delimiter character (or <c>null</c> for no delimiter) and whether to use uppercase hex.</returns>
        /// <exception cref="FormatException">Thrown when the format string is invalid.</exception>
        private static (char? delimiter, bool toUpper) ParseFormatString(string format)
        {
            if (string.IsNullOrEmpty(format))
            {
                return (':', true);
            }

            if (format.Length > 2)
            {
                throw new FormatException($"Invalid format string: \"{format}\".");
            }

            if (format.Length == 1)
            {
                return format[0] switch
                {
                    'U' or 'u' => (null, true),
                    'L' or 'l' => (null, false),
                    ':' => (':', true),
                    '-' => ('-', true),
                    _ => throw new FormatException($"Invalid format string: \"{format}\"."),
                };
            }

            var toUpper = char.ToUpper(format[0]) switch
            {
                'U' => true,
                'L' => false,
                _ => throw new FormatException($"Invalid format string: \"{format}\"."),
            };
            var delimiter = format[1] switch
            {
                ':' => ':',
                '-' => '-',
                _ => throw new FormatException($"Invalid format string: \"{format}\"."),
            };
            return (delimiter, toUpper);
        }

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
