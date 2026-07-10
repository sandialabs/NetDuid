# NetDuid Remediation Plan

**Date:** 2026-07-05
**Source:** `.scratch/code-review-handoff.md`
**Status:** Ready for execution

---

## Executive Summary

This plan addresses 24 findings from the code review, organized into 5 batches that can be parallelized using git worktrees. Batches 1-4 have no file overlap and can execute simultaneously. Batch 5 depends on Batch 2 (both touch `Duid.Factory.cs`).

**Deferred items** (separate documents or out of scope):
- M5: Struct conversion → `.scratch/future-design-duid-struct-conversion.md`
- M6, M7, L2: Documentation-only, already documented
- T2: ISerializable NET48 gating is correct behavior
- C7: Empty benchmark directory kept for future use

---

## Batching Strategy

### Batch 1: Configuration & CI Fixes
**Worktree:** `fix/config-and-ci`
**Approach:** Mechanical edits, no TDD
**Files:** `NetDuid.csproj`, `Directory.Build.props`, `NetDuid.Tests.csproj`, `build.yml`, `publish.yml`, `CONTRIBUTING.md`, `AGENTS.md`, `.runsettings`, `smoketests/SmokeTests/Program.cs`

| ID | Issue | Fix | Verification |
|---|---|---|---|
| H1 | Unconditional `Microsoft.Bcl.HashCode` | Remove line 85, change line 84 condition to `netstandard2.0` | `dotnet restore` succeeds, `dotnet build` succeeds for all TFMs |
| H3 | Dead `TargetFrameworks` in Directory.Build.props | Remove line 3 (or comment explaining it's overridden) | `dotnet build` succeeds |
| H4 | Double-encoded XML entity in test csproj | Fix `&amp;amp;` → `&amp;` on line 24 | `dotnet build` succeeds, verify assembly metadata |
| C1 | CI build doesn't treat warnings as errors | Add `--configuration Release` to `build.yml` build step | CI passes |
| C2 | Publish workflow doesn't run tests | Add `needs: [build]` to `publish.yml` | CI passes |
| C3 | Wrong `global.json` path in CONTRIBUTING.md | Change `src/global.json` → `global.json` | Manual verification |
| C4 | `.runsettings` possibly unused | Add comment explaining it's for local dev, or remove if unused | Manual verification |
| C5 | Coverage only for net10.0 | Expand `build.yml` to collect coverage for net8.0 and net9.0 on Ubuntu | CI produces coverage for all Ubuntu TFMs |
| C6 | Stale smoke test info in AGENTS.md | Remove claim about non-existent namespaces | Manual verification |
| C8 | Smoke test hex string mismatch | Fix `llTimeStringNoDelim` to match `llTimeBytes` (14 bytes, not 15) | Smoke tests pass |

**Execution order:** All items are independent, can be done in any order within the batch.

---

### Batch 2: Trivial Production Fixes
**Worktree:** `fix/trivial-production`
**Approach:** Readability improvements, comment out old code and add improved versions alongside
**Files:** `Duid.IComparable.cs`, `DuidType.cs`, `Duid.Factory.cs`, `Duid.IFormattable.cs`, `DuidRegexSource.cs`

| ID | Issue | Fix | Verification |
|---|---|---|---|
| M1 | `<remark>` tag | Change to `<remarks>` on line 13 of `Duid.IComparable.cs` | `dotnet build` succeeds, XML doc validates |
| L1 | Typo in DuidType XML doc | Change "of RFC6355" → "or RFC6355" on line 9 of `DuidType.cs` | `dotnet build` succeeds |
| M2 | Obscure `HexCharToUpper` | Comment out lines 167-175, add improved version with clear comment: `// Improved: use char.ToUpperInvariant for clarity` | Existing Parse/TryParse tests pass |
| M3 | Division/modulo in IFormattable | Comment out lines 148-149, add bit shift version: `@byte >> 4` and `@byte & 0x0F` with comment: `// Improved: bit shifts are idiomatic for nibble extraction` | Existing IFormattable tests pass |
| L3 | `DuidRegexSource` naming | Rename class to `DuidRegexPatterns` (clearer intent), update all references in `Duid.Factory.cs` | `dotnet build` succeeds, Parse/TryParse tests pass |

**Execution order:** M2 and M3 both touch `Duid.Factory.cs`, so do them together. L3 requires updating references in `Duid.Factory.cs`, so do it after M2/M3.

**Note:** For M2 and M3, the old code is commented out (not deleted) so both versions are visible for comparison. After verification, the commented code can be removed in a follow-up commit.

---

### Batch 3: Equals Allocation & Lazy HashCode
**Worktree:** `fix/equals-and-hashcode`
**Approach:** TDD — existing tests verify correctness, add allocation test
**Files:** `Duid.IEquatable.cs`, `Duid.cs`, `Duid.ISerializable.cs`

| ID | Issue | Fix | Verification |
|---|---|---|---|
| H2 | `Equals(Duid)` allocates via `GetBytes()` | Change line 38-39 in `Duid.IEquatable.cs` to access `other._duidBytes` directly instead of `other.GetBytes()` | Existing `IEquatable` tests pass |
| X2 | Inconsistent field access pattern | Same fix as H2 (makes `Equals` consistent with `CompareTo`) | Existing `IComparable` tests pass |
| M4 | `_lazyHashCode` cross-file dependency | Extract initialization into private helper method `InitializeLazyHashCode()` called by both constructors (main and deserialization) | Existing `GetHashCode` tests pass |

**TDD approach:**
1. Run existing tests to establish baseline (all should pass)
2. Optionally add a benchmark or allocation test to verify the improvement (e.g., using `BenchmarkDotNet` or manual allocation counting)
3. Refactor `Equals(Duid)` to use `other._duidBytes` directly
4. Extract `_lazyHashCode` initialization into helper method
5. Run tests again to verify no regression

**Execution order:** H2/X2 first (simple refactor), then M4 (helper method extraction).

**Breaking changes:** None. This is a behavior-preserving optimization.

---

### Batch 4: Test Improvements
**Worktree:** `fix/test-improvements`
**Approach:** TDD — add new tests, fix existing test issues
**Files:** `Duid.IEquatableTests.cs`, `DuidTests.cs`, `Duid.OperatorsTests.cs`

| ID | Issue | Fix | Verification |
|---|---|---|---|
| T1 | Misnamed test | Rename `Equal_Object_NotDuid_Throws_ArgumentException_Test` → `Equal_Object_NotDuid_Returns_False_Test` | Test still passes |
| T3 | No test with non-array `IEnumerable<byte>` | Add test in `DuidTests.cs` that constructs `Duid` from `List<byte>` and verifies correctness | New test passes |
| T4 | Operator tests derive expected from `CompareTo` | Refactor test data generation in `Duid.OperatorsTests.cs` to use independently computed expected values (e.g., manual byte comparison) | Existing operator tests still pass |
| T5 | No hash code stability/inequality test | Add tests in `Duid.IEquatableTests.cs`: (a) same instance returns same hash code on multiple calls, (b) different DUIDs produce different hash codes (probabilistic, test a few known-different cases) | New tests pass |

**TDD approach:**
1. Write failing tests first (T3, T5 — new tests)
2. Verify they fail (or pass if the implementation is already correct)
3. For T4, refactor test data generation to be independent of `CompareTo`
4. For T1, rename the test method
5. Run all tests to verify

**Execution order:** T1 (rename) first, then T3/T5 (new tests), then T4 (refactor test data).

**Breaking changes:** None. Test-only changes.

---

### Batch 5: Parse Performance Optimization
**Worktree:** `fix/parse-performance`
**Depends on:** Batch 2 (both touch `Duid.Factory.cs`)
**Approach:** TDD + benchmarks
**Files:** `Duid.Factory.cs`

| ID | Issue | Fix | Verification |
|---|---|---|---|
| M8 | `DelimitedStringToBytes` over-allocates | Refactor to use `List<byte>` or `Span<byte>` with stackalloc for small inputs, eliminating the over-allocation and `Buffer.BlockCopy` | Existing Parse/TryParse tests pass, benchmark shows improvement |

**TDD approach:**
1. Run existing Parse/TryParse tests to establish baseline
2. Optionally add a benchmark using `BenchmarkDotNet` (once the benchmark project is scaffolded) to measure allocation and throughput
3. Refactor `DelimitedStringToBytes` to use a more efficient approach:
   - Option A: Use `List<byte>` (simpler, still allocates but less over-allocation)
   - Option B: Use `Span<byte>` with `stackalloc` for small inputs (≤128 bytes), fall back to array for larger
4. Run tests again to verify no regression
5. Compare benchmark results

**Execution order:** Must wait for Batch 2 to merge (both touch `Duid.Factory.cs`).

**Breaking changes:** None. This is a behavior-preserving optimization.

---

## Git Worktree Parallelization Strategy

### Phase 1: Parallel Execution (Batches 1-4)

Create 4 worktrees, one for each batch:

```bash
# Batch 1: Configuration & CI
git worktree add ../netduid-config-fixes fix/config-and-ci
# Batch 2: Trivial production
git worktree add ../netduid-trivial-fixes fix/trivial-production
# Batch 3: Equals & hashcode
git worktree add ../netduid-equals-fixes fix/equals-and-hashcode
# Batch 4: Test improvements
git worktree add ../netduid-test-fixes fix/test-improvements
```

**File overlap analysis:**
- Batch 1: Config files, CI files, docs, smoke tests
- Batch 2: `Duid.IComparable.cs`, `DuidType.cs`, `Duid.Factory.cs`, `Duid.IFormattable.cs`, `DuidRegexSource.cs`
- Batch 3: `Duid.IEquatable.cs`, `Duid.cs`, `Duid.ISerializable.cs`
- Batch 4: Test files only

**No overlap** — all 4 batches can execute simultaneously.

### Phase 2: Sequential Execution (Batch 5)

After Batch 2 merges:

```bash
# Batch 5: Parse performance (depends on Batch 2)
git worktree add ../netduid-perf-fixes fix/parse-performance
```

**File overlap:** Batch 5 touches `Duid.Factory.cs`, which Batch 2 also touches. Must wait for Batch 2 to merge first.

### Merge Strategy

After all batches complete:

1. **Option A: Sequential merge** (safer)
   - Merge Batch 1 → main
   - Merge Batch 2 → main
   - Merge Batch 3 → main
   - Merge Batch 4 → main
   - Merge Batch 5 → main (after Batch 2)
   - Resolve any merge conflicts as they arise

2. **Option B: Octopus merge** (faster, riskier)
   - Merge all 5 branches into main in one operation
   - Risk: conflicts may be harder to resolve
   - Only viable if all batches are well-tested and independent

**Recommendation:** Use Option A (sequential merge) for safety. Each merge should be followed by a full test run.

---

## Verification Strategy

### Per-Batch Verification

| Batch | Build | Test | Format | Coverage |
|---|---|---|---|---|
| 1 | `dotnet build --configuration Release` | `dotnet test --configuration Release` | N/A | N/A |
| 2 | `dotnet build` | `dotnet test` | `dotnet format style && dotnet format analyzers && dotnet csharpier format .` | N/A |
| 3 | `dotnet build` | `dotnet test` | `dotnet format style && dotnet format analyzers && dotnet csharpier format .` | N/A |
| 4 | `dotnet build` | `dotnet test` | `dotnet format style && dotnet format analyzers && dotnet csharpier format .` | N/A |
| 5 | `dotnet build` | `dotnet test` | `dotnet format style && dotnet format analyzers && dotnet csharpier format .` | Optional benchmark |

### Post-Merge Verification

After all batches merge:

```bash
# Full build and test
dotnet build --configuration Release
dotnet test --configuration Release

# Format check
dotnet tool restore
dotnet format style --verify-no-changes
dotnet format analyzers --verify-no-changes
dotnet csharpier format --check .

# Smoke tests
./smoketests/run-smoke-tests.sh
```

---

## Risk Assessment

| Batch | Risk | Mitigation |
|---|---|---|
| 1 | Low — config changes only | Full build + test after each change |
| 2 | Low — readability improvements, old code commented | Existing tests verify behavior preservation |
| 3 | Medium — changes to core equality/comparison logic | TDD approach, existing tests verify correctness |
| 4 | Low — test-only changes | New tests verify themselves |
| 5 | Medium — performance optimization in parsing | TDD approach, existing tests verify behavior preservation |

---

## Suggested Skills for Implementation

| Batch | Skills to invoke |
|---|---|
| 1 | `msbuild-antipatterns`, `directory-build-organization`, `property-patterns`, `github-actions-writer` |
| 2 | `gen-dotnet-docs-comments` |
| 3 | `test-driven-development`, `analyzing-dotnet-performance` |
| 4 | `test-driven-development`, `code-testing-agent` |
| 5 | `test-driven-development`, `analyzing-dotnet-performance`, `microbenchmarking` |

---

## Summary

- **5 batches** of work, organized by file overlap and dependencies
- **4 batches can run in parallel** (Batches 1-4)
- **1 batch is sequential** (Batch 5, depends on Batch 2)
- **TDD approach** for Batches 3-5 (production code changes)
- **Mechanical edits** for Batches 1-2 (no TDD needed)
- **Deferred items** documented in separate files

**Total estimated effort:** 2-3 days for a single developer, or 1 day with 4 parallel worktrees.
