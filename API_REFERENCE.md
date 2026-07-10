# API Reference

## `NetDuid.Duid` (sealed class)

The primary type of the library. Represents a DHCP Unique Identifier (DUID) as an immutable byte array.

Implements: `IEquatable<Duid>`, `IComparable<Duid>`, `IComparable`, `IFormattable`, `ISerializable`, and (on .NET 7+) `IParsable<Duid>`. On .NET 8+ it additionally implements `ISpanFormattable` and `IUtf8SpanFormattable`.

---

### Construction

```csharp
public Duid(IEnumerable<byte> bytes)
```

Creates a DUID from any enumerable of bytes. Accepts `byte[]`, `List<byte>`, `Memory<byte>`, or any other `IEnumerable<byte>`.

```csharp
// from byte array
var duid = new Duid(new byte[] { 0x00, 0x01, 0x02, 0x03 });

// from List<byte>
var duid = new Duid(new List<byte> { 0x00, 0x01, 0x02, 0x03 });
```

**Validation rules:**
| Condition | Exception |
|-----------|-----------|
| `null` | `ArgumentNullException` |
| Empty (0 bytes) | `ArgumentException` |
| 1–2 bytes | `ArgumentException` ("less than 3 octets") |
| 3–130 bytes | Success |
| 131+ bytes | `ArgumentException` ("more than 130 octets") |

#### `Duid(ReadOnlySpan<byte>)` (.NET 8+)

```csharp
public Duid(ReadOnlySpan<byte> bytes)
```

Creates a DUID from a read-only span of bytes. This constructor is useful when working with stack-allocated data, `ArrayPool<byte>`, or other span-based APIs. The bytes are copied defensively into an internal array.

```csharp
// from stack-allocated span
Span<byte> stackBytes = stackalloc byte[] { 0x00, 0x01, 0x02, 0x03 };
var duid = new Duid((ReadOnlySpan<byte>)stackBytes);
```

Same validation rules as the `IEnumerable<byte>` constructor. Throws `ArgumentException` on invalid input.

---

### Properties

#### `Type`

```csharp
public DuidType Type { get; }
```

A heuristic type label derived from the first two bytes (big-endian type code). See `DuidType` below.

> **Important:** The RFC advises treating DUIDs as opaque byte arrays — this property is a best guess based on the type code, not a definitive classification. The byte content may disagree with the type label.

```csharp
var duid = new Duid(new byte[] { 0x00, 0x01, 0xAB, 0xCD });
Console.WriteLine(duid.Type); // LinkLayerPlusTime
```

#### `Length`

```csharp
public int Length { get; }
```

The number of octets in the DUID (always between 3 and 130 inclusive). This is a zero-allocation alternative to `GetBytes().Count`.

```csharp
var duid = new Duid(new byte[] { 0x00, 0x01, 0x02, 0x03 });
Console.WriteLine(duid.Length); // 4
```

#### `Span` (.NET 8+)

```csharp
public ReadOnlySpan<byte> Span { get; }
```

Returns the DUID bytes as a read-only span. Provides zero-allocation, stack-friendly access to the underlying byte data.

```csharp
var duid = new Duid(new byte[] { 0x00, 0x01, 0x02, 0x03 });
ReadOnlySpan<byte> span = duid.Span;
Console.WriteLine(span.Length);        // 4
Console.WriteLine(span[0]);           // 0
Console.WriteLine(span.SequenceEqual(new byte[] { 0x00, 0x01, 0x02, 0x03 })); // True
```

#### `Memory` (.NET 8+)

```csharp
public ReadOnlyMemory<byte> Memory { get; }
```

Returns the DUID bytes as a read-only memory region. Useful for interop scenarios where `ReadOnlySpan<byte>` cannot be used (e.g., async operations, `IAsyncEnumerable`).

```csharp
var duid = new Duid(new byte[] { 0x00, 0x01, 0x02, 0x03 });
ReadOnlyMemory<byte> memory = duid.Memory;
Console.WriteLine(memory.Length); // 4
```

---

### Methods

#### `GetBytes()`

```csharp
public IReadOnlyCollection<byte> GetBytes()
```

Returns a read-only view of the underlying bytes. The runtime type is `ReadOnlyCollection<byte>`, preventing callers from casting and mutating the DUID's internal state.

```csharp
var duid = new Duid(new byte[] { 0x00, 0x01, 0x02, 0x03 });
var bytes = duid.GetBytes();
```

#### `ToString()` / `ToString(string)` / `ToString(string, IFormatProvider)`

Converts the DUID to a colon-delimited uppercase hex string by default. The single-argument `ToString(string)` overload is a convenience that delegates to the two-argument form with a `null` format provider. Supports these format strings:

| Format | Description | Example |
|--------|-------------|---------|
| `null`, `""`, `":"`, `"U:"` | Uppercase, colon delimited | `00:01:A2:B3` |
| `"U-"` or `"-"` | Uppercase, dash delimited | `00-01-A2-B3` |
| `"U"` | Uppercase, no delimiter | `0001A2B3` |
| `"L:"` | Lowercase, colon delimited | `00:01:a2:b3` |
| `"L-"` | Lowercase, dash delimited | `00-01-a2-b3` |
| `"L"` | Lowercase, no delimiter | `0001a2b3` |

```csharp
var duid = new Duid(new byte[] { 0x00, 0x01, 0xA2, 0xB3 });

Console.WriteLine(duid.ToString());       // 00:01:A2:B3
Console.WriteLine(duid.ToString("L-", null)); // 00-01-a2-b3
Console.WriteLine($"{duid:L}");           // 0001a2b3  (composite formatting)
```

Throws `FormatException` for unrecognized format strings (e.g. `"X"`, `"Ux"`, `"L "`).

#### `Parse(string)` / `TryParse(string, out Duid)`

```csharp
public static Duid Parse(string duidString)
public static bool TryParse(string duidString, out Duid duid)
```

Parses a hex string into a DUID. Accepts these formats:
- Colon-delimited octets: `01:02:A3:B4`
- Dash-delimited octets: `01-02-A3-B4`
- Space-delimited octets: `01 02 A3 B4`
- Undelimited octets: `0102A3B4`
- Leading-zero-omitted pairs: `1:2:A3:B4` (delimiter required)

Input is trimmed before parsing; casing is ignored. Mixed delimiters (e.g. `01:02-A3:B4`) are rejected.

```csharp
var a = Duid.Parse("00:01:A2:b3");
var b = Duid.Parse("00-01-A2-b3");
var c = Duid.Parse("0001A2b3");
var d = Duid.Parse(" 00:01:A2:B3 ");  // whitespace trimmed

if (Duid.TryParse("GG:HH:II", out var result))
{
    // not reached — invalid hex chars
}
```

`Parse` throws `ArgumentException` on failure. `TryParse` returns `false` and sets the out parameter to `null`.

#### `IParsable<Duid>` — `Parse(string, IFormatProvider)` / `TryParse(string, IFormatProvider, out Duid)`

```csharp
public static Duid Parse(string s, IFormatProvider provider)
public static bool TryParse(string s, IFormatProvider provider, out Duid result)
```

Required by the `IParsable<Duid>` interface (.NET 7+). The `provider` parameter is unused — these methods delegate directly to the non-provider overloads.

```csharp
// generic parsing via IParsable<T>
var duid = IParsable<Duid>.Parse("00:01:A2:B3", null);
```

#### `Equals(Duid)` / `Equals(object)`

```csharp
public bool Equals(Duid other)
public override bool Equals(object obj)
```

Byte-wise equality — two DUIDs are equal when they have the same length and identical byte values at every position.

```csharp
var a = new Duid(new byte[] { 0x00, 0x01, 0x02 });
var b = new Duid(new byte[] { 0x00, 0x01, 0x02 });
var c = new Duid(new byte[] { 0x00, 0x01, 0xFF });

Console.WriteLine(a.Equals(b)); // True
Console.WriteLine(a.Equals(c)); // False
Console.WriteLine(a.Equals("not a duid")); // False (no exception)
```

#### `GetHashCode()`

```csharp
public override int GetHashCode()
```

Lazily computed on first access and cached for the lifetime of the instance. Uses `HashCode.Add(byte)` for each byte, ensuring deterministic results across all target frameworks. Stable across serialization/deserialization boundaries.

```csharp
var duid = new Duid(new byte[] { 0x00, 0x01, 0x02 });
var hash = duid.GetHashCode();  // computed once, cached
var same = duid.GetHashCode();  // returns cached value
```

#### `CompareTo(Duid)` / `CompareTo(object)`

```csharp
public int CompareTo(Duid other)
public int CompareTo(object obj)
```

Sorts by length first, then by unsigned byte value. A non-null DUID is always greater than `null`.

```csharp
var shortDuid = new Duid(new byte[] { 0x00, 0x01, 0x02 });        // 3 bytes
var longDuid  = new Duid(new byte[] { 0x00, 0x01, 0x02, 0x03 });  // 4 bytes

Console.WriteLine(shortDuid.CompareTo(longDuid));  // -1  (shorter sorts first)
Console.WriteLine(longDuid.CompareTo(null));       //  1  (non-null > null)
```

`CompareTo(object)` throws `ArgumentException` if the argument is not a `Duid`.

---

### Operators

```csharp
public static bool operator ==(Duid lhs, Duid rhs)
public static bool operator !=(Duid lhs, Duid rhs)
public static bool operator  <(Duid lhs, Duid rhs)
public static bool operator  >(Duid lhs, Duid rhs)
public static bool operator <=(Duid lhs, Duid rhs)
public static bool operator >=(Duid lhs, Duid rhs)
```

`null` is treated as less than any non-`null` value for ordering operators (`<`, `>`, `<=`, `>=`). Both `null` operands are equal for `==`.

```csharp
Duid a = new Duid(new byte[] { 0x00, 0x01, 0x02 });
Duid b = new Duid(new byte[] { 0x00, 0x01, 0x02 });
Duid c = null;

Console.WriteLine(a == b); // True
Console.WriteLine(a == c); // False
Console.WriteLine(c == c); // True (both null)
Console.WriteLine(a > c);  // True (non-null > null)
```

---

### Serialization

Implements `ISerializable`. Uses a versioned payload (`SerializableVersion = 0`) for forward compatibility. Deserialization properly initializes the lazy hash code cache.

```csharp
// BinaryFormatter example (net48 only)
var duid = new Duid(new byte[] { 0x00, 0x01, 0x02 });
var formatter = new BinaryFormatter();
using var stream = new MemoryStream();
formatter.Serialize(stream, duid);
stream.Position = 0;
var deserialized = (Duid)formatter.Deserialize(stream);
```

> **Note:** `BinaryFormatter` is removed in .NET 9+. Serialization testing targets `net48` only.

---

### Span Formatting (.NET 8+)

#### `ISpanFormattable`

```csharp
bool ISpanFormattable.TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider provider)
```

Formats the DUID into a character span buffer. Returns `false` if the destination is too small. Uses the same format strings as `ToString(string, IFormatProvider)`.

```csharp
var duid = new Duid(new byte[] { 0x00, 0x01, 0xA2, 0xB3 });
Span<char> buffer = stackalloc char[11];
bool success = ((ISpanFormattable)duid).TryFormat(buffer, out int written, "U:", null);
// success = true, written = 11, buffer = "00:01:A2:B3"
```

#### `IUtf8SpanFormattable`

```csharp
bool IUtf8SpanFormattable.TryFormat(Span<byte> utf8Destination, out int bytesWritten, ReadOnlySpan<char> format, IFormatProvider provider)
```

Formats the DUID directly into a UTF-8 byte buffer. Returns `false` if the destination is too small. Avoids intermediate string allocations when the output is consumed as UTF-8 bytes.

```csharp
var duid = new Duid(new byte[] { 0x00, 0x01, 0xA2, 0xB3 });
Span<byte> utf8Buffer = stackalloc byte[11];
bool success = ((IUtf8SpanFormattable)duid).TryFormat(utf8Buffer, out int written, "U:", null);
// success = true, written = 11, utf8Buffer contains UTF-8 bytes of "00:01:A2:B3"
```

---

## `NetDuid.DuidType` (enum)

Maps the 2-byte big-endian type code to a human-readable label. This is a heuristic — the DUID content may not match the type it claims to be.

```csharp
Console.WriteLine(duid.Type); // DuidType.LinkLayerPlusTime

switch (duid.Type)
{
    case DuidType.Uuid:
        break;
    case DuidType.Undefined:
        break;
}
```

| Value | Type Code | RFC | Description |
|-------|-----------|-----|-------------|
| `Undefined` | `0x0000`, `0x0005`–`0xFFFF` | — | Unknown or unregistered type |
| `LinkLayerPlusTime` | `0x0001` | RFC 8415 | Link-layer address plus time |
| `VendorAssigned` | `0x0002` | RFC 8415 | Vendor-assigned unique ID based on Enterprise Number |
| `LinkLayer` | `0x0003` | RFC 8415 | Link-layer address |
| `Uuid` | `0x0004` | RFC 6355 | Universally Unique Identifier (UUID) |
