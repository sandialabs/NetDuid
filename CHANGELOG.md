# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/).

## [3.0.0] - TBD

### Added

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
- Dependency updates: SonarAnalyzer `10.15.0` to `10.29.0`, StyleCop `1.2.0-beta.556` to `1.1.118` (stable), SourceLink `8.0.0` to `10.0.300`, Roslynator `4.14.1` to `4.15.0`.

### Fixed

- `CompareTo(Duid)` and `CompareTo(object)` now return `1` when comparing a non-null DUID to `null`, correcting a violation of the `IComparable<T>` contract. Previously these methods returned `-1`, which incorrectly treated non-null values as less than null.

### Removed

- `GeneratePackageOnBuild` MSBuild property - packaging is now an explicit CI step, improving build times during local development.
- `.husky/pre-commit` hook - replaced by the dual Husky.Net and Python pre-commit system.
