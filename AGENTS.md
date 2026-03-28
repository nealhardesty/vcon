# C# / .NET Cursor Rules

## Think Hard
**CRITICAL: Think deeply before taking action**
- Pause before editing; ask when requirements are unclear—do not assume
- Compare approaches and trade-offs; if unsure, discuss with the user
- Clarity before code: summarize understanding before implementing

## Workflow and Planning
**CRITICAL: Read the project before changing it**
- Use search and file reads to learn existing patterns, references, architecture, and related types—then verify in code, never by guesswork
- Before implementing, consider edge cases, interactions, alternatives, maintainability, and backward compatibility

## Language and Version
- Target current LTS/STS .NET (e.g. 8+) unless the repo pins another TFM; match `README.md` / `DESIGN.md`
- Use modern C# when it fits the language version; prefer BCL over new NuGet packages
- Follow [.NET design guidelines](https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/), `EditorConfig`, and analyzers

## Code Style
- `dotnet format` / IDE + repo `EditorConfig`; consistent indentation and braces
- Naming: public `PascalCase`; private fields `_camelCase` (or repo convention); locals/parameters `camelCase`; constants `PascalCase` or existing `UPPER_SNAKE`—stay consistent
- Short methods, early returns; nullable reference types when enabled—no casual null suppressions

## Project and Namespace Organization
- Clear folders/namespaces by domain (e.g. `Vcon.Core`, `Vcon.UI`); avoid bloated catch-all projects
- Non-public API: `internal` or separate projects; `InternalsVisibleTo` for tests if needed
- Thin hosts (WPF `App`, APIs, workers); file-scoped namespaces if the repo uses them

## Usings
- Group/order per convention; remove unused; `var` vs explicit per project style

## Error Handling
- Log or propagate—never silent swallow; match project pattern (exceptions vs `Result`/etc.)
- Preserve stacks (avoid `throw ex`); `using` / `await using` for disposal
- Validate inputs at boundaries (public API, UI, handlers)

## Documentation
- XML `///` on public APIs when the repo documents them; complete sentences; comments for non-obvious logic
- Examples via small tests or docs, consistent with the repo

## Testing
- Automate tests for new behavior and regressions; one framework (xUnit/NUnit/MSTest)—do not add another without cause
- Clear names; parameterized tests for many cases; async tests without `.Result`/`.Wait()` deadlocks
- Stress or parallel runs where shared state might race

## Dependencies
- SDK-style projects, NuGet `PackageReference`; lock files or `Directory.Packages.props` when used—pin/centralize versions
- Minimize packages; justify non-trivial ones; document global tools with pinned versions when stability matters
- Periodically check outdated/vulnerable packages; keep restore/build free of ignored warnings

## Makefile
**CRITICAL: Root `Makefile` required** — primary shortcut over raw `dotnet`; call **`dotnet`** under the hood.

Include: `build` (`dotnet build`), `test` (`dotnet test`), `run` (main app), `clean` (`bin`/`obj`, `dotnet clean` per doc), `lint` (analyzers / `dotnet format --verify` / `-warnaserror` per policy), `fmt` (`dotnet format`), `restore`, **`help`** (lists targets), `.PHONY` where needed. Add extras (e.g. `publish`, `docker`, `benchmark`) as needed.

## Versioning
**CRITICAL: One authoritative version** (e.g. `Directory.Build.props` and/or `VersionInfo.cs` in sync with the build).

- **SemVer** `MAJOR.MINOR.PATCH`: major = breaking; minor = compatible features; patch = compatible fixes
- CLI/UI: `--version` / `-v` or About; **Makefile:** `version`, `version-increment`, `release` (tag/build/package per project)
- Align releases with **git tags**; update **`CHANGELOG.md`** on version bumps
- **CRITICAL:** bump patch (or follow documented release rules) per change set—match automation or written policy

## Code Quality
**CRITICAL: DRY** — no duplicated logic; on copy-paste, stop and consolidate into shared types/services/libraries; multiple hosts share one implementation (hosts = DI/config only). Small interfaces; composition/DI over deep inheritance. `async`/`await` for I/O; `async void` only for events. `IDisposable`/`IAsyncDisposable`; avoid static mutable state; `CancellationToken` on async/long work.

## Concurrency
- Prefer async + thread-safe abstractions; protect shared state (`lock`, `SemaphoreSlim`, concurrent collections)—document invariants
- No fire-and-forget `Task`s without logging/cancellation; careful `TaskCompletionSource`
- No `.Result`/`.Wait()` on deadlock-prone paths; test under parallel load when races are plausible

## Performance
- Profile before micro-opts (`BenchmarkDotNet` if used); cut hot-path allocations when measured
- `Span<T>`/`Memory<T>` for buffers/interop; `ArrayPool<T>` only when justified

## Security
- No hardcoded secrets—User Secrets, env vars, or secure stores
- Validate/sanitize input; encode output appropriately; parameterized DB access
- Avoid `unsafe` except reviewed cases; `RandomNumberGenerator` for crypto, not `System.Random`

## Documentation, README, Git, and CHANGELOG
**CHANGELOG.md — CRITICAL:** Update **before** every commit/PR for **every** change by humans or agents (features, fixes, config, deps, docs, refactors, breaking). Clear entries; group by date/version as the file does. Never skip.

**README.md:** Keep aligned with `DESIGN.md` and real behavior. After significant changes, document features, fixes, config, deps; add Changelog/Updates section if used; refresh install/build when workflows change.

**Git:** Clear messages; focused commits; sensible branch names.

## Project-Specific Guidelines
- Personal project: ship working code over perfection, but stay readable and maintainable
- Important architecture in **README.md** or linked docs; keep the solution navigable
