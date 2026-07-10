# Changelog

All notable changes to this project will be documented in this file.

## [3.0.0] - TBD

### Added

- `Length` property on `Duid` for retrieving the octet count without allocating a collection.
- `Span` property (`ReadOnlySpan<byte>`) on `Duid` for zero-allocation access to underlying bytes (.NET 8+).
- `Memory` property (`ReadOnlyMemory<byte>`) on `Duid` for interop-friendly access to underlying bytes (.NET 8+).
- `Duid(ReadOnlySpan<byte>)` constructor for stack-friendly DUID creation from span data (.NET 8+).
- `ToString(string)` convenience overload that delegates to `ToString(string, IFormatProvider)`.
- `ISpanFormattable` implementation for formatting DUIDs into `Span<char>` buffers (.NET 8+).
- `IUtf8SpanFormattable` implementation for formatting DUIDs directly into UTF-8 `Span<byte>` buffers (.NET 8+).
- `API_REFERENCE.md` - comprehensive public API documentation covering construction, validation, formatting, equality, comparison, operators, serialization, and parsing.
- Whitespace-tolerant parsing - `Parse` and `TryParse` now trim leading and trailing whitespace from input strings.
- `[NotNullWhen]` and `[MaybeNullWhen]` nullability annotations on `TryParse` overloads for .NET 8+ targets, enabling compiler null analysis after parse results.
- `Microsoft.Bcl.Memory` dependency for the `net48` target, providing modern `Span<T>` and `Memory<T>` APIs on .NET Framework.
- Smoke test suite (`smoketests/`) that validates the packed NuGet package works correctly as a consumer reference across all target frameworks.
- BenchmarkDotNet project (`src/NetDuid.Benchmarks/`) for performance regression testing covering comparison, equality, factory, formatting, and miscellaneous operations.

### Changed

- **Central Package Management** - all NuGet package versions are now centralized in `Directory.Packages.props` instead of individual `.csproj` files.
- Solution file migrated from `.sln` to `.slnx` format and moved to the repository root.
- Configuration files consolidated to the repository root: `.editorconfig`, `stylecop.json`, `.csharpierrc.json`, and `global.json` now live at the repo root instead of under `src/`.
- Regex patterns used in parsing now include a 1-second timeout to protect against ReDoS on malformed input.
- `Equals(Duid)` optimized to directly access the underlying byte array instead of allocating a `ReadOnlyCollection<byte>` on every equality check.
- Format string parsing extracted into a dedicated method for cleaner internals (no behavioral change).
- `DuidRegexSource` renamed to `DuidRegexPatterns` (internal type, not part of public API).

### Fixed

- Undelimited hex parsing now accepts uppercase characters (`A-F`). The `UndelimitedOctetPattern` regex was missing `A-F`, causing `TryParse` and `Parse` to reject valid uppercase undelimited DUID strings.
- `CompareTo(Duid)` and `CompareTo(object)` now return `1` when comparing a non-null DUID to `null`, correcting a violation of the `IComparable<T>` contract. Previously these methods returned `-1`, which incorrectly treated non-null values as less than null.

### Removed

- `GeneratePackageOnBuild` MSBuild property - packaging is now an explicit CI step, improving build times during local development.
- `.husky/pre-commit` hook - replaced by the dual Husky.Net and Python pre-commit system.
