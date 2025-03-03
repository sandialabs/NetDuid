using System.Runtime.Serialization;

namespace NetDuid
{
    /// <content>
    ///     <see cref="Duid"/> implementation of <see cref="ISerializable"/>
    /// </content>
    public sealed partial class Duid : ISerializable
    {
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
        ///     Serializable Version for version safe serialization
        /// </summary>
        /// <remarks>This value should change uniquely when the serialization process of the serialized members change</remarks>
        private const int SerializableVersion = 0;

        #region ISerializable

        /// <inheritdoc/>
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(SerializableVersion), SerializableVersion);
            info.AddValue(nameof(_duidBytes), _duidBytes, _duidBytes.GetType());
        }

        #endregion
    }
}
