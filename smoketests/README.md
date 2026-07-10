# NetDuid Smoke Tests

Smoke tests that prove the **packed NuGet package** works for every supported target framework.

## Why these exist

The main `src/NetDuid.Tests/` suite references the library via a *project reference*, which
bypasses NuGet asset-selection entirely.  A project reference can never surface:

- Incorrect `lib/<tfm>/` folder layout in the `.nupkg`
- Wrong TFM asset being chosen by a consumer (e.g. `net9.0` falling back to `netstandard2.0`
  when a dedicated `net9.0` asset should exist)
- BCL types referenced by a C# language feature that are missing on an older runtime
- Missing or mismatched transitive dependencies declared in the `.nuspec`

Smoke tests fix this by **packing the library first**, then referencing it as a NuGet package
from a separate consumer project — exactly as an end-user would.

## Structure

```
smoketests/
├── nuget.config              # adds smoketests/feed/ as a local NuGet source
├── run-smoke-tests.sh        # Linux / macOS (net8.0, net9.0, net10.0)
├── run-smoke-tests.ps1       # Windows     (net48, net8.0, net9.0, net10.0)
└── SmokeTests/
    ├── SmokeTests.csproj     # multi-target console app; PackageReference to NetDuid
    └── Program.cs            # exercises the full public API surface
```

`smoketests/feed/` is git-ignored; the run scripts create it on demand.

## Running locally

### Linux / macOS

```bash
# From the repo root:
./smoketests/run-smoke-tests.sh

# With an explicit version:
./smoketests/run-smoke-tests.sh 4.0.0-preview
```

Covers **net8.0**, **net9.0**, **net10.0**.

### Windows (PowerShell)

```powershell
# From the repo root:
.\smoketests\run-smoke-tests.ps1

# With an explicit version:
.\smoketests\run-smoke-tests.ps1 -Version 4.0.0-preview
```

Covers **net48** (→ `netstandard2.0` asset), **net8.0**, **net9.0**, **net10.0**.

> **net48 requires Windows.**  The `netstandard2.0` asset is what any .NET Framework
> or legacy .NET Standard consumer receives; net48 is the proxy TFM that validates it.

## What is tested

`Program.cs` exercises the entire public API surface against a real runtime:

| Area | Checks |
|---|---|
| `Duid` (all types) | Create from bytes, type detection for all 5 `DuidType` values |
| Parse / TryParse | Colon, dash, space, undelimited, leading-zero-omitted, whitespace trim, case-insensitive, mixed-delimiter rejection, null/empty rejection |
| `IFormattable` / `ToString` | All 6 format strings (`null`, `""`, `"U:"`, `"U-"`, `"U"`, `"L:"`, `"L-"`, `"L"`), invalid format throws `FormatException` |
| `IParsable<Duid>` (NET7+) | `Parse(string, IFormatProvider)`, `TryParse(string, IFormatProvider, out Duid)` success and failure |
| Equality | Same bytes, different bytes, null, `==`, `!=`, `GetHashCode` |
| Comparison | `CompareTo` (shorter, longer, equal), `<`, `>`, `<=`, `>=` |
| Null operator semantics | `null == null`, `null != non-null`, `non-null > null`, `null < non-null`, `>=`, `<=` with null |
| `GetBytes` | Content, read-only snapshot semantics |
| Edge cases | 3-byte minimum, 130-byte maximum, constructor rejects null/empty/2-byte/131-byte |

## How the package reference works

The consumer project (`SmokeTests.csproj`) does **not** use a project reference.
Instead it declares:

```xml
<PackageReference Include="NetDuid" Version="$(NetDuidVersion)"
                  Condition="'$(NetDuidVersion)' != '0.0.0-placeholder'" />
```

This indirection is necessary because there is no published NetDuid package on nuget.org.
The workflow is always the same:

1. **Pack** `src/NetDuid/NetDuid.csproj` with a synthetic version (e.g. `99.0.0-smoke`)
   into `smoketests/feed/`.
2. **Restore** passing `-p:NetDuidVersion=99.0.0-smoke` — the condition matches, NuGet
   resolves the package from the local feed.
3. **Build and run** the consumer project.

### The placeholder version

When no `NetDuidVersion` is passed, the csproj defaults to `0.0.0-placeholder`.
The `Condition` on the `PackageReference` excludes it entirely in this state,
preventing restore failures when the local feed does not exist.

### org-level dependency submission

The repository has an org-wide `submit-nuget` action that scans every `.csproj` and
runs `dotnet restore` to submit dependency graphs.  Because `0.0.0-placeholder` causes
the `PackageReference` to be omitted, the restore succeeds without needing the local
feed — no manual exclusions required.

## CI

Smoke tests run in the independent **`smoke-test.yml`** workflow (`.github/workflows/smoke-test.yml`),
triggered on every PR and push to `main`:

- **ubuntu-latest** → net8.0, net9.0, net10.0
- **windows-latest** → net48 (netstandard2.0 asset validation)

The workflow packs the library, restores the consumer against the local feed, builds
per-TFM, and runs the console app.  Any non-zero exit code from the consumer fails the job.

## Developer notes

### Opening in an IDE

Opening `smoketests/SmokeTests/SmokeTests.csproj` directly in an IDE will show build
errors — the `PackageReference` is conditioned away and the `Duid` type is not available.
This is expected.  Always use the run scripts or the CI workflow, which first pack the
library into the local feed.

If you need IDE support for editing, open the whole repo solution (`src/`) instead.

### Adding a new test

All smoke tests are top-level `Check()` calls in `Program.cs`.  Add a new block:

```csharp
Check(
    "Description of what is tested",
    () =>
    {
        // Arrange
        var duid = new Duid(new byte[] { 0x00, 0x04, 0x01, 0x02 });
        // Assert
        Require(duid.Type == DuidType.Uuid, "expected UUID type");
    }
);
```

The project uses no test framework — it is a self-checking console app.  `Check()`
catches exceptions, prints `[PASS]` / `[FAIL]`, and exits with code 1 on any failure.

### Troubleshooting

| Symptom | Likely cause |
|---|---|
| `NU1301: The local source '.../feed' doesn't exist` | Restore without first packing.  Run the script or CI workflow. |
| `NU1101: Unable to find package NetDuid` | The feed is populated with a different version than `NetDuidVersion` expects.  Check the version passed to `pack` and `restore` match. |
| `CS0246: The type or namespace 'Duid' could not be found` | The `PackageReference` was conditioned away (placeholder version).  Pass `-p:NetDuidVersion=...` with a real version. |
| `Failed to restore` on unrelated checkout | Org-level `submit-nuget` ran restore without a local feed.  Verify the placeholder condition is intact. |
