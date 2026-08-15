# CLAUDE.md

Guidance for Claude Code when working in this repository.

## What this project is

ImageStore is an image-deduplication toolkit shipped as a **PowerShell binary module** (a single
`ImageStore.dll`). It is not an application with an entry point — users load it with
`Import-Module .\ImageStore.dll` and drive it through cmdlets. Some cmdlets open WinForms dialogs
for interactive review of duplicate/similar images.

It is optimized for very large libraries (~1 million files) and for CG-library layouts, where
files inside the same directory are often intentional variants that should not be compared with
each other.

Duplicate detection has two independent pipelines:

- **Same File** — byte-identical files, detected via SHA-1 (`SameFile` table).
- **Similar File** — visually similar images, detected via pHash
  ([Shipwreck.Phash](https://github.com/pgrho/phash)) (`SimilarFile` table).

## Build and toolchain

| | |
|---|---|
| Target | `net10.0-windows`, `OutputType=Library` |
| UI | Windows Forms (`UseWindowsForms`) |
| Project style | SDK-style csproj (`Microsoft.NET.Sdk`) |
| Build | `dotnet build` / `dotnet publish ImageStore/ImageStore.csproj -c Release` |
| Host | **PowerShell 7.6+** only. 7.6 is the first release built on .NET 10; Windows PowerShell 5.1 cannot load the module at all. |
| Root namespace | `SecretNest.ImageStore` (assembly name `ImageStore`) |
| Tests | None. There is no test project. |
| CI | `.github/workflows/build-and-release.yml` — builds on `windows-latest`, publishes a release on every push to `master`, builds without publishing on pull requests. |

**It builds on Linux**, which is worth knowing because the WinForms target suggests otherwise:

```
dotnet build   ImageStore/ImageStore.csproj -c Release -p:EnableWindowsTargeting=true
dotnet publish ImageStore/ImageStore.csproj -c Release -o /tmp/out -p:EnableWindowsTargeting=true
```

That compiles and produces a complete publish tree, so compile errors, analyzer errors and the
exact package contents can all be checked locally. It cannot be *run* there — use it to verify
the build, then rely on CI or a Windows box for anything behavioural.

Dependencies come from NuGet; nothing is committed to the repo:

| Package | Note |
|---|---|
| `Shipwreck.Phash`, `Shipwreck.Phash.Bitmaps` | Upstream. A fork used to be vendored here for extra `GetCrossCorrelation` overloads; upstream 0.5.0 has them. |
| `Microsoft.Data.SqlClient` | Replaces `System.Data.SqlClient`, which has no .NET 10 story. |
| `System.Management.Automation` | `ExcludeAssets="runtime;native"` — see below. |

**`ExcludeAssets` on `System.Management.Automation` must keep both `runtime` and `native`.**
The PowerShell host supplies that assembly at run time, and the package's managed dll is a
reference assembly with no method bodies. Excluding only `runtime` still drags the host's *native*
payload into the output — `pwrshplugin.dll`, `PowerShell.Core.Instrumentation.dll`, and
`libpsl-native.so`/`.dylib` for Linux and macOS, in a Windows-only module. That mistake costs
~36 files and is invisible unless the package is opened.

Use `dotnet publish`, not `dotnet build`, when producing something to ship: the module needs its
full dependency closure, `ImageStore.deps.json`, and `runtimes/win-*/native/Microsoft.Data.SqlClient.SNI.dll`.
A package missing SNI loads fine and then fails on the first database connection.

## Repository layout

```
ImageStore.sln          Solution; also carries every doc/*.md as SolutionItems
ImageStore/             The only real project
  Database/             Open/Close/Compress database cmdlets
  DatabaseShared/       Connection singleton + SQL building helpers
  Folder/               Folder entity, helper, cmdlets
  IgnoredDirectory/     Directory-exclusion entity, helper, cmdlets
  Extension/            File-extension entity, helper, cmdlets
  File/                 File entity, hashing (Measure*), file-system operations
  SameFile/             SHA-1 duplicate detection + WinForms review UI
  SimilarFile/          pHash similarity detection + WinForms review UI + thumbprint cache
Database/               CreateDatabase.txt (full schema script) + an empty DataStore.mdf/.ldf
doc/                    User documentation: concept/, cmdlet/, type/, walkthrough/
```

Source directories map one-to-one onto documentation directories and onto the domain concepts.
Keep that alignment when adding features.

## Architecture

### One class per cmdlet

Every cmdlet is its own file named `<Verb><Noun>Cmdlet.cs`, deriving from
`System.Management.Automation.Cmdlet` (not `PSCmdlet`). Cross-cmdlet logic lives in a
`<Area>Helper.cs` static class in the same directory. Entity classes are named
`ImageStore<Thing>.cs` and are the public surface returned to PowerShell.

### Global mutable state (important)

Two static fields hold state for the whole PowerShell session:

- `DatabaseConnection.Current` (`DatabaseShared/DatabaseConnection.cs`) — a single open
  `SqlConnection`, set by `Open-ImageStoreDatabase`, torn down by `Close-ImageStoreDatabase`.
  Accessing it before opening throws `InvalidOperationException("Database is not specified.")`.
  Every cmdlet starts with `var connection = DatabaseConnection.Current;`.
- `LoadImageHelper.cachePath` (`SimilarFile/LoadImageHelper.cs`) — the thumbprint cache directory,
  set by `Set-ImageStoreThumbprintCacheFolder` (resolved **relative to the assembly folder**),
  cleared by `Clear-ImageStoreThumbprintCacheFolder`. `null` means caching is disabled.

Consequences to respect: there is exactly one connection, so nothing may run two DB-touching
cmdlets concurrently; and several code paths create `#temp` tables, which only work because that
one connection is reused for the whole operation. Neither setting survives a PowerShell restart.

### ADO.NET conventions

Raw `Microsoft.Data.SqlClient` throughout — no ORM, no EF, no async. Only the namespace differs
from the old `System.Data.SqlClient`; the type names are the same, and `SqlDbType` is still
`System.Data.SqlDbType`. The consistent shape is:

```csharp
var connection = DatabaseConnection.Current;
using (var command = new SqlCommand("Select [Id],[Extension] from [Extension]"))
{
    command.Connection = connection;
    command.CommandTimeout = 0;                 // long-running by design; do not remove
    command.Parameters.Add(new SqlParameter("@Id", SqlDbType.UniqueIdentifier) { Value = id });
    using (var reader = command.ExecuteReader(CommandBehavior.SequentialAccess))
    {
        while (reader.Read()) { /* read by ordinal */ }
        reader.Close();
    }
}
```

- `CommandTimeout = 0` (infinite) is deliberate — comparison passes can run for hours or days.
- Columns are read **by ordinal**, so changing the `Select` list means changing the indices too.
- Nullable columns go through `DBNullableReader.ConvertFromReferenceType<T>` /
  `ConvertFromValueType<T>`.
- **Never concatenate user values into SQL.** Dynamic filters are built with
  `DatabaseShared/WhereCauseBuilder.cs`, which appends parameterized predicates
  (`AddStringComparingCause`, `AddIntComparingCause`, `AddBitComparingCause`,
  `AddUniqueIdentifierComparingCause`, `AddRealComparingCause`, `AddIntInRangeCause`) and emits
  the final clause via `ToFullWhereCommand()`. `LIKE` values are escaped by
  `SqlServerLikeValueBuilder`.
- The name "Cause" is a long-standing misspelling of "Clause" in this codebase. Match the existing
  spelling rather than renaming.

### PowerShell surface conventions

```csharp
[Cmdlet(VerbsCommon.Search, "ImageStoreExtension")]   // noun always prefixed "ImageStore"
[Alias("SearchExtension")]                            // short alias without the prefix
[OutputType(typeof(List<ImageStoreExtension>))]
public class SearchExtensionCmdlet : Cmdlet
{
    [Parameter(ValueFromPipelineByPropertyName = true, Position = 0, ValueFromPipeline = true)]
    public string Extension { get; set; }
```

- Nouns are `ImageStore<Thing>`; every cmdlet also gets a short `[Alias]`.
- Parameters use explicit `Position` values, `ValueFromPipelineByPropertyName = true` almost
  always, and `ValueFromPipeline = true` on the primary parameter. Optional filters are nullable
  (`bool?`, `float?`, `Guid?`) so "not specified" is distinguishable from a default.
- Search-style cmdlets take a `StringPropertyComparingModes` flags enum
  (`Equals | Contains | StartsWith | EndsWith`) next to each string parameter.
- Output channels: `WriteObject` for results; `WriteInformation(message, new[] { "Tag", ... })`
  for key milestones; `WriteVerbose` for progress; `WriteWarning` for recoverable oddities;
  `WriteError` / `ThrowTerminatingError` with an `ErrorRecord` whose id reads like
  `"ImageStore Add Folder"`. Information and verbose output are silent unless the user sets
  `$InformationPreference` / `$VerbosePreference`, so keep the important facts at information level.
- `Search-*` cmdlets emit a single `List<T>`; `Get-*`/`Find-*` emit one entity.

### Long-running and parallel work

`Measure-ImageStoreFiles`, `Compare-ImageStoreSimilarFiles` and the resolve paths are the hot
spots. They follow a producer/consumer shape: worker threads or `Parallel.ForEach` compute, results
flow through a `BlockingCollection<T>` / `ConcurrentBag<ErrorRecord>`, and the cmdlet thread drains
the collection to call `WriteVerbose`/`WriteWarning`. This matters — **PowerShell's `Write*` methods
are only legal on the cmdlet's own thread**, so never call them from a worker.

`Compare-ImageStoreSimilarFiles` loads the entire file table (hashes, paths, existing similar pairs)
into nested dictionaries before comparing. Memory footprint is a real constraint at ~1M files; be
careful about adding per-file state to those structures.

### WinForms interaction

`Select-ImageStoreSameFile` and `Resolve-ImageStoreSimilarFiles` call
`Application.EnableVisualStyles()` in `BeginProcessing` and then `ShowDialog()`. There is no
message loop of the module's own — the dialogs rely on the host thread being STA. `pwsh` is STA by
default on Windows (restored in 7.0-preview.3 precisely so WinForms and WPF work), but VS Code's
PowerShell Integrated Console is MTA, where these two cmdlets misbehave. The heaviest UI is
`SimilarFile/SimilarFileInGroupManager.cs`; `DoubleBufferedDataGridView` / `DoubleBufferedListView`
exist to keep large lists from flickering.

## Data model

Schema lives in `Database/CreateDatabase.txt` (SQL Server 2017; LocalDB/Express are fine, attached
`.mdf` mode is the recommended setup). All primary keys are `uniqueidentifier` generated in C# with
`Guid.NewGuid()`, never by the database.

| Table | Notes |
|---|---|
| `Folder` | Root of an image library. `CompareImageWith` controls comparison scope. `IsSealed` marks read-only libraries. |
| `IgnoredDirectory` | Per-folder exclusions, optionally recursive. |
| `Extension` | One row per file extension; `IsImage` and `Ignored` drive what gets hashed. |
| `File` | `Path` + `FileName` + `ExtensionId` relative to the folder; `ImageHash binary(40)` (pHash), `Sha1Hash binary(20)`, `FileState`, `ImageComparedThreshold`. |
| `SameFile` | Rows grouped by shared `Sha1Hash`; `IsIgnored` hides a row from review. |
| `SimilarFile` | Pair `File1Id`/`File2Id` with `DifferenceDegree` and `IgnoredMode`. |

Enums that must stay in sync with the stored `int` values:

- `FileState` — `New = 0`, `NotImage = 1`, `NotReadable = 2`, `SizeZero = 254`, `Computed = 255`.
- `CompareImageWith` — `All = 0`, `FilesInOtherDirectories = 1`, `FilesInOtherFolders = 2`.
  This is the CG-library optimization: skip comparisons inside the same directory or same folder.
- `IgnoredMode` — `Effective = 0`, `HiddenButConnected = 1`, `HiddenAndDisconnected = 2`.
  "Connected" vs "Disconnected" decides whether the pair still links two files into one group
  during the next grouping pass; it is not merely a display flag.
- `StringPropertyComparingModes` — `[Flags]`: `Equals = 1`, `Contains = 2`, `StartsWith = 4`,
  `EndsWith = 8`.

Entities expose a public typed enum property plus an `internal int <Name>Code` shim used for DB
round-tripping. Keep both when adding an enum-backed column.

**Difference degree** is `1 - crossCorrelation` from pHash, range `[0, 1]`, smaller = more similar,
and it is **not linear** — above ~0.03 most pairs are unrelated to a human eye. `File.ImageComparedThreshold`
records the largest threshold a file has already been compared at, so a later run with a smaller
threshold does no redundant work; a run with a larger threshold re-compares. `Reset-ImageStoreSimilarFiles`
wipes both the pair table and those thresholds.

**Thumbprint cache** is not in the database — it is a directory of `{fileId:N}.png` files managed by
`LoadImageHelper`, purely a speedup for repeated `Resolve-ImageStoreSimilarFiles` sessions.

## CI and releases

`.github/workflows/build-and-release.yml` runs on `windows-latest` and does
publish → verify → package → release.

**Every push to `master` publishes a real release.** The tag is date-based, `v<yyyy.MM.dd>.<n>`,
where `n` continues from the highest tag already published that day; the date is stamped in
`TAG_TIMEZONE` (China Standard Time), not the runner's UTC clock. Pushing a `v*` tag by hand
publishes under that tag verbatim instead. Because the sequence number is derived from existing
tags, the workflow is serialised with a `concurrency` group — do not remove that.

**A pull request runs the same build but skips publishing** (`if: github.event_name != 'pull_request'`
on the last step) and labels its artifact `pr<n>-<sha>` so it cannot be mistaken for a release.
Use it for anything risky: nothing here can be *run* outside Windows, so CI is the only behavioural
feedback before merging.

Each release carries two assets: `ImageStore-<tag>.zip` (the whole publish tree minus `.pdb`) and
`ImageStore-Database-<tag>.zip` (the empty `.mdf`/`.ldf` and `CreateDatabase.txt`). The database is
deliberately separate — its contents are identical in every release and are only needed once, when
setting up a project. The module archive is copied wholesale rather than as flat `*.dll`, because
`deps.json` and `runtimes/win-*/native/` have to keep their layout.

The "Verify build output" step guards the package in both directions:

- **Nothing missing.** `ImageStore.dll`, `ImageStore.deps.json`, both `Shipwreck.Phash*` dlls,
  `Microsoft.Data.SqlClient.dll`, and — checked recursively — the native SNI library.
- **Nothing extra.** No `System.Management.Automation.dll`, `pwrshplugin.dll`,
  `PowerShell.Core.Instrumentation.dll`, `libpsl-native.*` or `getfilesiginforedist.dll`. These
  belong to the PowerShell host and are kept out by `ExcludeAssets="runtime;native"`; the check is
  the net for a regression, since an oversized package otherwise looks perfectly healthy. Both
  halves of this have already caught a real mistake — v2026.08.15.1 shipped the reference assembly,
  and the native payload leaked in during the .NET 10 upgrade.

The workflow does not touch `AssemblyInfo.cs`: the dll stays at `1.0.0.0` and the version lives
only in the tag, release title, and asset name.

Tag names are attacker-controllable text, so they are passed into the PowerShell steps as
environment variables rather than spliced in with `${{ }}`, and `gh` arguments are splatted as an
array instead of built into a command string. Keep it that way when editing.

## Documentation

`doc/` is user-facing and part of the deliverable. Adding or changing a cmdlet means updating:

1. `doc/cmdlet/<Area>/<VerbNoun>.md` — title, one-line description, `Alias:`, a Parameters table
   (Name | Type | Description | Optional), a `FromPipeline:` line, a `# Return` section naming the
   type, and a `# See also` list.
2. `doc/cmdlet/cmdlets.md` — the index table for that area.
3. `ImageStore.sln` — new doc files are registered as `SolutionItems` under the matching solution
   folder.
4. `README.md` — only if a concept or entity type is added.

Documentation is written in English and cross-links are relative paths. All links currently resolve
with exact casing, and every `doc/**/*.md` file is registered in `ImageStore.sln`; keep both true.
This is easy to break silently, because Windows filesystems are case-insensitive — a link like
`file/UpdateFile.md` or `../concept/database.md` works locally while being broken on GitHub and on
case-sensitive hosts. Cmdlet doc files are named after the cmdlet without the `ImageStore` prefix
(`ResetSimilarFiles.md` for `Reset-ImageStoreSimilarFiles`), not after the C# class name.

## Working notes

- Typical user pipeline, useful for reasoning about ordering:
  `Open-ImageStoreDatabase` → `Add-ImageStoreFolder` → `Add-ImageStoreExtension` →
  `Sync-ImageStoreFolder` → `Measure-ImageStoreFiles` → `Compare-ImageStoreSameFiles` /
  `Compare-ImageStoreSimilarFiles` → `Select-ImageStoreSameFile` / `Resolve-ImageStoreSimilarFiles`
  → `Remove-ImageStoreFile`. `doc/walkthrough/case.md` is the worked example.
- Cmdlets in `File/` (`Remove-ImageStoreFile`, `Remove-ImageStoreDirectory`, `Move-ImageStoreFile`,
  `Rename-ImageStoreFile`, `Clear-ImageStoreEmptyFolders`) **delete and move real files on disk**,
  not just rows. Treat changes there as destructive-path changes and be correspondingly careful.
- Commit messages in this repo are short and lower-key ("bug fix", "add tooltip",
  "Add Multi process support"). Match that tone.
- `.remember/` is local session scratch state, not project content.
