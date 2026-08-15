# ImageStore
Image deduplication tool, optimized for large amount of pictures and CG libraries.

This tool is built as PowerShell cmdlets, providing user a flexible way to deal with large amount of image files (~1 million).
Specially, it is optimized for CG libraries storing by allowing user to suppress the comparing among files in the same folder.

# Requirements
  * Windows.
  * [PowerShell 7.6](https://github.com/PowerShell/PowerShell/releases) or later. The module targets .NET 10, and 7.6 is the first PowerShell release built on it.
  * [.NET 10 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/10.0), which the windows opened by some cmdlets need. If ```Import-Module``` fails complaining about a missing framework, this is the one to install.

No database server is needed. A library is a single SQLite file, created by [New-ImageStoreDatabase](doc/cmdlet/Database/NewDatabase.md).

*Note: Windows PowerShell 5.1 — the ```powershell.exe``` that ships with Windows — cannot load this module, because it runs on .NET Framework. Use ```pwsh```. The last release that works under 5.1 is [v2026.08.15.2](https://github.com/SecretNest/ImageStore/releases/tag/v2026.08.15.2).*

# Image Hashing Arithmetic
This tool use [priHash](https://github.com/pgrho/phash), a C# Implementation of pHash (http://phash.org), based on phash-0.9.4 for Windows.
In this tool, [difference degree](doc/concept/DifferenceDegree.md) is based on the calculation of priHash.

# Use Module in PowerShell
To use any module from dll in PowerShell, you just need 3 steps:
1. Start PowerShell by running ```pwsh```.
2. Use command ```Import-Module``` to load module from dll file. The parameter of this command is the path of the dll file.  
```Import-Module C:\ImageStore.dll``` will load the module file named as ImageStore.dll and placed in the root folder of drive C.  
```Import-Module .\ImageStore.dll``` will load the module file named as ImageStore.dll from the current directory.
3. Use [cmdlets](doc/cmdlet/cmdlets.md).

Unpack the whole release archive into one folder and load ImageStore.dll from there. The other files beside it are its dependencies, and the module will not work without them.

Also, you can combine the step 1 and 2 as one, by passing the command as a parameter while starting PowerShell.  
```pwsh -noexit -command "Import-Module .\ImageStore.dll"```

*Note: [Select-ImageStoreSameFile](doc/cmdlet/SameFile/SelectSameFile.md) and [Resolve-ImageStoreSimilarFiles](doc/cmdlet/SimilarFile/ResolveSimilarFiles.md) open windows, which requires the host to run in a single-threaded apartment. ```pwsh``` does so by default, but the PowerShell Integrated Console in Visual Studio Code does not — run those two cmdlets from a real console.*

## Enable Information and Verbose Output
By default, the information and verbose level output will be silenced. ImageStore will report key information as informational output and progress updates as verbose one. Thus, enabling information output is highly recommended. If ImageStore is dealing with large amount of files, turning on verbose output is advised.

  * Turn on information output: ```$InformationPreference="Continue"```
  * Turn on verbose output: ```$VerbosePreference="Continue"```
  * Turn off information output: ```$InformationPreference="SilentlyContinue"```, or simply restart PowerShell.
  * Turn off verbose output: ```$VerbosePreference="SilentlyContinue"```, or simply restart PowerShell.

These setting will not be preserved among instances of PowerShell. Every time the PowerShell started, there are set as ```
"SilentlyContinue"```.

# Database
ImageStore stores everything in one SQLite file. There is nothing to install and no server to run, and each project has its own file.

Create one and start using it:
```
New-ImageStoreDatabase D:\Library\library.db
```
This creates the file, builds the schema and opens it, so no separate [Open-ImageStoreDatabase](doc/cmdlet/Database/OpenDatabase.md) is needed afterwards. Later sessions open the existing file:
```
Open-ImageStoreDatabase D:\Library\library.db
```

**Keep the database file on a local disk.** SQLite relies on file locking, which is unreliable over SMB and other network shares, and write-ahead logging cannot be used there at all. This is safe and normal even when the images themselves live on a NAS: the database stores paths, not image data, so the two do not have to sit together.

## Upgrading from a Sql Server library
Releases before v2.0 kept data in Sql Server. To bring an existing library across, download ```ImageStore-Migrator``` from the [release](https://github.com/SecretNest/ImageStore/releases) and run it once:
```
ImageStore.Migrator --source "server=(LocalDB)\MSSQLLocalDB;AttachDbFilename=D:\DataStore.mdf;Integrated Security=True" --target D:\Library\library.db
```
It copies every record and leaves the Sql Server database untouched, so the old one remains as a fallback. [v2.0](https://github.com/SecretNest/ImageStore/releases/tag/v2.0) is the last release that works against Sql Server.

# Concepts
There are several concepts defined in ImageStore. Reading these docs will help you to understand the system.

|Concept|Description|
| --- | --- |
|[Database](doc/concept/Database.md)|A Sql Server 2017 database to save all records and settings.|
|[Folder](doc/concept/Folder.md)|The root directory of your image library.|
|[Extension](doc/concept/Extension.md)|Extension record for each kind of files.|
|[File](doc/concept/File.md)|File record for each file.|
|[Same File](doc/concept/SameFile.md)|Exactly same files detected by Sha1 hashing.|
|[Similar File](doc/concept/SimilarFile.md)|Similar images detected by pHash algorithm.|
|[Thumbprint Cache](doc/concept/ThumbprintCache.md)|Cache for image thumbprints used in Similar File UI.|
|[Difference Degree](doc/concept/DifferenceDegree.md)|The difference between two image files.|

# Cmdlets
See [Cmdlets](doc/cmdlet/cmdlets.md).

# Entity Types
These types of entities will be used while operating with cmdlets of ImageStore.

|Type|Description|
| --- | --- |
|[ImageStoreFolder](doc/type/ImageStoreFolder.md)|Represents a [folder](doc/concept/Folder.md) for storing image files.|
|[ImageStoreIgnoredDirectory](doc/type/ImageStoreIgnoredDirectory.md)|Represents an exclusion a directory from a [folder](doc/concept/Folder.md).|
|[ImageStoreExtension](doc/type/ImageStoreExtension.md)|Represents an [extension](doc/concept/Extension.md), a kind of file.|
|[ImageStoreFile](doc/type/ImageStoreFile.md)|Represents a [file](doc/concept/File.md) stored in a [folder](doc/concept/Folder.md).|
|[ImageStoreSameFile](doc/type/ImageStoreSameFile.md)|Represents a [record](doc/concept/SameFile.md) that a file detected to be the same as at least one other file.|
|[ImageStoreSimilarFile](doc/type/ImageStoreSimilarFile.md)|Represents a [similar relationship](doc/concept/SimilarFile.md) between two image files.|

# Other Types
These types will be used while operating with cmdlets of ImageStore.

|Type|Class|Description|
| --- | --- | --- |
|[StringPropertyComparingModes](doc/type/StringPropertyComparingModes.md)|enum|The way to use the string provided as search condition.|

# Walkthrough
A [walkthrough](doc/walkthrough/case.md) of use this product.