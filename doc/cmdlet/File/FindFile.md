# Find-ImageStoreFile
Finds the record related to the file by folder, path and file name.

Alias: FindFile

# Parameters
|Name|Type|Description|Optional|
|---|---|---|---|
|FolderId|Guid|Record id of the folder to find the file within|No|
|Path|string|Relevant path of the file to find. Set to empty string when the file is located in the root directory of the folder.|No|
|FileName|string|File name without extension. Set to empty string when the file doesn't have a name.|No|
|ExtensionId|Guid|Record id of the extension of this file|No|

From Pipeline: FileName

# Return
The record of the file.

Type: [ImageStoreFile](../../type/ImageStoreFile.md)

Or, ```null```, when no related record can be found.

# See also
  * [Concept: File](../../concept/File.md)
  * [File Cmdlets](../cmdlets.md#file)