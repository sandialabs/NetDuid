using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace NetDuid
{
    /// <summary>
    ///     Representation of a DHCP Unique Identifier (DUID) as interpreted from RFC8415
    ///     ("Dynamic Host Configuration Protocol for IPv6 (DHCPv6)") and RFC6355
    ///     ("Definition of the UUID-Based DHCPv6 Unique Identifier (DUID-UUID)")
    /// </summary>
    [DebuggerDisplay("DUID: {Type} {ToString()}")]
    [Serializable]
    public sealed partial class Duid
    {
        /// <summary>
        ///     The bytes that comprise the DUID
        /// </summary>
        private readonly byte[] _duidBytes;

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

        /// <summary>
        ///     Get the underlying bytes of the DUID
        /// </summary>
        /// <returns>a collection of big-endian bytes</returns>
        public IReadOnlyCollection<byte> GetBytes()
        {
            return _duidBytes;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return ToString(null, null); // use default IFormattable
        }
    }
}
