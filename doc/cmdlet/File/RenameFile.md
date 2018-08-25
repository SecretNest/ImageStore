# Rename-ImageStoreFile
Renames the file record specified and the pointed file.

To move a file, use [Move-ImageStoreFile](MoveFile.md) instead.

Alias: RenameFile

# Parameters
|Name|Type|Description|Optional|
|---|---|---|---|
|File|[ImageStoreFile](../../type/ImageStoreFile.md)|File to be renamed|No("1)|
|Id|Guid|Id of the file record to be renamed|No(*1)|
|NewFileName|string|New name of the file. Extension is not included. Set to empty string to remove the filename.|No|
|NewExtension|[ImageStoreExtension](../../type/ImageStoreExtension.md)|New extension|No(*2)|
|NewExtension|Id|Guid|Id of the extension record as the new one|No(*2)|
|SkipFile|*switch*|Skips the file renaming operating in file system.|-|
|SkipReturn|*switch*|Returns null instead of the record of the renamed file.|-|
|OverrideSealedFolder|*switch*|Overrides the IsSealed mark of the folder.|-|

From Pipeline: File, Id

*1: One and only one parameter within this group should be provided.

*2: One and only one parameter within this group should be provided.

# Return
The record of the renamed file.

Type: [ImageStoreFile](../../type/ImageStoreFile.md)

Or, ```null```, when SkipReturn is present.

# See also
  * [Concept: File](../../concept/File.md)
  * [File Cmdlets](../cmdlets.md#file)