# Move-ImageStoreFile
Moves the file record specified and the pointed file to the target folder and directory.

To rename a file, use [Rename-ImageStoreFile](RenameFile.md) instead.

Alias: MoveFile

# Parameters
|Name|Type|Description|Optional|
|---|---|---|---|
|File|[ImageStoreFile](../../type/ImageStoreFile.md)|File to be moved|No(*1)|
|Id|Guid|Id of the file record to be moved|No(*1)|
|NewFolder|[ImageStoreFolder](../../type/ImageStoreFolder.md)|Target folder|No(*2)|
|NewFolderId|Guid|Id of the folder record as the target|No(*2)|
|NewPath|string|Relevant path to the directory of the target. Filename is not included. Set to empty string if you want to put the file to the root directory of the target folder. Set to ```null``` to preserve the old directory structure in target folder. Default value is ```null```.|Yes|
|SkipFile|*switch*|Skips the file moving operating in file system.|-|
|SkipReturn|*switch*|Returns null instead of the record of the moved file.|-|
|OverrideSealedFolder|*switch*|Overrides the IsSealed mark of the folder.|-|

From Pipeline: File, Id

*1: One and only one parameter within this group should be provided.

*2: One and only one parameter within this group should be provided.

# Return
The record of the moved file.

Type: [ImageStoreFile](../../type/ImageStoreFile.md)

Or, ```null```, when SkipReturn is present.

# See also
  * [Concept: File](../../concept/File.md)
  * [File Cmdlets](../cmdlets.md#file)
