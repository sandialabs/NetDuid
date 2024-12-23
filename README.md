# NetDuid

## About the Project

### Background

The `NetDuid` library was developed by the Production Tools Team at [Sandia National Laboratories](http://sandia.gov). The primary motivation behind this project is to provide a robust and efficient .NET representation of DHCP Unique Identifiers (DUIDs) as specified in [RFC 8415, "_Dynamic Host Configuration Protocol for IPv6 (DHCPv6)_"](https://datatracker.ietf.org/doc/html/rfc8415) and [RFC 6355, "_Definition of the UUID-Based DHCPv6 Unique Identifier (DUID-UUID)_"](https://datatracker.ietf.org/doc/html/rfc6355). DUIDs are essential in the context of DHCPv6, where they are used to uniquely identify clients and servers.

### Goals

The main goals of this project are:

- **Standards Compliance**: Ensure full compliance with the relevant RFCs to accurately represent and manipulate DUIDs.
- **Ease of Use**: Provide a simple and intuitive API for developers to work with DUIDs in .NET applications.
- **Performance**: Optimize the library for performance, ensuring that it can handle large volumes of DUIDs efficiently.
- **Cross-Platform and Target Support**: Target multiple .NET versions to ensure compatibility across different platforms and environments.

### Features

- **Parsing and Construction**: Easily parse DUIDs from strings or construct them from byte arrays.
- **Comparison and Equality**: Implement comparison and equality operations for DUIDs.
- **Formatting**: Convert DUIDs to formatted string representations for display or logging.

### Use Cases

This library is intended for use in various scenarios, including but not limited to:

- **Network Configuration**: Managing and configuring network devices that use DHCPv6.
- **Logging and Monitoring**: Tracking and logging DUIDs in network traffic for monitoring and analysis.
- **Testing and Simulation**: Simulating DHCPv6 clients and servers in test environments.

## Getting Started

You are most likely to be interacting with the `NetDuid.Duid` type.

The `Duid` type implements `IEquatable<Duid>`, `IComparable<Duid>`, `IFormattable`, `ISerializable`, and for .NET 7 and greater `IParsable<Duid>`.

The library "knows" RFC 8415 ("Link-layer address plus time", "Vendor-assigned unique ID based on Enterprise Number", and "Link-layer address") and RFC 6355 ("Universally Unique Identifier (UUID)") DUIDs, but can treat any valid `byte` array (a minimum of 3 bytes, and maximum of 130 bytes per the RFCs) as a DUID. An unhandled DUID type will be treated as an "Undefined" type, but otherwise functionality is identical.

### Creating a DUID

#### Via Constructor

The most common way to create a DUID is to construct it via an array of `byte` data.

```csharp
var duidBytes = new byte[] { 0x00, 0x01, 0x02, 0x03 };
var duid = new Duid(duidBytes);
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

Calling `GetBytes()` will return a read only collection of the underlying bytes of a DUID.

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

The `CompareTo`, and its operators, is not done in mathematical order or bytes, but rather fist by byte length then by unsigned value. When using the comparison operators a `null` value is considered less than any non-`null` value.

## Developer Notes

### Built With

This project was built with the aid of:

- [CSharpier](https://csharpier.com/)
- [dotnet-outdated](https://github.com/dotnet-outdated/dotnet-outdated)
- [Husky.Net](https://alirezanet.github.io/Husky.Net/)
- [Roslynator](https://josefpihrt.github.io/docs/roslynator/)
- [SonarAnalyzer](https://www.sonarsource.com/products/sonarlint/features/visual-studio/)
- [StyleCop.Analyzers](https://github.com/DotNetAnalyzers/StyleCopAnalyzers)
- [xUnit.net](https://xunit.net/)

The project has no runtime library dependencies outside of [`Microsoft.Bcl.HashCode`](https://www.nuget.org/packages/Microsoft.Bcl.HashCode) when targeting .NET Standard 2.0.

### Versioning

This project uses [Semantic Versioning](https://semver.org/)

### Targeting

The project targets [.NET Standard 2.0](https://learn.microsoft.com/en-us/dotnet/standard/net-standard?tabs=net-standard-2-0), [.NET 6](https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-6), [.NET 7](https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-7), and [.NET 8](https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-8). The test project similarly targets .NET 6, .NET 7, and .NET 8, but targets [.NET Framework 4.8](https://dotnet.microsoft.com/en-us/download/dotnet-framework/net48) for the .NET Standard 2.0 tests.

### Commit Hook

The project itself has a configured pre-commit git hook, via [Husky.Net](https://alirezanet.github.io/Husky.Net/) that automatically lints and formats code via [dotnet format](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-format) and [csharpier](https://csharpier.com/).

#### Disable husky in CI/CD pipelines

Per the [Husky.Net instructions](https://alirezanet.github.io/Husky.Net/guide/automate.html#disable-husky-in-ci-cd-pipelines)

> You can set the `HUSKY` environment variable to `0` in order to disable husky in CI/CD pipelines.

#### Manual Linting and Formatting

On occasion a manual run is desired it may be done so via the `src` directory and with the command

```shell
dotnet format style; dotnet format analyzers; dotnet csharpier .
```

These commands may be called independently, but order may matter.

#### Testing

After making changes tests should be run that include all targets

## Acknowledgments

This project was built by the Production Tools Team at Sandia National Laboratories. Special thanks to all contributors and reviewers who helped shape and improve this library.

Including, but not limited to:

- Robert H. Engelhardt
- Drew Antonich
- Stephen Jackson
- Sterling Violette

## Copyright

> Copyright 2024 National Technology & Engineering Solutions of Sandia, LLC (NTESS). Under the terms of Contract DE-NA0003525 with NTESS, the U.S. Government retains certain rights in this software

## License

> Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at
> http://www.apache.org/licenses/LICENSE-2.0
> Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.

This project is licensed under the terms of Contract DE-NA0003525 with NTESS. The U.S. Government retains certain rights in this software.
