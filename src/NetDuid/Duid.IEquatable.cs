using System;
using System.Linq;

namespace NetDuid
{
    /// <content>
    ///     <see cref="Duid"/> implementation of <see cref="IEquatable{Duid}"/>
    /// </content>
    public sealed partial class Duid : IEquatable<Duid>
    {
        /// <summary>
        ///     A lazily initialized cache of the <see cref="GetHashCode"/> value.
        /// </summary>
        private readonly Lazy<int> _lazyHashCode;

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return _lazyHashCode.Value;
        }

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

        #region utility methods

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
