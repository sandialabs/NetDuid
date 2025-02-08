using System.Text.RegularExpressions;

namespace NetDuid
{
    /// <summary>
    ///     Utility class for handling <see cref="Regex"/> sourcing for <see cref="Duid"/>. This is done such that NET 7
    ///     and greater targets may use Regex source generation while older targets may fall back to legacy Regex.
    /// </summary>
    /// <remarks>
    ///     <para> Case-insensitive backreferences are not supported by the source generator IgnoreCase is not used to ensure a generated implementation is possible.</para>
    /// </remarks>
    internal static
#if NET7_0_OR_GREATER
    partial
#endif
    class DuidRegexSource
    {
        /// <summary>
        ///     Regex pattern that should match a string of hexadecimal octet pairs delimited by a single dash ('-'), colon (':') or space (' ') character. Leading 0 in pair may be omitted.
        /// </summary>
        /// <remarks>
        ///     <para>The pattern is explicitly **not** set to ignore case due to current limitations of Case-insensitive backreferences for source generation.</para>
        /// </remarks>
        private const string DelimitedOctetsPattern =
            @"^(?:[0-9a-fA-F]{1,2}(?<separator>[-: ]))(?:[0-9a-fA-F]{1,2}\k<separator>){0,128}[0-9a-fA-F]{1,2}$";

        /// <summary>
        ///     Regex pattern that should match a string of undelimited hexadecimal octet pairs.
        /// </summary>
        private const string UndelimitedOctetPattern = "^(?:[0-9a-f]{2}){3,130}$";

        /// <summary>
        ///     Get the regular expression used to identify delimited octets
        /// </summary>
        /// <remarks>
        ///     <para>
        ///     For dotnet versions that support regular expression source generation (as of .NET 9) case-insensitive backreferences are not
        ///     yet supported by the source generator. Enabling case-insensitivity would prevent source generation and require fallback to
        ///     the legacy Regex implementation. This limitation may change in future .NET versions.
        ///     </para>
        ///     <para>
        ///         See <see href="https://learn.microsoft.com/en-us/dotnet/standard/base-types/regular-expression-source-generators#inside-the-source-generated-files">.NET regular expression source generators: Inside the source-generated files</see> and
        ///         <see href="https://devblogs.microsoft.com/dotnet/regular-expression-improvements-in-dotnet-7/#source-generation">Regular Expression Improvements in .NET 7: Source Generation</see>
        ///     </para>
        /// </remarks>
        /// <returns>a regular expression</returns>
#if NET7_0_OR_GREATER
        [GeneratedRegex(DelimitedOctetsPattern)]
        public static partial Regex GetDelimitedOctetsRegex();
#else
        public static Regex GetDelimitedOctetsRegex()
        {
            return DelimitedOctetsRegex;
        }

        private static readonly Regex DelimitedOctetsRegex = new Regex(DelimitedOctetsPattern, RegexOptions.Compiled);
#endif

        /// <summary>
        ///     Get the regular expression used to identify undelimited octets
        /// </summary>
        /// <returns>a regular expression</returns>
#if NET7_0_OR_GREATER
        [GeneratedRegex(UndelimitedOctetPattern, RegexOptions.IgnoreCase)]
        public static partial Regex GetUndelimitedOctetsRegex();
#else
        public static Regex GetUndelimitedOctetsRegex()
        {
            return UndelimitedOctetsRegex;
        }

        private static readonly Regex UndelimitedOctetsRegex = new Regex(
            UndelimitedOctetPattern,
            RegexOptions.Compiled | RegexOptions.IgnoreCase
        );
#endif
    }
}
