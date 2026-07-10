# NetDuid

![GitHub Actions Workflow Status](https://img.shields.io/github/actions/workflow/status/sandialabs/NetDuid/build.yml?branch=main&logo=github&label=build)
[![NuGet Version](https://img.shields.io/nuget/v/NetDuid?logo=nuget)](https://www.nuget.org/packages/NetDuid)
[![NuGet Downloads](https://img.shields.io/nuget/dt/NetDuid?logo=nuget)](https://www.nuget.org/packages/NetDuid)
[![GitHub Release](https://img.shields.io/github/v/release/sandialabs/NetDuid?logo=github)](https://github.com/sandialabs/NetDuid/releases)
![Targets](https://img.shields.io/badge/.NET-Standard%202.0%20%7C%208.0%20%7C%209.0%20%7C%2010.0-blue)
[![License](https://img.shields.io/github/license/sandialabs/NetDuid?logo=apache)](https://github.com/sandialabs/NetDuid/blob/main/LICENSE)

## About the Project

### Background

The `NetDuid` library was developed by the Production Tools Team at [Sandia National Laboratories](http://sandia.gov). The primary motivation behind this project is to provide a robust and efficient .NET representation of DHCP Unique Identifiers (DUIDs) as specified in [RFC 8415, "_Dynamic Host Configuration Protocol for IPv6 (DHCPv6)_"](https://datatracker.ietf.org/doc/html/rfc8415) and [RFC 6355, "_Definition of the UUID-Based DHCPv6 Unique Identifier (DUID-UUID)_"](https://datatracker.ietf.org/doc/html/rfc6355). DUIDs are essential in the context of DHCPv6, where they are used to uniquely identify clients and servers.

### Goals

The main goals of this project are:

- **Standards Compliance**: Ensure full compliance with the relevant RFCs to accurately represent and manipulate DUIDs.
- **Ease of Use**: Provide a simple and intuitive API for developers to work with DUIDs in .NET applications.
- **Performance**: Optimize the library for performance, ensuring that it can handle large volumes of DUIDs efficiently.
- **Cross-Platform and Target Support**: Target multiple .NET versions to ensure compatibility across different platforms and environments.

### Use Cases

This library is intended for use in various scenarios, including but not limited to:

- **Network Configuration**: Managing and configuring network devices that use DHCPv6.
- **Logging and Monitoring**: Tracking and logging DUIDs in network traffic for monitoring and analysis.
- **Testing and Simulation**: Simulating DHCPv6 clients and servers in test environments.

## Changelog

For a detailed list of changes, see [CHANGELOG.md](CHANGELOG.md).

## Getting Started

For a complete API reference, see [API_REFERENCE.md](API_REFERENCE.md).

You are most likely to be interacting with the `NetDuid.Duid` type.

The `Duid` type implements `IEquatable<Duid>`, `IComparable<Duid>`, `IFormattable`, `ISerializable`, for .NET 7+ `IParsable<Duid>`, and for .NET 8+ `ISpanFormattable` and `IUtf8SpanFormattable`.

The library "knows" RFC 8415 ("Link-layer address plus time", "Vendor-assigned unique ID based on Enterprise Number", and "Link-layer address") and RFC 6355 ("Universally Unique Identifier (UUID)") DUIDs, but can treat any valid `byte` array (a minimum of 3 bytes, and maximum of 130 bytes per the RFCs) as a DUID. An unhandled DUID type will be treated as an "Undefined" type, but otherwise functionality is identical.

### Creating a DUID

#### Via Constructor

The most common way to create a DUID is to construct it via an array of `byte` data.

```csharp
var duidBytes = new byte[] { 0x00, 0x01, 0x02, 0x03 };
var duid = new Duid(duidBytes);
```

On .NET 8+ you can also construct from a `ReadOnlySpan<byte>`, which is useful for stack-allocated data or pool-sourced buffers:

```csharp
Span<byte> stackBytes = stackalloc byte[] { 0x00, 0x01, 0x02, 0x03 };
var duid = new Duid((ReadOnlySpan<byte>)stackBytes);
```

#### Parsing a string to a DUID

You can also parse a DUID from a `string` using the `TryParse` or `Parse` methods.
The parsing methods expect either a `string` of hexadecimal octet pairs optionally delimited by a single dash (`-`), colon (`:`) or space character. The leading `0` in a delimited pair may be omitted. Input strings will be trimmed prior to parsing, and casing is ignored.

```csharp
if (Duid.TryParse("00:01:02:03", out var parsedDuid))
{
    Console.WriteLine(parsedDuid.ToString());
}
```

or

```csharp
var parsedDuidB = Duid.Parse("00:01:A2:b3");
var parsedDuidA = Duid.Parse("00-01-A2-b3");
var parsedDuidC = Duid.Parse("0001A2b3");
var parsedDuidD = Duid.Parse("0:1:A2:b3");
```

### Formatting

`ToString()` and `ToString(string format, IFormatProvider formatProvider)` Converts the DUID to a formatted string representation.

There exist three generally recognized DUID string formats with upper and lower case variants:

- Colon delimited e.g. `00:01:A2:B3` or `00:01:a2:b3`. _Note that upper cased colon delimited is the default format._
- Dash delimited e.g. `00-01-A2-B3` or `00-01-a2-b3`.
- Non-delimited e.g. `0001A2B3` or `0001a2b3`.

Simply calling `ToString()` will return the default format of upper cased colon delimited string. However, as the Duid type implements `IFormattable` there are other formatting options available as well.

| Format String                   | Description                                | Example Output |
| ------------------------------- | ------------------------------------------ | -------------- |
| `null`, empty string, `:`, `U:` | Uppercase with colon delimiter (_default_) | `00:01:A2:B3`  |
| `U-`                            | Uppercase with dash delimiter              | `00-01-A2-B3`  |
| `U`                             | Uppercase with no delimiter                | `0001A2B3`     |
| `L:`                            | Lowercase with colon delimiter             | `00:01:a2:b3`  |
| `L-`                            | Lowercase with dash delimiter              | `00-01-a2-b3`  |
| `L`                             | Lowercase with no delimiter                | `0001a2b3`     |

### Gathering Information about a DUID

#### Bytes

Calling `GetBytes()` will return a read only collection of the underlying bytes of a DUID. The `Length` property provides the octet count without any allocation.

On .NET 8+, `Span` and `Memory` properties provide zero-allocation access to the underlying bytes as `ReadOnlySpan<byte>` and `ReadOnlyMemory<byte>` respectively.

#### DUID Types

Accessing the `Duid.Type` property will return a `NetDuid.DuidType` enum based examining on the 2-octet type code of the DUID. The type code is the first two bytes of the DUID in big-endian order. It is generally recommended that a DUID is not interpreted outside of being an opaque array of bytes, as such the type is a best guess based on hints and should not be interpreted as definitive.

The `DuidType` enum emits the following values

| Enum Value          | Description                                                                     | RFC              |
| ------------------- | ------------------------------------------------------------------------------- | ---------------- |
| `Undefined`         | Any DUID with a type code not specified in RFC 8415 or RFC 6355.                | _not applicable_ |
| `LinkLayerPlusTime` | Link-layer address plus time (DUID type code `0x0001`).                         | RFC 8415         |
| `VendorAssigned`    | Vendor-assigned unique ID based on Enterprise Number (DUID type code `0x0002`). | RFC 8415         |
| `LinkLayer`         | Link-layer address (DUID type code `0x0003`).                                   | RFC 8415         |
| `Uuid`              | Universally Unique Identifier (UUID) (DUID type code `0x0004`).                 | RFC 6355         |

### Equality and Comparison

The `Duid` class implements `IEquatable<Duid>`, `IComparable<Duid>`, `IComparable` and the standard Equality and Comparison operators.

The `CompareTo`, and its operators, is not done in mathematical order or bytes, but rather first by byte length then by unsigned value. When using the comparison operators a `null` value is considered less than any non-`null` value.

## New Features in v3.0.0

### Span-Based APIs (.NET 8+)

- `Duid(ReadOnlySpan<byte>)` constructor — create DUIDs from stack-allocated or pool-sourced byte data.
- `Span` / `Memory` properties — zero-allocation access to underlying bytes.
- `ISpanFormattable` / `IUtf8SpanFormattable` — format DUIDs directly into character or UTF-8 byte buffers without intermediate string allocations.
- `ToString(string)` convenience overload — call `duid.ToString("L")` without providing a format provider.
- `Length` property — get the octet count without calling `GetBytes().Count`.

### Whitespace-Tolerant Parsing

`Parse` and `TryParse` now trim leading and trailing whitespace from input strings, so `"  00:01:A2:B3  "` parses successfully.

### Performance Improvements

- **Regex source generation**: On .NET 7+ targets, parsing uses `[GeneratedRegex]` for compile-time regex generation, reducing startup overhead.
- **Lazy hash code**: The hash code is computed once and cached. Deserialized instances also initialize the cache, fixing a `NullReferenceException` in earlier versions.

## Breaking Changes in v3.0.0

### `GetBytes()` Returns an Immutable View

The runtime type of the returned `IReadOnlyCollection<byte>` changed from `byte[]` to `ReadOnlyCollection<byte>`. Previously, callers could cast the return value and mutate the DUID's internal state. The new implementation wraps the array via `Array.AsReadOnly()`, enforcing true immutability.

### `CompareTo` Null Contract Corrected

`CompareTo(Duid?)` and `CompareTo(object?)` previously returned `-1` when comparing any non-null DUID to `null`, violating the `IComparable<T>` standard contract (non-null > null should return `1`). Apologies — this was improperly implemented and went unnoticed because null DUIDs are uncommon in practice. v3.0.0 corrects both overloads to return `1`.

## Developer Notes

### Built With

This project was built with the aid of:

- [CSharpier](https://csharpier.com/)
- [dotnet-outdated](https://github.com/dotnet-outdated/dotnet-outdated)
- [Husky.Net](https://alirezanet.github.io/Husky.Net/)
- [NSubstitute](https://nsubstitute.github.io/)
- [Roslynator](https://josefpihrt.github.io/docs/roslynator/)
- [SonarAnalyzer](https://www.sonarsource.com/products/sonarlint/features/visual-studio/)
- [StyleCop.Analyzers](https://github.com/DotNetAnalyzers/StyleCopAnalyzers)
- [xUnit.net](https://xunit.net/)

The project has no runtime library dependencies outside of [`Microsoft.Bcl.HashCode`](https://www.nuget.org/packages/Microsoft.Bcl.HashCode) when targeting .NET Standard 2.0.

### Versioning

This project uses [Semantic Versioning](https://semver.org/)

### Targeting

The project targets [.NET Standard 2.0](https://learn.microsoft.com/en-us/dotnet/standard/net-standard?tabs=net-standard-2-0), [.NET 8](https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-8), [.NET 9](https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-9/overview), and [.NET 10](https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-10/overview). The test project similarly targets .NET 8, .NET 9, .NET 10, but targets [.NET Framework 4.8](https://dotnet.microsoft.com/en-us/download/dotnet-framework/net48) for the .NET Standard 2.0 tests.

### Commit Hooks

The project has two pre-commit systems configured:

1. **[Husky.Net](https://alirezanet.github.io/Husky.Net/)** — lints and stages `.cs` files via `dotnet format` and [CSharpier](https://csharpier.com/).
2. **[pre-commit](https://pre-commit.com/)** (Python-based) — runs `codespell`, `markdownlint`, trailing-whitespace and end-of-file checks, YAML validation, and CSharpier on all supported file types.

Both run automatically on `git commit`. To disable Husky in CI/CD pipelines, set the `HUSKY` environment variable to `0`.

#### Manual Linting and Formatting

To run formatting manually from the repository root:

```shell
dotnet format style; dotnet format analyzers; dotnet csharpier format .
```

These commands may be called independently, but order may matter.

#### Testing

After making changes, run tests across all target frameworks:

```shell
dotnet test src --verbosity normal
```

## Acknowledgments

This project was built by the Production Tools Team at Sandia National Laboratories. Special thanks to all contributors and reviewers who helped shape and improve this library.

Including, but not limited to:

- [Robert H. Engelhardt](https://github.com/rheone)
- [Drew Antonich](https://github.com/drewantonich)
- [Stephen Jackson](https://github.com/scj7t4)
- [Sterling Violette](https://github.com/Sterlinghv)
- [Madison Brewer](https://github.com/mabdrew)

## Copyright

> Copyright 2026 National Technology & Engineering Solutions of Sandia, LLC (NTESS). Under the terms of Contract DE-NA0003525 with NTESS, the U.S. Government retains certain rights in this software

## License

> Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at
>
> <http://www.apache.org/licenses/LICENSE-2.0>
>
> Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.
