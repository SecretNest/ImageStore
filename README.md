# ImageStore
Image deduplication tool, optimized for large amount of pictures and CG libraries.

This tool is built as PowerShell cmdlets, providing user a flexible way to deal with large amount of image files (~1 million).
Specially, it is optimized for CG libraries storing by allowing user to suppress the comparing among files in the same folder.

# Image Hashing Arithmetic
This tool use a [forked version](https://github.com/scegg/phash) of [priHash](https://github.com/pgrho/phash), which added methods optimized for ImageStore calling.
priHash is a C# Implementation of pHash (http://phash.org). Based on phash-0.9.4 for Windows.
In this tool, [difference degree](doc/concept/DifferenceDegree.md) is based on the calculation of priHash.

# Use Module in PowerShell
To use any module from dll in PowerShell, you just need 3 steps:
1. Start PowerShell. Usually it is placed as ```C:\windows\System32\windowspowershell\v1.0\powershell.exe``` in Windows.
2. Use command ```Import-Module``` to load module from dll file. The parameter of this command is the path of the dll file.  
```Import-Module C:\ImageStore.dll``` will load the module file named as ImageStore.dll and placed in the root folder of drive C.  
```Import-Module .\ImageStore.dll``` will load the module file named as ImageStore.dll from the current directory.
3. Use cmdlets.

Also, you can combine the step 1 and 2 as one, by passing the command as a parameter while starting PowerShell.  
```C:\windows\System32\windowspowershell\v1.0\powershell.exe -noexit -command "Import-Module .\ImageStore.dll"```

## Enable Information and Verbose Output
By default, the information and verbose level output will be silenced. ImageStore will report key information as informational output and progress updates as verbose one. Thus, enabling information output is highly recommended. If ImageStore is dealing with large amount of files, turning on verbose output is advised.

  * Turn on information output: ```$InformationPreference="Continue"```
  * Turn on verbose output: ```$VerbosePreference="Continue"```
  * Turn off information output: ```$InformationPreference="SilentlyContinue"```, or simply restart PowerShell.
  * Turn off verbose output: ```$VerbosePreference="SilentlyContinue"```, or simply restart PowerShell.

These setting will not be preserved among instances of PowerShell. Every time the PowerShell started, there are set as ```
"SilentlyContinue"```.

# Database
ImageStore need SqlServer 2017 for hosting database. Each project should have a dedicated database.

All editions of SqlServer 2017 on Windows are supported, including LocalDb, Express, Standard, Enterprise and Developer. Linux versions are not tested.

Attached database file mode is supported and recommended.

To install SqlServer 2017, access [Sql Server 2017 Homepage](https://www.microsoft.com/en-us/sql-server/sql-server-2017) and download the edition you desired. LocalDb or Express edition will be a good choice IMHO.

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
|[ImageStoreIgnoredDirectory](doc/type/ImageStoreIgnoredDirectory.md)|Excludes a directory from a [folder](doc/concept/Folder.md).|
|[ImageStoreExtension](doc/type/ImageStoreExtension.md)|Represents an [extension](doc/concept/extension.md), a kind of file.|
|[ImageStoreFile](doc/type/ImageStoreFile.md)|Represents a [file](doc/concept/File.md) stored in a [folder](doc/concept/Folder.md).|
|[ImageStoreSameFile](doc/type/ImageStoreSameFile.md)|Represents a [record](doc/concept/SameFile.md) that a file detected to be the same as at least one other file.|
|[ImageStoreSimilarFile](doc/type/ImageStoreSimilarFile.md)|Represents a [similar relationship](doc/concept/SimilarFile.md) between two image files.|

# Workthough

