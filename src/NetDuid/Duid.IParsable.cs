using System;
#if NET7_0_OR_GREATER
using System.Diagnostics.CodeAnalysis;
#endif

namespace NetDuid
{
#if NET7_0_OR_GREATER
    /// <content>
    /// <see cref="Duid"/> implementation of <see cref="IParsable{Duid}"/>
    /// </content>
#else
    /// <content>
    /// <see cref="Duid"/> implementation of C# 7+ compatible implementation of <c>TryParse(string, IFormatProvider, out Duid)</c> without the <c>IParsable</c> interface
    /// </content>
#endif

    public sealed partial class Duid
#if NET7_0_OR_GREATER
        : IParsable<Duid>
#endif
    {
        #region IParsable

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
#if NET7_0_OR_GREATER
            [NotNullWhen(true)] string s,
            IFormatProvider provider,
            [MaybeNullWhen(false)] out Duid result
#else
            string s, IFormatProvider provider, out Duid result
#endif
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

        #endregion
    }
}
