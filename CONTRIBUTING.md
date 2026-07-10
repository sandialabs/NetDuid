# Contributing to NetDuid

## Prerequisites

- [.NET SDK](https://dotnet.microsoft.com/download) (version pinned in `global.json`)
- (optional) The dev container in `.devcontainer/` if you'd rather not install the SDK locally
- (optional) [Python](https://www.python.org/) and `pre-commit` for the Python-based pre-commit hooks

## Build & test

Run everything from the repository root:

```bash
dotnet restore
dotnet build
dotnet test --verbosity normal
```

To test a single target framework:

```bash
dotnet test --framework net8.0
```

> **Note:** `net48` tests only work on Windows. CI runs per-target-framework on Ubuntu and all target frameworks on Windows.

## Project layout

- `src/` — the `NetDuid` library and its xUnit test project
- `src/NetDuid.Benchmarks/` — BenchmarkDotNet performance benchmarks
- `smoketests/` — a standalone consumer project that validates the packed NuGet package works correctly across all target frameworks
- `.devcontainer/` — dev container configuration
- `.scratch/` — internal working documents (not shipped)

The solution file is `NetDuid.slnx` at the repository root.

## Pull requests

1. Fork and branch from `main`.
2. Keep changes focused — one logical change per PR.
3. Add or update tests for behavior changes.
4. Run formatting before pushing (or let the pre-commit hooks do it).
5. Fill out the PR template; link any related issue.

## Code style

Formatting and analyzer rules are enforced via `.editorconfig` and
`Directory.Build.props`. CI will fail the build on analyzer warnings treated
as errors — run `dotnet build` locally to catch these before pushing.

To run formatting manually:

```bash
dotnet tool restore
dotnet format style; dotnet format analyzers; dotnet csharpier format .
```

These commands may be called independently, but order may matter.

## Pre-commit hooks

Two systems run independently on commit:

1. **[Husky.Net](https://alirezanet.github.io/Husky.Net/)** — lints and stages `.cs` files via `dotnet format` and [CSharpier](https://csharpier.com/). Disable with `HUSKY=0`.
2. **[pre-commit](https://pre-commit.com/)** (Python-based) — runs `codespell`, `markdownlint`, trailing-whitespace and end-of-file checks, YAML validation, and CSharpier. Install with `pre-commit install`.

## Central package management

NuGet package versions are centralized in `Directory.Packages.props`. When adding or updating a dependency, edit that file — not the individual `.csproj` files. After changing dependencies, run `dotnet restore` to regenerate the committed `packages.lock.json` files.

## Smoke tests

`smoketests/` exercises the **packed NuGet package** (not a project reference). Requires building and packing first. Run via `smoketests/run-smoke-tests.sh` (Linux/macOS) or `smoketests/run-smoke-tests.ps1` (Windows).

## Benchmarks

`src/NetDuid.Benchmarks/` is a BenchmarkDotNet project targeting `net10.0`. Run via `src/run-benchmarks.sh` (Linux/macOS) or `src/run-benchmarks.ps1` (Windows). Results land in `src/results/<timestamp>/`.
