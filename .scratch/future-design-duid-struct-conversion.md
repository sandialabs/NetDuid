# Future Design: Convert `Duid` from `class` to `readonly record struct`

**Date:** 2026-07-05
**Status:** Deferred — requires separate design discussion and major version bump
**Origin:** Code review finding M5 in `code-review-handoff.md`

---

## Current Design

`Duid` is a `sealed partial class` implementing:
- `IEquatable<Duid>`
- `IComparable<Duid>`, `IComparable`
- `IFormattable`
- `ISerializable`
- `IParsable<Duid>` (on NET7_0_OR_GREATER)

All interface implementations include explicit null checks. The type uses value semantics (equality based on byte contents, not reference identity) but is a reference type.

### Current Implementation Characteristics

- **Null handling:** Every comparison, equality, and operator method checks for null
- **Field storage:** `_duidBytes` (byte array), `Type` (property), `_lazyHashCode` (Lazy<int>)
- **Partial class structure:** Split across 8 files by interface/concern
- **Constructors:** Public constructor from `IEnumerable<byte>`, private deserialization constructor
- **Operators:** `==`, `!=`, `<`, `<=`, `>`, `>=` all handle null operands

---

## Proposed Conversion: `readonly record struct`

### Benefits

1. **Eliminate null checks:** Structs cannot be null (unless `Nullable<Duid>`). All null-check boilerplate in equality, comparison, and operators disappears.

2. **Automatic value equality:** `record struct` provides synthesized `Equals`, `GetHashCode`, and `==`/`!=` operators based on field values. Could eliminate custom implementations in `Duid.IEquatable.cs` and `Duid.Operators.cs`.

3. **Performance:**
   - No heap allocation for `Duid` instances (stack-only)
   - No null-check overhead in comparisons
   - Better cache locality
   - `GetHashCode()` could be computed eagerly in constructor instead of lazily

4. **Reduced code:**
   - Remove null checks from `CompareTo`, `Equals`, all 6 operators
   - Potentially remove custom `Equals`/`GetHashCode` implementations (use synthesized versions)
   - Remove `operator ==` and `operator !=` if record struct synthesis is sufficient

5. **Immutability:** `readonly struct` enforces immutability at the type level. Current `_duidBytes` is `readonly` but the array contents are mutable (though the constructor copies the input).

### Breaking Changes

#### 1. **Null semantics (MAJOR)**

**Before:**
```csharp
Duid d = null;  // Valid
if (d == null) { ... }  // Valid
Duid? nullable = null;  // Valid (reference type)
```

**After:**
```csharp
Duid d = null;  // Compile error
Duid? nullable = null;  // Valid (Nullable<Duid>)
if (nullable is null) { ... }  // Valid
```

**Impact:** Any code that checks `d == null` or `d is null` will break. Any code that passes `null` as a `Duid` parameter will break. This is a **source-level breaking change**.

**Binary compatibility:** Existing compiled assemblies that pass `null` will throw `NullReferenceException` at runtime when the struct is dereferenced.

#### 2. **Default value (MAJOR)**

**Before:**
```csharp
Duid d;  // d is null
Duid[] arr = new Duid[10];  // All elements are null
```

**After:**
```csharp
Duid d;  // d is default(Duid) with _duidBytes = null, Type = Undefined
Duid[] arr = new Duid[10];  // All elements are default(Duid)
```

**Impact:** Code that relies on uninitialized `Duid` being `null` will break. Arrays of `Duid` will no longer contain nulls.

#### 3. **Boxing behavior (MODERATE)**

**Before:** `Duid` is a reference type. Casting to `object` does not box.

**After:** `Duid` is a value type. Casting to `object` boxes the struct (allocates on heap).

**Impact:** Performance regression if `Duid` is frequently cast to `object` (e.g., stored in non-generic collections, used with reflection). However, this is unlikely for a DUID type.

#### 4. **Serialization (MAJOR)**

**Current:** Implements `ISerializable` with custom `GetObjectData` and deserialization constructor.

**After:** `ISerializable` is still supported on structs, but:
- Binary serialization of structs has different semantics
- The deserialization constructor must be adjusted (structs cannot have parameterless constructors, but `ISerializable` deserialization uses a special constructor)
- Need to verify `BinaryFormatter` works correctly with `readonly record struct`

**Impact:** Existing serialized data may not deserialize correctly. Need to test round-trip compatibility.

#### 5. **Operator behavior with null (MODERATE)**

**Before:**
```csharp
Duid a = null;
Duid b = new Duid(bytes);
bool result = a < b;  // false (null is less than non-null)
```

**After:**
```csharp
Duid? a = null;
Duid b = new Duid(bytes);
bool result = a < b;  // Compile error: cannot compare Nullable<Duid> with Duid
bool result2 = a.Value < b;  // Throws InvalidOperationException if a is null
```

**Impact:** All comparison operators need to be rethought for `Nullable<Duid>`. The current "null is less than any non-null value" semantics cannot be expressed with operator overloads on structs.

#### 6. **Record struct equality synthesis (MODERATE)**

**Current:** Custom `Equals(Duid)` compares `_duidBytes` using `SequenceEqual`.

**After:** `record struct` synthesizes `Equals` based on all fields. If we keep `_duidBytes`, `Type`, and `_lazyHashCode` as fields, the synthesized equality will compare all three.

**Problem:** `_lazyHashCode` is a `Lazy<int>` which is a reference type. Two `Duid` instances with the same bytes but different `_lazyHashCode` instances will not be equal under synthesized equality.

**Solution:** Mark `_lazyHashCode` with `[field: NonSerialized]` or compute hash code eagerly. Or implement custom `Equals` to exclude `_lazyHashCode`.

#### 7. **`IParsable<Duid>` and `ISerializable` on structs (LOW)**

Both interfaces are supported on structs, but:
- `IParsable<T>.TryParse` must return `bool` and output `T` (works fine)
- `ISerializable` deserialization constructor must be `private` (works fine)

**Impact:** Minimal, but need to verify both interfaces work correctly with `readonly record struct`.

#### 8. **Partial class structure (LOW)**

**Current:** 8 partial class files.

**After:** Can still use partial struct, but the syntax is `partial struct` not `partial class`. Need to update all file headers.

**Impact:** Mechanical change, but affects all 8 files.

---

## Migration Strategy

### Option 1: Major version bump (breaking change)

- Convert `Duid` to `readonly record struct`
- Update all null checks in tests and smoke tests
- Update serialization tests
- Document breaking changes in release notes
- Publish as v2.0.0

### Option 2: Introduce new type alongside (non-breaking)

- Keep `Duid` as `class`
- Introduce `DuidStruct` as `readonly record struct`
- Deprecate `Duid` class
- Provide conversion methods: `Duid.ToStruct()` and `DuidStruct.ToClass()`
- Migrate consumers over time
- Eventually remove `Duid` class in v3.0.0

### Option 3: Hybrid approach (complex)

- Convert `Duid` to `readonly record struct`
- Provide extension methods for null-like semantics: `Duid?` with custom operators
- This is complex and may not be worth the effort

---

## Open Questions

1. **Should `Type` be a field or computed property?**
   - Current: Property set in constructor
   - Struct: Could be computed on-demand from `_duidBytes[0..1]`
   - Trade-off: Memory vs. computation

2. **Should hash code be eager or lazy?**
   - Current: Lazy (computed on first `GetHashCode()` call)
   - Struct: Could compute in constructor (no allocation overhead for `Lazy<T>`)
   - Trade-off: Constructor cost vs. repeated computation

3. **Should we keep `ISerializable`?**
   - BinaryFormatter is deprecated and discouraged
   - Struct serialization has different semantics
   - Consider removing `ISerializable` in v2.0.0

4. **Should we keep all operator overloads?**
   - `<`, `<=`, `>`, `>=` with null semantics are awkward for structs
   - Consider removing comparison operators and requiring explicit `CompareTo()`
   - Or provide operators only for non-nullable `Duid`

5. **Should `_duidBytes` be a fixed-size buffer?**
   - RFC 8415 says DUIDs are 3-130 bytes
   - Could use `fixed byte _duidBytes[130]` with a length field
   - Trade-off: Stack allocation (fast) vs. wasted space (most DUIDs are much smaller than 130 bytes)

6. **Should we use `ReadOnlyMemory<byte>` instead of `byte[]`?**
   - Would allow zero-copy slicing of larger buffers
   - Trade-off: More complex, may not be needed for typical DUID use cases

---

## Implementation Checklist (for future session)

- [ ] Decide on migration strategy (Option 1, 2, or 3)
- [ ] Resolve open questions above
- [ ] Convert `Duid.cs` to `readonly record struct`
- [ ] Update all 7 partial files (`Duid.*.cs`)
- [ ] Update test project (remove null checks, update serialization tests)
- [ ] Update smoke tests
- [ ] Verify `ISerializable` round-trip works
- [ ] Verify `IParsable<Duid>` works
- [ ] Benchmark performance (allocation, equality, comparison)
- [ ] Update XML documentation
- [ ] Write migration guide for consumers
- [ ] Update `AGENTS.md` with new architecture
- [ ] Bump major version

---

## References

- Original finding: `.scratch/code-review-handoff.md` (finding M5)
- Current implementation: `src/NetDuid/Duid.cs` and partial files
- Test coverage: `src/NetDuid.Tests/DuidTests.cs` and partial files
- RFC 8415: DHCP Unique Identifier specification
- RFC 6355: UUID-based DUID specification
