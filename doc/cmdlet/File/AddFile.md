# Add-ImageStoreFile
Creates and adds a file record to database.

Alias: AddFile

# Parameters
|Name|Type|Description|Optional|
|---|---|---|---|
|Folder|[ImageStoreFolder](../../type/ImageStoreFolder.md)|Folder which contains this file|No|
|Path|string|Path to the directory which contains this file, relevant to the root directory of the folder. Set to empty string when the file is located in the root directory of the folder. File name should not be included.|No|
|FileName|string|File name without extension. Set to empty string when the file doesn't have a name.|No|
|Extension|[ImageStoreExtension](../../type/ImageStoreExtension.md)|Extension of the file|No|
|OverrideSealedFolder|*switch*|Overrides the IsSealed mark of the folder.|-|

# Return
The record of newly created file.

Type: [ImageStoreFile](../../type/ImageStoreFile.md)

# See also
  * [Concept: File](../../concept/File.md)
  * [File Cmdlets](../cmdlets.md#file)