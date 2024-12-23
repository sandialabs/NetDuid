using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
#if NET7_0_OR_GREATER
using System.Diagnostics.CodeAnalysis;
#endif

namespace NetDuid
{
    /// <summary>
    ///     Representation of a DHCP Unique Identifier (DUID) as interpreted from RFC8415
    ///     ("Dynamic Host Configuration Protocol for IPv6 (DHCPv6)") and RFC6355
    ///     ("Definition of the UUID-Based DHCPv6 Unique Identifier (DUID-UUID)")
    /// </summary>
    [DebuggerDisplay("DUID: {Type} {ToString()}")]
    [Serializable]
    public sealed class Duid : IEquatable<Duid>, IComparable<Duid>, IComparable, IFormattable,
#if NET7_0_OR_GREATER
            IParsable<Duid>,
#endif
            ISerializable
    {
        /// <summary>
        ///     Serializable Version for version safe serialization
        /// </summary>
        /// <remarks>This value should change uniquely when the serialization process of the serialized members change</remarks>
        private const int SerializableVersion = 0;

        /// <summary>
        ///     Regex pattern that should match a string of hexadecimal octet pairs delimited by a single dash ('-'), colon (':') or space (' ') character. Leading 0 in pair may be omitted.
        /// </summary>
        private const string DelimitedOctetsPattern =
            @"^(?:[0-9a-fA-F]{1,2}(?<separator>[-: ]))(?:[0-9a-fA-F]{1,2}\k<separator>){0,128}[0-9a-fA-F]{1,2}$";

        /// <summary>
        ///     Regex pattern that should match a string of undelimited hexadecimal octet pairs.
        /// </summary>
        private const string UndelimitedOctetPattern = "^(?:[0-9a-fA-F]{2}){3,130}$";

        /// <summary>
        ///     Regular expression used to identify delimited octets
        /// </summary>
        private static readonly Regex DelimitedOctetsRegex = new Regex(DelimitedOctetsPattern, RegexOptions.Compiled);

        /// <summary>
        ///     Regular expression used to identify undelimited octets
        /// </summary>
        private static readonly Regex UndelimitedOctetsRegex = new Regex(UndelimitedOctetPattern, RegexOptions.Compiled);

        /// <summary>
        ///     The bytes that comprise the DUID
        /// </summary>
        private readonly byte[] _duidBytes;

        /// <summary>
        ///     A lazily initialized cache of the <see cref="GetHashCode"/> value.
        /// </summary>
        private readonly Lazy<int> _lazyHashCode;

        /// <summary>
        ///     Gets DUID Type
        /// </summary>
        /// <value>
        /// The type of the DUID based on the 2-octet type code
        /// </value>
        /// <remarks>
        ///     <para>Be aware that it is possible for the DUID contents to disagree with the type specification</para>
        /// </remarks>
        public DuidType Type { get; }

        #region constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="Duid"/> class.
        /// </summary>
        /// <param name="bytes">DUID bytes</param>
        public Duid(IEnumerable<byte> bytes)
        {
            _duidBytes = ConstructWithBytesGuard(bytes);
            Type = GetDuidType();

            _lazyHashCode = new Lazy<int>(ComputeHashCode); // for a lazy hash code requiring only a single generation
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="Duid"/> class.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> to populate with data</param>
        /// <param name="context">The destination <see cref="StreamingContext"/> for this serialization</param>
        private Duid(SerializationInfo info, StreamingContext context)
        {
            var version = info.GetInt32(nameof(SerializableVersion));

            if (version == SerializableVersion)
            {
                var bytes =
                    info.GetValue(nameof(_duidBytes), typeof(byte[])) as byte[]
                    ?? throw new SerializationException("unrecognized input byte array");
                _duidBytes = ConstructWithBytesGuard(bytes);
                Type = GetDuidType();
                return;
            }

            throw new SerializationException("Could not deserialize unrecognized format");
        }

        /// <summary>
        ///     Determines the type of the DUID (DHCP Unique Identifier) based on the 2-octet type code.
        /// </summary>
        /// <returns>
        ///     The DUID type as a <see cref="DuidType"/> enumeration value.
        ///     Possible return values include:
        ///     <list type="bullet">
        ///         <item>
        ///             <description><see cref="DuidType.LinkLayerPlusTime"/>: If the type code is <c>0x0001</c>.</description>
        ///         </item>
        ///         <item>
        ///             <description><see cref="DuidType.VendorAssigned"/>: If the type code is <c>0x0002</c>.</description>
        ///         </item>
        ///         <item>
        ///             <description><see cref="DuidType.LinkLayer"/>: If the type code is <c>0x0003</c>.</description>
        ///         </item>
        ///         <item>
        ///             <description><see cref="DuidType.Uuid"/>: If the type code is <c>0x0004</c>.</description>
        ///         </item>
        ///         <item>
        ///             <description><see cref="DuidType.Undefined"/>: If the type code is <c>0x0000</c> or within the range <c>0x0005</c> to <c>0xFFFF</c> inclusive, or if the byte array length is less than 2.</description>
        ///         </item>
        ///     </list>
        /// </returns>
        /// <remarks>
        ///     The DUID type is determined by examining the first two bytes of the byte array <c>_duidBytes</c> in big-endian order.
        ///     If the byte array has fewer than two bytes, the method returns <see cref="DuidType.Undefined"/>.
        /// </remarks>
        private DuidType GetDuidType()
        {
            // DUID type is determined by the first two bytes (big endian) that represent a 2-octet type code
            // if at least two bytes don't exist we can't determine the type
            if (_duidBytes.Length < 2)
            {
                // this is in theory unreachable given all current construction enforces a minimum of 3 bytes
                return DuidType.Undefined;
            }

            if (_duidBytes[0] == 0x00)
            {
                switch (_duidBytes[1])
                {
                    case 0x01:
                        return DuidType.LinkLayerPlusTime;
                    case 0x02:
                        return DuidType.VendorAssigned;
                    case 0x03:
                        return DuidType.LinkLayer;
                    case 0x04:
                        return DuidType.Uuid;
                }
            }

            return DuidType.Undefined;
        }

        private static byte[] ConstructWithBytesGuard(IEnumerable<byte> bytes)
        {
            if (bytes is null)
            {
                throw new ArgumentNullException(nameof(bytes));
            }

            var byteArray = bytes.ToArray(); // ToArray is explicitly called (even if input is an array) so as to copy the bytes explicitly

            if (byteArray.Length == 0)
            {
                throw new ArgumentException("cannot create DUID from empty input", nameof(bytes));
            }

            if (byteArray.Length < 3)
            {
                throw new ArgumentException("cannot create DUID from an input with less than 3 octets", nameof(bytes));
            }

            if (byteArray.Length > 130)
            {
                throw new ArgumentException("cannot create DUID from an input with more than 130 octets", nameof(bytes));
            }

            return byteArray;
        }

        #endregion

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

        #region IParsable
#if NET7_0_OR_GREATER

        /// <summary>
        ///     Attempt to parse the given <paramref name="s"/> into a <see cref="Duid"/>
        /// </summary>
        /// <param name="s">the DUID string</param>
        /// <param name="provider">An object that provides culture-specific formatting information. This parameter is not used in this implementation.</param>
        /// <param name="result">the created DUID on success</param>
        /// <returns><see langword="true"/> on success</returns>
        /// <remarks>
        ///     <para>the anticipated input format is a string of hexadecimal are as follows</para>
        ///     <list type="bullet">
        ///         <item>string of hexadecimal octet pairs delimited by a single dash ('-'), colon (':') or space (' ') character. Leading 0 in pair may be omitted</item>
        ///         <item>string of undelimited hexadecimal octet pairs</item>
        ///     </list>
        /// </remarks>
        public static bool TryParse(
            [NotNullWhen(true)] string s,
            IFormatProvider provider,
            [MaybeNullWhen(false)] out Duid result
        )
        {
            return TryParse(s, out result); // Use the existing TryParse method as the provider is not relevant
        }

        /// <summary>
        ///     Parse the given <paramref name="s"/> into a <see cref="Duid"/>
        /// </summary>
        /// <param name="s">the DUID string</param>
        /// <param name="provider">An object that provides culture-specific formatting information. This parameter is not used in this implementation.</param>
        /// <returns>the created DUID</returns>
        /// <remarks>
        ///     <para>the anticipated input format is a string of hexadecimal are as follows</para>
        ///     <list type="bullet">
        ///         <item>string of hexadecimal octet pairs delimited by a single dash ('-'), colon (':') or space (' ') character. Leading 0 in pair may be omitted</item>
        ///         <item>string of undelimited hexadecimal octet pairs</item>
        ///     </list>
        /// </remarks>
        public static Duid Parse(string s, IFormatProvider provider)
        {
            return Parse(s); // Use the existing TryParse method as the provider is not relevant
        }
#endif

        #endregion

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

            if (DelimitedOctetsRegex.IsMatch(trimmed))
            {
                return new Duid(DelimitedStringToBytes(trimmed, 1));
            }
            else if (UndelimitedOctetsRegex.IsMatch(trimmed))
            {
                // convert and return non-delimited string of octets into bytes
                return new Duid(UndelimitedStringToBytes(trimmed));
            }

            throw new ArgumentException("could not parse as DUID", nameof(duidString));
        }

        #endregion factory methods

        /// <summary>
        ///     Get the underlying bytes of the DUID
        /// </summary>
        /// <returns>a collection of big-endian bytes</returns>
        public IReadOnlyCollection<byte> GetBytes()
        {
            return _duidBytes;
        }

        #region CompareTo

        /// <inheritdoc/>
        /// <remark>Sorting is not done in mathematical order, but rather by length then value</remark>
        public int CompareTo(Duid other)
        {
            if (ReferenceEquals(this, other))
            {
                return 0;
            }

            if (other is null)
            {
                return -1;
            }

            var thisLength = _duidBytes.Length;
            var otherLength = other._duidBytes.Length;

            if (thisLength < otherLength)
            {
                return -1;
            }

            if (thisLength > otherLength)
            {
                return 1;
            }

            // length must be the same at this point
            for (var i = 0; i < thisLength; i++)
            {
                var byteCompare = _duidBytes[i].CompareTo(other._duidBytes[i]);

                if (byteCompare != 0)
                {
                    return byteCompare;
                }
            }

            return 0;
        }

        /// <inheritdoc/>
        public int CompareTo(object obj)
        {
            if (ReferenceEquals(this, obj))
            {
                return 0;
            }

            if (obj is null)
            {
                return -1;
            }

            if (obj is Duid duid)
            {
                return CompareTo(duid);
            }

            throw new ArgumentException("unexpected type", nameof(obj));
        }
        #endregion

        #region Equals

        /// <inheritdoc/>
        public bool Equals(Duid other)
        {
            if (ReferenceEquals(this, other))
            {
                return true;
            }

            if (other is null)
            {
                return false;
            }

            // considered equal if this and other have the same number of bytes, and their bytes are the same
            var otherBytes = other.GetBytes();
            return _duidBytes.Length == otherBytes.Count && _duidBytes.SequenceEqual(otherBytes);
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj is null)
            {
                return false;
            }

            if (obj is Duid duid)
            {
                return Equals(duid);
            }

            return false;
        }
        #endregion

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return _lazyHashCode.Value;
        }

        #region Operators

        /// <summary>
        ///     Equality operator
        /// </summary>
        /// <param name="lhs">Left hand operand</param>
        /// <param name="rhs">Right hand operand</param>
        /// <returns>
        ///     <see langword="true"/> if <paramref name="lhs"/> and <paramref name="rhs"/> are logically or referentially equal.
        ///     Returns <see langword="true"/> if both are <see langword="null"/>.
        /// </returns>
        public static bool operator ==(Duid lhs, Duid rhs)
        {
            if (ReferenceEquals(lhs, rhs))
            {
                return true;
            }

            if (lhs is null || rhs is null)
            {
                return false;
            }

            return lhs.Equals(rhs);
        }

        /// <summary>
        ///     Inequality operator
        /// </summary>
        /// <param name="lhs">Left hand operand</param>
        /// <param name="rhs">Right hand operand</param>
        /// <returns>
        ///     <see langword="true"/> if <paramref name="lhs"/> and <paramref name="rhs"/> are logically or referentially unequal.
        ///     Returns <see langword="false"/> if both are <see langword="null"/>.
        /// </returns>
        public static bool operator !=(Duid lhs, Duid rhs) => !(lhs == rhs);

        /// <summary>
        ///     Less than operator
        /// </summary>
        /// <param name="lhs">Left hand operand</param>
        /// <param name="rhs">Right hand operand</param>
        /// <remarks>null is considered less than any non-null value</remarks>
        /// <returns>
        ///     <see langword="true"/> if <paramref name="lhs"/> is less than <paramref name="rhs"/>
        /// </returns>
        public static bool operator <(Duid lhs, Duid rhs)
        {
            if (ReferenceEquals(lhs, rhs))
            {
                return false;
            }

            if (lhs is null)
            {
                return !(rhs is null); // null is considered less than any non-null value
            }

            if (rhs is null)
            {
                return false; // non-null is not less than null
            }

            return lhs.CompareTo(rhs) < 0;
        }

        /// <summary>
        ///     Less than or equal to operator
        /// </summary>
        /// <param name="lhs">Left hand operand</param>
        /// <param name="rhs">Right hand operand</param>
        /// <remarks>null is considered less than any non-null value</remarks>
        /// <returns>
        ///     <see langword="true"/> if <paramref name="lhs"/> is less than or equal to <paramref name="rhs"/>.
        /// </returns>
        public static bool operator <=(Duid lhs, Duid rhs)
        {
            if (ReferenceEquals(lhs, rhs))
            {
                return true;
            }

            if (lhs is null)
            {
                return !(rhs is null); // null is considered less than any non-null value
            }

            if (rhs is null)
            {
                return false; // non-null is not less than null
            }

            return lhs.CompareTo(rhs) <= 0;
        }

        /// <summary>
        ///     Greater than operator
        /// </summary>
        /// <param name="lhs">Left hand operand</param>
        /// <param name="rhs">Right hand operand</param>
        /// <remarks>null is considered less than any non-null value</remarks>
        /// <returns>
        ///     <see langword="true"/> if <paramref name="lhs"/> is greater than <paramref name="rhs"/>.
        /// </returns>
        public static bool operator >(Duid lhs, Duid rhs)
        {
            if (ReferenceEquals(lhs, rhs))
            {
                return false;
            }

            if (lhs is null)
            {
                return false; // null is not greater than any value
            }

            if (rhs is null)
            {
                return true; // any non-null value is greater than null
            }

            return lhs.CompareTo(rhs) > 0;
        }

        /// <summary>
        ///     Greater than or equal to operator
        /// </summary>
        /// <param name="lhs">Left hand operand</param>
        /// <param name="rhs">Right hand operand</param>
        /// <remarks>null is considered less than any non-null value</remarks>
        /// <returns>
        ///     <see langword="true"/> if <paramref name="lhs"/> is greater than or equal to <paramref name="rhs"/>.
        /// </returns>
        public static bool operator >=(Duid lhs, Duid rhs)
        {
            if (ReferenceEquals(lhs, rhs))
            {
                return true;
            }

            if (lhs is null)
            {
                return false; // null is not greater than any value
            }

            if (rhs is null)
            {
                return true; // any non-null value is greater than null
            }

            return lhs.CompareTo(rhs) >= 0;
        }

        #endregion

        /// <inheritdoc/>
        public override string ToString()
        {
            return ToString(null, null); // use default IFormattable
        }

        #region

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

        #region ISerializable

        /// <inheritdoc/>
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(SerializableVersion), SerializableVersion);
            info.AddValue(nameof(_duidBytes), _duidBytes, _duidBytes.GetType());
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

        /// <summary>
        /// Computes the hash code for the current instance based on the _duidBytes field.
        /// </summary>
        /// <remarks>Intended usage is for generating a lazy hash code as they underlying bytes should not change</remarks>
        /// <returns>The computed hash code.</returns>
        private int ComputeHashCode()
        {
            var hashCode = default(HashCode);

            // .NET Standard 2.0 does not have a hashCode.Add(byte[]) method. Despite it being present in newer versions,
            // we want to ensure that different builds of the same assembly with different targets will hash to the same value.
            // Therefore, we add each byte individually to the hash code.
            foreach (var b in _duidBytes)
            {
                hashCode.Add(b);
            }

            // Convert the accumulated hash code components into a single hash code value.
            return hashCode.ToHashCode();
        }

        #endregion
    }
}
