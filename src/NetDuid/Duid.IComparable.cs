using System;

namespace NetDuid
{
    /// <content>
    ///     <see cref="Duid"/> implementation of <see cref="IComparable{Duid}"/> and <see cref="IComparable"/>
    /// </content>
    public sealed partial class Duid : IComparable<Duid>, IComparable
    {
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
    }
}
