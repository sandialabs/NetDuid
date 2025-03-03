using System;
using Xunit.Sdk;

namespace NetDuid.Tests.XunitSerializers
{
    /// <summary>
    ///     <see cref="IXunitSerializer"/> for <see cref="Duid"/>
    /// </summary>
    public class DuidXunitSerializer : IXunitSerializer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DuidXunitSerializer"/> class.
        /// </summary>
        /// <remarks>from <see href="https://xunit.net/docs/getting-started/v3/custom-serialization">xUnit Serialization support in v3</see></remarks>
        public DuidXunitSerializer() { }

        /// <inheritdoc/>
        public bool IsSerializable(Type type, object value, out string failureReason)
        {
            if (type == typeof(Duid) && value is Duid)
            {
                failureReason = null;
                return true;
            }

            failureReason = $"Type {type.FullName} is not supported by {nameof(DuidXunitSerializer)}.";
            return false;
        }

        /// <inheritdoc/>
        public string Serialize(object value)
        {
            if (value is Duid duid)
            {
                return duid.ToString();
            }

            throw new InvalidOperationException(
                $"Invalid type for serialization: {value.GetType().FullName} is not supported by {nameof(DuidXunitSerializer)}."
            );
        }

        /// <inheritdoc/>
        public object Deserialize(Type type, string serializedValue)
        {
            if (type == typeof(Duid))
            {
                return Duid.Parse(serializedValue);
            }

            throw new ArgumentException(
                $"Invalid type for deserialization: {type.FullName} is not supported by {nameof(DuidXunitSerializer)}"
            );
        }
    }
}
