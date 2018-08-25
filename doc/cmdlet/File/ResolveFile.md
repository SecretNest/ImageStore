# Resolve-ImageStoreFile
Gets the full path of the file specified.

Alias: ResolveFile

# Parameters
|Name|Type|Description|Optional|
|---|---|---|---|
|File|[ImageStoreFile](../../type/ImageStoreFile.md)|File entity to be displayed|No(*1)|
|Id|Guid|Id of the record to be displayed|No(*1)|

From Pipeline: File, Id

*1: One and only one parameter within this group should be provided.

# Return
Full path of the file.

Type: string

# See also
  * [Concept: File](../../concept/File.md)
  * [File Cmdlets](../cmdlets.md#file)